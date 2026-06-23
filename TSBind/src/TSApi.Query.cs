namespace TreeSitter.Native;

using System.Runtime.InteropServices;

public static partial class TSApi
{
    // =====================
    // Query 创建/销毁
    // =====================

    // ts_query_new → IntPtr (失败返回 IntPtr.Zero), 参数含 out uint error_offset, out TSQueryError error_type
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_query_new(
        IntPtr language,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string source,
        uint source_len,
        out uint error_offset,
        out TSQueryError error_type);

    // ts_query_delete → void
    [LibraryImport(LibraryName)]
    public static partial void ts_query_delete(IntPtr self);

    // =====================
    // Query 信息查询
    // =====================

    // ts_query_pattern_count → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_query_pattern_count(IntPtr self);

    // ts_query_capture_count → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_query_capture_count(IntPtr self);

    // ts_query_string_count → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_query_string_count(IntPtr self);

    // ts_query_start_byte_for_pattern → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_query_start_byte_for_pattern(IntPtr self, uint pattern_index);

    // ts_query_end_byte_for_pattern → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_query_end_byte_for_pattern(IntPtr self, uint pattern_index);

    // ts_query_predicates_for_pattern → IntPtr (const TSQueryPredicateStep*，内部拥有), 参数含 out uint step_count
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_query_predicates_for_pattern(IntPtr self, uint pattern_index, out uint step_count);

    // ts_query_is_pattern_rooted → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_query_is_pattern_rooted(IntPtr self, uint pattern_index);

    // ts_query_is_pattern_non_local → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_query_is_pattern_non_local(IntPtr self, uint pattern_index);

    // ts_query_is_pattern_guaranteed_at_step → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_query_is_pattern_guaranteed_at_step(IntPtr self, uint byte_offset);

    // ts_query_capture_name_for_id → IntPtr (const char*，内部拥有), 参数含 out uint length
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_query_capture_name_for_id(IntPtr self, uint index, out uint length);

    // ts_query_capture_quantifier_for_id → TSQuantifier
    [LibraryImport(LibraryName)]
    public static partial TSQuantifier ts_query_capture_quantifier_for_id(
        IntPtr self, uint pattern_index, uint capture_index);

    // ts_query_string_value_for_id → IntPtr (const char*，内部拥有), 参数含 out uint length
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_query_string_value_for_id(IntPtr self, uint index, out uint length);

    // ts_query_disable_capture → void, 参数 string name + uint length
    [LibraryImport(LibraryName)]
    public static partial void ts_query_disable_capture(
        IntPtr self, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, uint length);

    // ts_query_disable_pattern → void
    [LibraryImport(LibraryName)]
    public static partial void ts_query_disable_pattern(IntPtr self, uint pattern_index);

    // =====================
    // QueryCursor 创建/销毁
    // =====================

    // ts_query_cursor_new → IntPtr
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_query_cursor_new();

    // ts_query_cursor_delete → void
    [LibraryImport(LibraryName)]
    public static partial void ts_query_cursor_delete(IntPtr self);

    // =====================
    // QueryCursor 执行
    // =====================

    // ts_query_cursor_exec → void, 参数含 TSNode by value
    [LibraryImport(LibraryName)]
    public static partial void ts_query_cursor_exec(IntPtr self, IntPtr query, TSNode node);

    // ts_query_cursor_exec_with_options → void, 参数含 TSNode by value + in TSQueryCursorOptions
    [LibraryImport(LibraryName)]
    public static partial void ts_query_cursor_exec_with_options(
        IntPtr self, IntPtr query, TSNode node, in TSQueryCursorOptions query_options);

    // =====================
    // QueryCursor 配置
    // =====================

    // ts_query_cursor_did_exceed_match_limit → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_query_cursor_did_exceed_match_limit(IntPtr self);

    // ts_query_cursor_match_limit → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_query_cursor_match_limit(IntPtr self);

    // ts_query_cursor_set_match_limit → void
    [LibraryImport(LibraryName)]
    public static partial void ts_query_cursor_set_match_limit(IntPtr self, uint limit);

    // ts_query_cursor_set_byte_range → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_query_cursor_set_byte_range(IntPtr self, uint start_byte, uint end_byte);

    // ts_query_cursor_set_point_range → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_query_cursor_set_point_range(IntPtr self, TSPoint start_point, TSPoint end_point);

    // ts_query_cursor_set_containing_byte_range → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_query_cursor_set_containing_byte_range(IntPtr self, uint start_byte, uint end_byte);

    // ts_query_cursor_set_containing_point_range → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_query_cursor_set_containing_point_range(
        IntPtr self, TSPoint start_point, TSPoint end_point);

    // ts_query_cursor_set_max_start_depth → void
    [LibraryImport(LibraryName)]
    public static partial void ts_query_cursor_set_max_start_depth(IntPtr self, uint max_start_depth);

    // =====================
    // QueryCursor 迭代
    // =====================

    // ts_query_cursor_next_match → bool, 参数 out TSQueryMatch
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_query_cursor_next_match(IntPtr self, out TSQueryMatch match);

    // ts_query_cursor_remove_match → void
    [LibraryImport(LibraryName)]
    public static partial void ts_query_cursor_remove_match(IntPtr self, uint match_id);

    // ts_query_cursor_next_capture → bool, 参数 out TSQueryMatch, out uint capture_index
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_query_cursor_next_capture(
        IntPtr self, out TSQueryMatch match, out uint capture_index);
}
