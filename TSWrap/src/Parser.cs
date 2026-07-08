namespace TreeSitter.Wrap;

using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using TreeSitter.Bind;

/// <summary>
/// ts_parser_parse 的 read 回调委托：返回从 <paramref name="byteIndex"/> 起可用的字节；
/// 返回空 span 表示 EOF。<paramref name="position"/> 为该字节对应的行列位置。
/// 返回的 span 仅需在本次回调返回期间有效（TSWrap 内部立即拷贝到内部缓冲）。
/// </summary>
public delegate ReadOnlySpan<byte> TsInputRead(uint byteIndex, Point position);

/// <summary>
/// 封装 tree-sitter 的 TSParser，提供源代码解析功能。
/// </summary>
public sealed class Parser : IDisposable
{
	private readonly ParserHandle _handle;

	/// <summary>创建新的解析器。</summary>
	public Parser()
	{
		var ptr = TSApi.ts_parser_new();
		if (ptr == IntPtr.Zero)
			throw new TreeSitterException("Failed to create parser.");
		_handle = new ParserHandle(ptr);
	}

	/// <summary>创建解析器并设置语言。语言设置失败时抛 TreeSitterException。</summary>
	public Parser(Language language) : this()
	{
		SetLanguage(language);
	}

	/// <summary>内部访问原生指针。</summary>
	internal IntPtr NativeHandle => _handle.DangerousGetHandle();

	/// <summary>
	/// 设置解析器使用的语言。
	/// 如果语言的 ABI 版本与库不兼容，抛出 TreeSitterException。
	/// </summary>
	public void SetLanguage(Language language)
	{
		if (!TSApi.ts_parser_set_language(NativeHandle, language.NativeHandle))
			throw new TreeSitterException("Failed to set language: incompatible ABI version.");
	}

	/// <summary>
	/// 使用 UTF-16LE 编码解析 C# 字符串。
	/// 零拷贝：直接 pin 住 string 内部 UTF-16LE 字节，经 ts_parser_parse 的 read 回调
	/// 按需喂给 tree-sitter，省去 Encoding.Unicode.GetBytes 的 O(N) 分配与拷贝。
	/// 返回的节点字节偏移 ÷ 2 即为 C# 字符串字符索引。
	/// 公开签名不带 unsafe —— 不安全代码封装在 <see cref="ParseZeroCopyCore"/> 内部，
	/// 调用方无需 unsafe 上下文。
	/// </summary>
	public Tree Parse(string source, Tree? oldTree = null)
	{
		ArgumentNullException.ThrowIfNull(source);
		return ParseZeroCopyCore(source, oldTree);
	}

	private unsafe Tree ParseZeroCopyCore(string source, Tree? oldTree)
	{
		var oldTreePtr = oldTree?._handle?.DangerousGetHandle() ?? IntPtr.Zero;

		fixed (char* c = source)
		{
			// 栈上 payload，同步调用期间有效；read 回调从中取数据指针与总长度。
			ReadPayload payload = new() { Data = (byte*)c, Length = (uint)source.Length * 2u };
			ReadPayload* payloadPtr = &payload;
			delegate* unmanaged[Cdecl]<void*, uint, TSPoint, uint*, byte*> readFn = &ReadCallback;

			var input = new TSInput
			{
				payload = (IntPtr)payloadPtr,
				read = (IntPtr)readFn,
				encoding = TSInputEncoding.TSInputEncodingUTF16LE,
				decode = IntPtr.Zero, // UTF-16LE 走库内置解码器，无需自定义 decode
			};

			var treePtr = TSApi.ts_parser_parse(NativeHandle, oldTreePtr, input);
			if (treePtr == IntPtr.Zero)
				throw new TreeSitterException("Parse returned null tree.");
			return new Tree(treePtr, utf16LEMode: true);
		}
	}

	/// <summary>
	/// 回调版 Parse：直接透传底层 <see cref="TSApi.ts_parser_parse"/>，由调用者自行构造
	/// <see cref="TSInput"/>（read 回调、encoding、decode）。适用于流式/惰性/自定义编码等场景。
	/// 注意：调用者须保证 read 函数指针与 payload 在解析期间存活（如 pin 住底层缓冲、
	/// 保持委托不被 GC 回收）。返回 Tree 的 <see cref="Tree.IsUtf16LE"/> 由 input.encoding
	/// 推导（仅 <see cref="TSInputEncoding.TSInputEncodingUTF16LE"/> 为 true）。
	/// </summary>
	public Tree Parse(TSInput input, Tree? oldTree = null)
	{
		var oldTreePtr = oldTree?._handle?.DangerousGetHandle() ?? IntPtr.Zero;
		var treePtr = TSApi.ts_parser_parse(NativeHandle, oldTreePtr, input);
		if (treePtr == IntPtr.Zero)
			throw new TreeSitterException("Parse returned null tree.");
		bool utf16LEMode = input.encoding == TSInputEncoding.TSInputEncodingUTF16LE;
		return new Tree(treePtr, utf16LEMode);
	}

	/// <summary>
	/// 回调版 Parse（安全）：调用方提供托管 <see cref="TsInputRead"/> 委托，TSWrap 内部
	/// 处理函数指针/pin/委托存活期，调用方无需 unsafe。适用于流式/惰性/任意按需来源。
	/// 返回的 span 由 TSWrap 立即拷贝到内部缓冲，仅本次调用期间有效即可。
	/// <see cref="Tree.IsUtf16LE"/> 由 encoding 推导。不支持
	/// <see cref="TSInputEncoding.TSInputEncodingCustom"/>（需 decode 回调，改用
	/// <see cref="Parse(TSInput, Tree?)"/>）。
	/// </summary>
	public Tree Parse(TsInputRead read, TSInputEncoding encoding, Tree? oldTree = null)
	{
		ArgumentNullException.ThrowIfNull(read);
		if (encoding == TSInputEncoding.TSInputEncodingCustom)
			throw new ArgumentException(
				"Custom encoding requires a decode callback; use Parse(TSInput, ...) instead.", nameof(encoding));

		var oldTreePtr = oldTree?._handle?.DangerousGetHandle() ?? IntPtr.Zero;

		var state = new CallbackState(read);
		GCHandle handle = GCHandle.Alloc(state);
		try {
			var treePtr = ParseCallbackCore(NativeHandle, oldTreePtr, handle, encoding);
			if (state.Error is not null)
				ExceptionDispatchInfo.Throw(state.Error);
			if (treePtr == IntPtr.Zero)
				throw new TreeSitterException("Parse returned null tree.");
			bool utf16LEMode = encoding == TSInputEncoding.TSInputEncodingUTF16LE;
			return new Tree(treePtr, utf16LEMode);
		}
		finally {
			handle.Free();
			state.Dispose();
		}
	}

	/// <summary>使用 UTF-8 字节缓冲区解析源代码。</summary>
	public Tree Parse(ReadOnlySpan<byte> utf8, Tree? oldTree = null)
	{
		var oldTreePtr = oldTree?._handle?.DangerousGetHandle() ?? IntPtr.Zero;
		var treePtr = TSApi.ts_parser_parse_string(NativeHandle, oldTreePtr, utf8, (uint)utf8.Length);
		if (treePtr == IntPtr.Zero)
			throw new TreeSitterException("Parse returned null tree.");
		return new Tree(treePtr, utf16LEMode: false);
	}

	/// <summary>
	/// 尝试解析 C# 字符串，不抛异常。
	/// 解析成功返回 true 并通过 tree 输出参数返回 Tree；失败返回 false。
	/// </summary>
	public bool TryParse(string source, out Tree? tree, Tree? oldTree = null)
	{
		try {
			tree = Parse(source, oldTree);
			return true;
		}
		catch (TreeSitterException) {
			tree = null;
			return false;
		}
	}

	/// <summary>
	/// 尝试解析 UTF-8 字节缓冲区，不抛异常。
	/// 解析成功返回 true 并通过 tree 输出参数返回 Tree；失败返回 false。
	/// </summary>
	public bool TryParse(ReadOnlySpan<byte> utf8, out Tree? tree, Tree? oldTree = null)
	{
		try {
			tree = Parse(utf8, oldTree);
			return true;
		}
		catch (TreeSitterException) {
			tree = null;
			return false;
		}
	}

	/// <summary>
	/// 重置解析器，使下次解析从头开始。
	/// 如果之前因进度回调取消了解析，且不想从断点恢复而是解析新文档，必须先调用此方法。
	/// </summary>
	public void Reset() => TSApi.ts_parser_reset(NativeHandle);

	/// <summary>
	/// 获取或设置解析器解析时包含的文本范围。
	/// get: 拷贝借用指针为 Range[] 返回。
	/// set: 转换 Wrap Range[] 为 TSRange[] 后设置。
	/// </summary>
	public Range[] IncludedRanges {
		get {
			var ptr = TSApi.ts_parser_included_ranges(NativeHandle, out uint count);
			if (ptr == IntPtr.Zero || count == 0) {
				return Array.Empty<Range>();
			}
			var result = new Range[count];
			var nativeSize = Marshal.SizeOf<TSRange>();
			for (uint i = 0; i < count; i++) {
				var native = Marshal.PtrToStructure<TSRange>(ptr + (int)(i * nativeSize));
				result[i] = new Range(native);
			}
			return result;
		}
		set {
			if (value == null || value.Length == 0) {
				TSApi.ts_parser_set_included_ranges(NativeHandle, Array.Empty<TSRange>(), 0);
				return;
			}

			var nativeRanges = new TSRange[value.Length];
			for (int i = 0; i < value.Length; i++) {
				nativeRanges[i] = value[i].ToNative();
			}

			if (!TSApi.ts_parser_set_included_ranges(NativeHandle, nativeRanges, (uint)nativeRanges.Length)) {
				throw new TreeSitterException("Failed to set included ranges.");
			}
		}
	}

	/// <summary>释放解析器及其使用的所有内存。</summary>
	public void Dispose() => _handle.Dispose();

	/// <summary>read 回调的 payload：指向 pinned string 的 UTF-16LE 字节起点与总字节数。</summary>
	[StructLayout(LayoutKind.Sequential)]
	private unsafe struct ReadPayload
	{
		public byte* Data;
		public uint Length;
	}

	/// <summary>
	/// ts_parser_parse 的 read 回调：从 payload 取数据指针，返回 byte_index 处起的可用字节。
	/// 静态、blittable、不向 native 抛异常 —— 满足 UnmanagedCallersOnly 约束。
	/// 用 UnmanagedCallersOnly 而非委托：无 GC 存活期问题（无委托对象可被回收），
	/// 无闭包分配（状态全部经 payload 传入）。
	/// </summary>
	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static unsafe byte* ReadCallback(void* payload, uint byteIndex, TSPoint position, uint* bytesRead)
	{
		var p = (ReadPayload*)payload;
		if (byteIndex >= p->Length) {
			*bytesRead = 0;
			return p->Data; // EOF：0 字节可读，返回任意非空指针
		}
		*bytesRead = p->Length - byteIndex;
		return p->Data + byteIndex;
	}

	// ===== 安全委托回调版 Parse 的内部实现 =====

	private static unsafe IntPtr ParseCallbackCore(IntPtr native, IntPtr oldTreePtr, GCHandle handle, TSInputEncoding encoding)
	{
		var input = new TSInput {
			payload = (IntPtr)handle,
			read = (IntPtr)(delegate* unmanaged[Cdecl]<void*, uint, TSPoint, uint*, byte*>)&ReadCallbackTrampoline,
			encoding = encoding,
			decode = IntPtr.Zero,
		};
		return TSApi.ts_parser_parse(native, oldTreePtr, input);
	}

	/// <summary>安全委托回调的 per-parse 状态：root 住的委托 + 内部 scratch + 捕获的异常。</summary>
	private unsafe sealed class CallbackState : IDisposable
	{
		public readonly TsInputRead Read;
		public byte* Scratch;
		public nuint Capacity;
		public Exception? Error;

		public CallbackState(TsInputRead read)
		{
			Read = read;
			Capacity = 64;
			Scratch = (byte*)NativeMemory.Alloc(Capacity);
		}

		public void EnsureCapacity(nuint needed)
		{
			if (needed <= Capacity) return;
			nuint cap = Capacity;
			while (cap < needed) cap *= 2;
			Scratch = (byte*)NativeMemory.Realloc(Scratch, cap);
			Capacity = cap;
		}

		public void Dispose()
		{
			if (Scratch != null) {
				NativeMemory.Free(Scratch);
				Scratch = null;
				Capacity = 0;
			}
		}
	}

	/// <summary>
	/// 安全委托回调的 native read trampoline：从 GCHandle 取 CallbackState，调用户委托，
	/// 把返回 span 拷进 scratch，返回 scratch 指针。委托抛异常 → 捕获存 state.Error，返回 0 字节
	/// 让 parse 收尾，由公共 Parse 用 ExceptionDispatchInfo 原样重抛。
	/// </summary>
	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static unsafe byte* ReadCallbackTrampoline(void* payload, uint byteIndex, TSPoint position, uint* bytesRead)
	{
		var state = (CallbackState)GCHandle.FromIntPtr((IntPtr)payload).Target!;
		if (state.Error is not null) { *bytesRead = 0; return state.Scratch; }
		try {
			var span = state.Read(byteIndex, new Point(position));
			if (span.IsEmpty) { *bytesRead = 0; return state.Scratch; }
			state.EnsureCapacity((nuint)span.Length);
			span.CopyTo(new Span<byte>(state.Scratch, span.Length));
			*bytesRead = (uint)span.Length;
			return state.Scratch;
		}
		catch (Exception ex) {
			state.Error = ex;
			*bytesRead = 0;
			return state.Scratch;
		}
	}
}
