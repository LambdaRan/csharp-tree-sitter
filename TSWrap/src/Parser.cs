namespace TreeSitter.Wrap;

using System.Runtime.InteropServices;
using System.Text;
using TreeSitter.Bind;

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
    /// 返回的节点字节偏移 ÷ 2 即为 C# 字符串字符索引。
    /// </summary>
    public Tree Parse(string source, Tree? oldTree = null)
    {
        var utf16 = Encoding.Unicode.GetBytes(source);
        var oldTreePtr = oldTree?._handle?.DangerousGetHandle() ?? IntPtr.Zero;
        var treePtr = TSApi.ts_parser_parse_string_encoding(
            NativeHandle, oldTreePtr, utf16, (uint)utf16.Length,
            TSInputEncoding.TSInputEncodingUTF16LE);
        if (treePtr == IntPtr.Zero)
            throw new TreeSitterException("Parse returned null tree.");
        return new Tree(treePtr, utf16LEMode: true);
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
        try
        {
            tree = Parse(source, oldTree);
            return true;
        }
        catch (TreeSitterException)
        {
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
        try
        {
            tree = Parse(utf8, oldTree);
            return true;
        }
        catch (TreeSitterException)
        {
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
    public Range[] IncludedRanges
    {
        get
        {
            var ptr = TSApi.ts_parser_included_ranges(NativeHandle, out uint count);
            if (ptr == IntPtr.Zero || count == 0)
                return Array.Empty<Range>();

            var result = new Range[count];
            var nativeSize = Marshal.SizeOf<TSRange>();
            for (uint i = 0; i < count; i++)
            {
                var native = Marshal.PtrToStructure<TSRange>(ptr + (int)(i * nativeSize));
                result[i] = new Range(native);
            }
            return result;
        }
        set
        {
            if (value == null || value.Length == 0)
            {
                TSApi.ts_parser_set_included_ranges(NativeHandle, Array.Empty<TSRange>(), 0);
                return;
            }

            var nativeRanges = new TSRange[value.Length];
            for (int i = 0; i < value.Length; i++)
                nativeRanges[i] = value[i].ToNative();

            if (!TSApi.ts_parser_set_included_ranges(NativeHandle, nativeRanges, (uint)nativeRanges.Length))
                throw new TreeSitterException("Failed to set included ranges.");
        }
    }

    /// <summary>释放解析器及其使用的所有内存。</summary>
    public void Dispose() => _handle.Dispose();
}
