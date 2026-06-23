namespace TreeSitter.Native;

using System.Runtime.InteropServices;

public static partial class TSApi
{
    // =====================
    // 类型/符号信息
    // =====================

    // ts_node_type → IntPtr (返回内部 const char*，不需释放，用 Marshal.PtrToStringUTF8)
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_node_type(TSNode self);

    // ts_node_symbol → ushort (TSSymbol)
    [LibraryImport(LibraryName)]
    public static partial ushort ts_node_symbol(TSNode self);

    // ts_node_language → IntPtr (返回内部 const TSLanguage*，不需释放)
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_node_language(TSNode self);

    // ts_node_grammar_type → IntPtr (返回内部 const char*，不需释放)
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_node_grammar_type(TSNode self);

    // ts_node_grammar_symbol → ushort (TSSymbol)
    [LibraryImport(LibraryName)]
    public static partial ushort ts_node_grammar_symbol(TSNode self);

    // =====================
    // 位置信息
    // =====================

    // ts_node_start_byte → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_node_start_byte(TSNode self);

    // ts_node_start_point → TSPoint
    [LibraryImport(LibraryName)]
    public static partial TSPoint ts_node_start_point(TSNode self);

    // ts_node_end_byte → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_node_end_byte(TSNode self);

    // ts_node_end_point → TSPoint
    [LibraryImport(LibraryName)]
    public static partial TSPoint ts_node_end_point(TSNode self);

    // =====================
    // 字符串表示
    // =====================

    // ts_node_string → IntPtr (需用 ts_node_string_free 释放)
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_node_string(TSNode self);

    // =====================
    // 状态检查 (全部返回 bool)
    // =====================

    // ts_node_is_null → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_node_is_null(TSNode self);

    // ts_node_is_named → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_node_is_named(TSNode self);

    // ts_node_is_missing → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_node_is_missing(TSNode self);

    // ts_node_is_extra → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_node_is_extra(TSNode self);

    // ts_node_has_changes → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_node_has_changes(TSNode self);

    // ts_node_has_error → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_node_has_error(TSNode self);

    // ts_node_is_error → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_node_is_error(TSNode self);

    // =====================
    // 解析状态
    // =====================

    // ts_node_parse_state → ushort (TSStateId)
    [LibraryImport(LibraryName)]
    public static partial ushort ts_node_parse_state(TSNode self);

    // ts_node_next_parse_state → ushort (TSStateId)
    [LibraryImport(LibraryName)]
    public static partial ushort ts_node_next_parse_state(TSNode self);

    // =====================
    // 导航
    // =====================

    // ts_node_parent → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_parent(TSNode self);

    // ts_node_child_with_descendant → TSNode, 特殊：接受两个 TSNode 参数 (self + descendant)
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_child_with_descendant(TSNode self, TSNode descendant);

    // ts_node_child → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_child(TSNode self, uint child_index);

    // ts_node_field_name_for_child → IntPtr (const char*，不需释放)
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_node_field_name_for_child(TSNode self, uint child_index);

    // ts_node_field_name_for_named_child → IntPtr (const char*，不需释放)
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_node_field_name_for_named_child(TSNode self, uint named_child_index);

    // ts_node_child_count → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_node_child_count(TSNode self);

    // ts_node_named_child → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_named_child(TSNode self, uint child_index);

    // ts_node_named_child_count → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_node_named_child_count(TSNode self);

    // ts_node_child_by_field_name → TSNode (输入 string + length)
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_child_by_field_name(
        TSNode self, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, uint name_length);

    // ts_node_child_by_field_id → TSNode (输入 ushort fieldId)
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_child_by_field_id(TSNode self, ushort field_id);

    // ts_node_next_sibling → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_next_sibling(TSNode self);

    // ts_node_prev_sibling → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_prev_sibling(TSNode self);

    // ts_node_next_named_sibling → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_next_named_sibling(TSNode self);

    // ts_node_prev_named_sibling → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_prev_named_sibling(TSNode self);

    // ts_node_first_child_for_byte → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_first_child_for_byte(TSNode self, uint byte_);

    // ts_node_first_named_child_for_byte → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_first_named_child_for_byte(TSNode self, uint byte_);

    // ts_node_descendant_count → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_node_descendant_count(TSNode self);

    // =====================
    // 范围查找
    // =====================

    // ts_node_descendant_for_byte_range → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_descendant_for_byte_range(TSNode self, uint start, uint end);

    // ts_node_descendant_for_point_range → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_descendant_for_point_range(TSNode self, TSPoint start, TSPoint end);

    // ts_node_named_descendant_for_byte_range → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_named_descendant_for_byte_range(TSNode self, uint start, uint end);

    // ts_node_named_descendant_for_point_range → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_named_descendant_for_point_range(TSNode self, TSPoint start, TSPoint end);

    // =====================
    // 编辑
    // =====================

    // ts_node_edit → void, 参数 ref TSNode self, in TSInputEdit edit
    [LibraryImport(LibraryName)]
    public static partial void ts_node_edit(ref TSNode self, in TSInputEdit edit);

    // ts_node_eq → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_node_eq(TSNode self, TSNode other);

    // =====================
    // 独立编辑工具函数
    // =====================

    // ts_point_edit → void, 参数 ref TSPoint point, ref uint point_byte, in TSInputEdit edit
    [LibraryImport(LibraryName)]
    public static partial void ts_point_edit(ref TSPoint point, ref uint point_byte, in TSInputEdit edit);

    // ts_range_edit → void, 参数 ref TSRange range, in TSInputEdit edit
    [LibraryImport(LibraryName)]
    public static partial void ts_range_edit(ref TSRange range, in TSInputEdit edit);

    // =====================
    // 释放函数
    // =====================

    // ts_node_string_free → void (释放 ts_node_string 返回的指针)
    [LibraryImport(LibraryName, EntryPoint = "ts_node_string_free")]
    public static partial void ts_node_string_free(IntPtr ptr);
}
