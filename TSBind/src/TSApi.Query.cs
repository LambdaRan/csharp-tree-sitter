namespace TreeSitter.Bind;

using System.Runtime.InteropServices;

public static partial class TSApi
{
    // =====================
    // Query 创建/销毁
    // =====================

    /// <summary>
    /// 从包含一个或多个 S 表达式模式的字符串创建新查询。
    /// 查询与特定语言关联，只能用于该语言解析的语法节点。
    /// 所有模式有效时返回 TSQuery；模式无效时返回 IntPtr.Zero，
    /// 并在 error_offset 和 error_type 中提供错误信息。
    /// </summary>
    // ts_query_new → IntPtr (失败返回 IntPtr.Zero)
    // 返回值：原 TSQuery*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_query_new(
        IntPtr language, // 原 const TSLanguage*
        ReadOnlySpan<byte> source,
        uint source_len,
        out uint error_offset,
        out TSQueryError error_type);

    /// <summary>删除查询，释放其使用的所有内存。</summary>
    // ts_query_delete → void
    [LibraryImport(LibraryName)]
    public static partial void ts_query_delete(IntPtr self); // 原 TSQuery*

    // =====================
    // Query 信息查询
    // =====================

    /// <summary>获取查询中的模式数量。</summary>
    // ts_query_pattern_count → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_query_pattern_count(IntPtr self); // 原 const TSQuery*

    /// <summary>获取查询中的捕获数量。</summary>
    // ts_query_capture_count → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_query_capture_count(IntPtr self); // 原 const TSQuery*

    /// <summary>获取查询中的字符串字面量数量。</summary>
    // ts_query_string_count → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_query_string_count(IntPtr self); // 原 const TSQuery*

    /// <summary>获取指定模式在查询源文本中的起始字节偏移。</summary>
    // ts_query_start_byte_for_pattern → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_query_start_byte_for_pattern(IntPtr self, uint pattern_index); // 原 const TSQuery*

    /// <summary>获取指定模式在查询源文本中的结束字节偏移。</summary>
    // ts_query_end_byte_for_pattern → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_query_end_byte_for_pattern(IntPtr self, uint pattern_index); // 原 const TSQuery*

    /// <summary>
    /// 获取指定模式的所有谓词。
    /// 谓词以步骤数组表示，type 字段有三种值：
    /// Capture 表示捕获名、String 表示字面量字符串、Done 表示谓词结束哨兵。
    /// </summary>
    // ts_query_predicates_for_pattern → IntPtr (内部拥有)
    // 返回值：原 const TSQueryPredicateStep*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_query_predicates_for_pattern(
        IntPtr self, // 原 const TSQuery*
        uint pattern_index, out uint step_count);

    /// <summary>检查指定模式是否有唯一的根节点。</summary>
    // ts_query_is_pattern_rooted → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_query_is_pattern_rooted(IntPtr self, uint pattern_index); // 原 const TSQuery*

    /// <summary>
    /// 检查指定模式是否为"非局部"的。
    /// 非局部模式有多个根节点，可在语法中的重复节点序列内匹配。
    /// </summary>
    // ts_query_is_pattern_non_local → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_query_is_pattern_non_local(IntPtr self, uint pattern_index); // 原 const TSQuery*

    /// <summary>检查指定模式是否在达到给定步骤（按字节偏移指定）后保证匹配。</summary>
    // ts_query_is_pattern_guaranteed_at_step → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_query_is_pattern_guaranteed_at_step(IntPtr self, uint byte_offset); // 原 const TSQuery*

    /// <summary>获取指定索引处捕获的名称和长度。每个捕获按在查询源文本中出现的顺序关联一个数值 id。</summary>
    // ts_query_capture_name_for_id → IntPtr (内部拥有)
    // 返回值：原 const char*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_query_capture_name_for_id(
        IntPtr self, // 原 const TSQuery*
        uint index, out uint length);

    /// <summary>获取指定捕获的量词。每个捕获按在查询源文本中出现的顺序关联一个数值 id。</summary>
    // ts_query_capture_quantifier_for_id → TSQuantifier
    [LibraryImport(LibraryName)]
    public static partial TSQuantifier ts_query_capture_quantifier_for_id(
        IntPtr self, // 原 const TSQuery*
        uint pattern_index, uint capture_index);

    /// <summary>获取指定索引处字符串字面量的值和长度。</summary>
    // ts_query_string_value_for_id → IntPtr (内部拥有)
    // 返回值：原 const char*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_query_string_value_for_id(
        IntPtr self, // 原 const TSQuery*
        uint index, out uint length);

    /// <summary>禁用查询中的某个捕获。禁用后该捕获不会在匹配结果中返回，也无法撤销。</summary>
    // ts_query_disable_capture → void
    [LibraryImport(LibraryName)]
    public static partial void ts_query_disable_capture(
        IntPtr self, // 原 TSQuery*
        ReadOnlySpan<byte> name, uint length);

    /// <summary>禁用查询中的某个模式。禁用后该模式不会匹配，且消除大部分与该模式相关的开销，无法撤销。</summary>
    // ts_query_disable_pattern → void
    [LibraryImport(LibraryName)]
    public static partial void ts_query_disable_pattern(IntPtr self, uint pattern_index); // 原 TSQuery*

    // =====================
    // QueryCursor 创建/销毁
    // =====================

    /// <summary>
    /// 创建用于执行查询的新游标。
    /// 游标存储迭代搜索匹配所需的状态。
    /// 使用 ts_query_cursor_exec 启动查询，然后：
    /// 1. 反复调用 ts_query_cursor_next_match 按发现顺序迭代所有匹配。
    /// 2. 反复调用 ts_query_cursor_next_capture 按顺序迭代所有捕获。
    /// </summary>
    // ts_query_cursor_new → IntPtr
    // 返回值：原 TSQueryCursor*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_query_cursor_new();

    /// <summary>删除查询游标，释放其使用的所有内存。</summary>
    // ts_query_cursor_delete → void
    [LibraryImport(LibraryName)]
    public static partial void ts_query_cursor_delete(IntPtr self); // 原 TSQueryCursor*

    // =====================
    // QueryCursor 执行
    // =====================

    /// <summary>在给定节点上开始运行指定查询。</summary>
    // ts_query_cursor_exec → void
    [LibraryImport(LibraryName)]
    public static partial void ts_query_cursor_exec(
        IntPtr self, // 原 TSQueryCursor*
        IntPtr query, // 原 const TSQuery*
        TSNode node);

    /// <summary>在给定节点上开始运行指定查询，可附加查询选项。</summary>
    // ts_query_cursor_exec_with_options → void
    [LibraryImport(LibraryName)]
    public static partial void ts_query_cursor_exec_with_options(
        IntPtr self, // 原 TSQueryCursor*
        IntPtr query, // 原 const TSQuery*
        TSNode node, in TSQueryCursorOptions query_options);

    // =====================
    // QueryCursor 配置
    // =====================

    /// <summary>检查此查询游标是否超出了进行中匹配的最大数量限制。</summary>
    // ts_query_cursor_did_exceed_match_limit → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_query_cursor_did_exceed_match_limit(IntPtr self); // 原 const TSQueryCursor*

    /// <summary>获取此查询游标的匹配数量上限。</summary>
    // ts_query_cursor_match_limit → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_query_cursor_match_limit(IntPtr self); // 原 const TSQueryCursor*

    /// <summary>设置此查询游标允许的最大进行中匹配数量。</summary>
    // ts_query_cursor_set_match_limit → void
    [LibraryImport(LibraryName)]
    public static partial void ts_query_cursor_set_match_limit(IntPtr self, uint limit); // 原 TSQueryCursor*

    /// <summary>设置查询执行的字节范围。游标返回与该范围有交集的匹配。</summary>
    // ts_query_cursor_set_byte_range → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_query_cursor_set_byte_range(IntPtr self, uint start_byte, uint end_byte); // 原 TSQueryCursor*

    /// <summary>设置查询执行的 (行, 列) 范围。游标返回与该范围有交集的匹配。</summary>
    // ts_query_cursor_set_point_range → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_query_cursor_set_point_range(IntPtr self, TSPoint start_point, TSPoint end_point); // 原 TSQueryCursor*

    /// <summary>设置字节范围，要求所有匹配必须完全包含在此范围内（不仅是交集）。</summary>
    // ts_query_cursor_set_containing_byte_range → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_query_cursor_set_containing_byte_range(IntPtr self, uint start_byte, uint end_byte); // 原 TSQueryCursor*

    /// <summary>设置 (行, 列) 范围，要求所有匹配必须完全包含在此范围内。</summary>
    // ts_query_cursor_set_containing_point_range → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_query_cursor_set_containing_point_range(
        IntPtr self, TSPoint start_point, TSPoint end_point); // 原 TSQueryCursor*

    /// <summary>
    /// 设置查询游标的最大起始深度。
    /// 设为零可限制只在某节点上搜索而深入子节点获取捕获；
    /// 设为 UINT32_MAX 可移除深度限制。
    /// </summary>
    // ts_query_cursor_set_max_start_depth → void
    [LibraryImport(LibraryName)]
    public static partial void ts_query_cursor_set_max_start_depth(IntPtr self, uint max_start_depth); // 原 TSQueryCursor*

    // =====================
    // QueryCursor 迭代
    // =====================

    /// <summary>推进到当前运行查询的下一个匹配。有匹配时写入 match 并返回 true，否则返回 false。</summary>
    // ts_query_cursor_next_match → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_query_cursor_next_match(IntPtr self, out TSQueryMatch match); // 原 TSQueryCursor*

    /// <summary>移除指定 id 的匹配。</summary>
    // ts_query_cursor_remove_match → void
    [LibraryImport(LibraryName)]
    public static partial void ts_query_cursor_remove_match(IntPtr self, uint match_id); // 原 TSQueryCursor*

    /// <summary>
    /// 推进到当前运行查询的下一个捕获。
    /// 有捕获时写入 match 及其在匹配的捕获列表中的索引 capture_index，否则返回 false。
    /// </summary>
    // ts_query_cursor_next_capture → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_query_cursor_next_capture(
        IntPtr self, out TSQueryMatch match, out uint capture_index); // 原 TSQueryCursor*
}
