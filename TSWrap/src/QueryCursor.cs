namespace TreeSitter.Wrap;

using System.Runtime.InteropServices;
using TreeSitter.Bind;

/// <summary>
/// 封装 tree-sitter 的 TSQueryCursor，用于在语法树上执行查询并迭代匹配结果。
/// 提供 ref struct 路径（<see cref="NextMatch"/> / <see cref="NextCapture"/>）和
/// IEnumerable 路径（<see cref="Matches"/>）。
/// <para>
/// <b>注意</b>：由于 <see cref="QueryCapture"/> 包含托管 string 字段，所有迭代路径
/// 都会为每次匹配的 captures 分配新数组。两种路径的主要区别在于返回类型：
/// <see cref="QueryMatch"/>（ref struct）vs <see cref="QueryMatchOwned"/>（普通 struct，支持 LINQ）。
/// </para>
/// </summary>
public sealed class QueryCursor : IDisposable
{
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
    public uint MatchLimit
    {
        get => TSApi.ts_query_cursor_match_limit(NativeHandle);
        set => TSApi.ts_query_cursor_set_match_limit(NativeHandle, value);
    }

    /// <summary>检查此查询游标是否超出了匹配数量限制。</summary>
    public bool DidExceedMatchLimit =>
        TSApi.ts_query_cursor_did_exceed_match_limit(NativeHandle);

    // ---- ref struct 迭代路径 ----

    /// <summary>
    /// 推进到下一个匹配，返回 <see cref="QueryMatch"/>（ref struct）。
    /// Captures 为 ReadOnlySpan，指向新分配的数组。
    /// </summary>
    public bool NextMatch(out QueryMatch match)
    {
        if (!TSApi.ts_query_cursor_next_match(NativeHandle, out TSQueryMatch nativeMatch))
        {
            match = default;
            return false;
        }

        match = ConvertMatch(nativeMatch);
        return true;
    }

    /// <summary>
    /// 推进到下一个捕获，返回 <see cref="QueryMatch"/>（ref struct）和捕获索引。
    /// Captures 为 ReadOnlySpan，指向新分配的数组。
    /// </summary>
    public bool NextCapture(out QueryMatch match, out uint captureIndex)
    {
        if (!TSApi.ts_query_cursor_next_capture(NativeHandle, out TSQueryMatch nativeMatch, out captureIndex))
        {
            match = default;
            return false;
        }

        match = ConvertMatch(nativeMatch);
        return true;
    }

    /// <summary>移除指定 id 的匹配。</summary>
    public void RemoveMatch(uint matchId) =>
        TSApi.ts_query_cursor_remove_match(NativeHandle, matchId);

    // ---- IEnumerable 路径 ----

    /// <summary>
    /// 获取所有匹配的 IEnumerable。每次迭代内部拷贝 captures 为数组，
    /// 返回 QueryMatchOwned，可安全跨迭代使用，支持 LINQ。
    /// </summary>
    public IEnumerable<QueryMatchOwned> Matches
    {
        get
        {
            while (TSApi.ts_query_cursor_next_match(NativeHandle, out TSQueryMatch nativeMatch))
            {
                yield return ConvertMatchOwned(nativeMatch);
            }
        }
    }

    // ---- 辅助方法 ----

    /// <summary>将原生 TSQueryMatch 转换为 QueryMatch（ref struct）。</summary>
    private QueryMatch ConvertMatch(TSQueryMatch nativeMatch)
    {
        if (_activeQuery == null || _activeTree == null ||
            nativeMatch.capture_count == 0 || nativeMatch.captures == IntPtr.Zero)
        {
            return new QueryMatch((int)nativeMatch.id, nativeMatch.pattern_index,
                ReadOnlySpan<QueryCapture>.Empty);
        }

        var captures = BuildCaptureArray(nativeMatch);
        return new QueryMatch((int)nativeMatch.id, nativeMatch.pattern_index, captures);
    }

    /// <summary>将原生 TSQueryMatch 转换为 QueryMatchOwned（普通 struct，拷贝 captures 为数组）。</summary>
    private QueryMatchOwned ConvertMatchOwned(TSQueryMatch nativeMatch)
    {
        var captures = BuildCaptureArray(nativeMatch);
        return new QueryMatchOwned(
            (int)nativeMatch.id, nativeMatch.pattern_index, captures.ToArray());
    }

    /// <summary>构建 QueryCapture 数组（从原生 TSQueryMatch 的 captures 指针）。</summary>
    private ReadOnlySpan<QueryCapture> BuildCaptureArray(TSQueryMatch nativeMatch)
    {
        if (_activeQuery == null || _activeTree == null ||
            nativeMatch.capture_count == 0 || nativeMatch.captures == IntPtr.Zero)
        {
            return ReadOnlySpan<QueryCapture>.Empty;
        }

        var result = new QueryCapture[nativeMatch.capture_count];
        var nativeSize = Marshal.SizeOf<TSQueryCapture>();

        for (int i = 0; i < nativeMatch.capture_count; i++)
        {
            var nativeCap = Marshal.PtrToStructure<TSQueryCapture>(
                nativeMatch.captures + i * nativeSize);

            var node = new Node(nativeCap.node, _activeTree);
            var name = _activeQuery.CaptureNameForId(nativeCap.index);
            result[i] = new QueryCapture(node, name);
        }

        return result;
    }

    /// <summary>释放查询游标及其使用的所有内存。</summary>
    public void Dispose()
    {
        _handle.Dispose();
        _activeQuery = null;
        _activeTree = null;
    }
}
