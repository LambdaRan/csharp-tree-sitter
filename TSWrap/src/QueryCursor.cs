namespace TreeSitter.Wrap;

using System.Runtime.InteropServices;
using TreeSitter.Bind;

/// <summary>
/// 封装 tree-sitter 的 TSQueryCursor，用于在语法树上执行查询并迭代匹配结果。
/// 提供 ref struct 路径（<see cref="NextMatch"/> / <see cref="NextCapture"/>）和
/// IEnumerable 路径（<see cref="Matches"/>）。
/// <para>
/// <b>ref struct 路径</b>：零数组分配，<see cref="QueryMatch"/> 直接持有原生指针，
/// 按需读取每个捕获。原生指针仅在下次迭代调用前有效。
/// </para>
/// <para>
/// <b>IEnumerable 路径</b>：返回 <see cref="QueryMatchOwned"/>（普通 struct），
/// 内部拷贝 captures 为数组，支持 LINQ，可跨迭代安全使用。
/// </para>
/// </summary>
public sealed class QueryCursor : IDisposable
{
	private static readonly int _captureNativeSize = Marshal.SizeOf<TSQueryCapture>();

	private readonly QueryCursorHandle _handle;
	private Query? _activeQuery;
	private Tree? _activeTree;

	/// <summary>创建新的查询游标。</summary>
	public QueryCursor()
	{
		var ptr = TSApi.ts_query_cursor_new();
		if (ptr == IntPtr.Zero)
			throw new TreeSitterException("Failed to create query cursor.");
		_handle = new QueryCursorHandle(ptr);
	}

	/// <summary>内部访问原生指针。</summary>
	private IntPtr NativeHandle => _handle.DangerousGetHandle();

	/// <summary>
	/// 在给定节点上执行查询。保持 query 和 node 的 tree 引用存活，
	/// 防止它们在迭代期间被 GC 回收。
	/// </summary>
	public void Exec(Query query, Node node)
	{
		_activeQuery = query;
		_activeTree = node.Owner;
		TSApi.ts_query_cursor_exec(NativeHandle, query.NativeHandle, node.RawNode);
	}

	// ---- 范围限制 ----

	/// <summary>设置查询执行的字节范围。</summary>
	public void SetByteRange(uint start, uint end) =>
		TSApi.ts_query_cursor_set_byte_range(NativeHandle, start, end);

	/// <summary>设置查询执行的 (行, 列) 范围。</summary>
	public void SetPointRange(Point start, Point end) =>
		TSApi.ts_query_cursor_set_point_range(NativeHandle, start.ToNative(), end.ToNative());

	/// <summary>设置字节范围，要求所有匹配必须完全包含在此范围内。</summary>
	public void SetContainingByteRange(uint start, uint end) =>
		TSApi.ts_query_cursor_set_containing_byte_range(NativeHandle, start, end);

	/// <summary>设置 (行, 列) 范围，要求所有匹配必须完全包含在此范围内。</summary>
	public void SetContainingPointRange(Point start, Point end) =>
		TSApi.ts_query_cursor_set_containing_point_range(NativeHandle, start.ToNative(), end.ToNative());

	/// <summary>设置查询游标的最大起始深度。</summary>
	public void SetMaxStartDepth(uint depth) =>
		TSApi.ts_query_cursor_set_max_start_depth(NativeHandle, depth);

	// ---- 匹配限制 ----

	/// <summary>获取或设置此查询游标允许的最大进行中匹配数量。</summary>
	public uint MatchLimit {
		get => TSApi.ts_query_cursor_match_limit(NativeHandle);
		set => TSApi.ts_query_cursor_set_match_limit(NativeHandle, value);
	}

	/// <summary>检查此查询游标是否超出了匹配数量限制。</summary>
	public bool DidExceedMatchLimit =>
		TSApi.ts_query_cursor_did_exceed_match_limit(NativeHandle);

	// ---- ref struct 迭代路径（零分配） ----

	/// <summary>
	/// 推进到下一个匹配，返回 <see cref="QueryMatch"/>（ref struct）。
	/// 零数组分配，captures 通过原生指针按需读取。
	/// </summary>
	public bool NextMatch(out QueryMatch match)
	{
		if (!TSApi.ts_query_cursor_next_match(NativeHandle, out TSQueryMatch nativeMatch)) {
			match = default;
			return false;
		}
		match = ConvertMatch(nativeMatch);
		return true;
	}

	/// <summary>
	/// 推进到下一个捕获，返回 <see cref="QueryMatch"/>（ref struct）和捕获索引。
	/// 零数组分配，captures 通过原生指针按需读取。
	/// </summary>
	public bool NextCapture(out QueryMatch match, out uint captureIndex)
	{
		if (!TSApi.ts_query_cursor_next_capture(NativeHandle, out TSQueryMatch nativeMatch, out captureIndex)) {
			match = default;
			return false;
		}
		match = ConvertMatch(nativeMatch);
		return true;
	}

	/// <summary>移除指定 id 的匹配。</summary>
	public void RemoveMatch(uint matchId) =>
		TSApi.ts_query_cursor_remove_match(NativeHandle, matchId);

	// ---- IEnumerable 路径（有分配） ----

	/// <summary>
	/// 获取所有匹配的 IEnumerable。每次迭代内部拷贝 captures 为数组，
	/// 返回 QueryMatchOwned，可安全跨迭代使用，支持 LINQ。
	/// </summary>
	public IEnumerable<QueryMatchOwned> Matches {
		get {
			while (TSApi.ts_query_cursor_next_match(NativeHandle, out TSQueryMatch nativeMatch)) {
				yield return ConvertMatchOwned(nativeMatch);
			}
		}
	}

	// ---- 辅助方法 ----

	/// <summary>将原生 TSQueryMatch 转换为 QueryMatch（ref struct，零分配）。</summary>
	private QueryMatch ConvertMatch(TSQueryMatch nativeMatch) =>
		new(nativeMatch, _activeTree, _activeQuery);

	/// <summary>将原生 TSQueryMatch 转换为 QueryMatchOwned（普通 struct，拷贝 captures 为数组）。</summary>
	private QueryMatchOwned ConvertMatchOwned(TSQueryMatch nativeMatch)
	{
		if (_activeQuery == null || _activeTree == null ||
			nativeMatch.capture_count == 0 || nativeMatch.captures == IntPtr.Zero) {
			return new QueryMatchOwned((int)nativeMatch.id, nativeMatch.pattern_index, []);
		}

		var captures = new QueryCapture[nativeMatch.capture_count];
		for (int i = 0; i < nativeMatch.capture_count; i++) {
			var nativeCap = Marshal.PtrToStructure<TSQueryCapture>(
				nativeMatch.captures + i * _captureNativeSize);
			captures[i] = new QueryCapture(
				new Node(nativeCap.node, _activeTree),
				_activeQuery.CaptureNameForId(nativeCap.index));
		}

		return new QueryMatchOwned((int)nativeMatch.id, nativeMatch.pattern_index, captures);
	}

	/// <summary>释放查询游标及其使用的所有内存。</summary>
	public void Dispose()
	{
		_handle.Dispose();
		_activeQuery = null;
		_activeTree = null;
	}
}
