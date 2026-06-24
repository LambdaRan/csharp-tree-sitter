namespace TreeSitter.Native;

using System.Runtime.InteropServices;

public static partial class TSApi
{
    // =====================
    // 类型/符号信息
    // =====================

    /// <summary>获取节点的类型，返回以 null 结尾的字符串。返回的指针由库内部拥有，不需释放。</summary>
    // ts_node_type → IntPtr (返回内部字符串，不需释放，用 Marshal.PtrToStringUTF8)
    // 返回值：原 const char*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_node_type(TSNode self);

    /// <summary>获取节点的类型，返回数值 id（TSSymbol）。</summary>
    // ts_node_symbol → ushort (TSSymbol)
    [LibraryImport(LibraryName)]
    public static partial ushort ts_node_symbol(TSNode self);

    /// <summary>获取节点的语言。</summary>
    // ts_node_language → IntPtr (返回内部指针，不需释放)
    // 返回值：原 const TSLanguage*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_node_language(TSNode self);

    /// <summary>获取节点在语法中的类型（忽略别名），返回以 null 结尾的字符串。</summary>
    // ts_node_grammar_type → IntPtr (返回内部字符串，不需释放)
    // 返回值：原 const char*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_node_grammar_type(TSNode self);

    /// <summary>获取节点在语法中的类型数值 id（忽略别名）。应在 ts_language_next_state 中使用此值。</summary>
    // ts_node_grammar_symbol → ushort (TSSymbol)
    [LibraryImport(LibraryName)]
    public static partial ushort ts_node_grammar_symbol(TSNode self);

    // =====================
    // 位置信息
    // =====================

    /// <summary>获取节点的起始字节偏移。</summary>
    // ts_node_start_byte → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_node_start_byte(TSNode self);

    /// <summary>获取节点的起始位置（行、列）。</summary>
    // ts_node_start_point → TSPoint
    [LibraryImport(LibraryName)]
    public static partial TSPoint ts_node_start_point(TSNode self);

    /// <summary>获取节点的结束字节偏移。</summary>
    // ts_node_end_byte → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_node_end_byte(TSNode self);

    /// <summary>获取节点的结束位置（行、列）。</summary>
    // ts_node_end_point → TSPoint
    [LibraryImport(LibraryName)]
    public static partial TSPoint ts_node_end_point(TSNode self);

    // =====================
    // 字符串表示
    // =====================

    /// <summary>获取节点的 S 表达式字符串表示。返回的字符串由 malloc 分配，需用 ts_node_string_free 释放。</summary>
    // ts_node_string → IntPtr (需用 ts_node_string_free 释放)
    // 返回值：原 char*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_node_string(TSNode self);

    // =====================
    // 状态检查
    // =====================

    /// <summary>检查节点是否为 null。ts_node_child 等函数在找不到节点时返回 null 节点。</summary>
    // ts_node_is_null → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_node_is_null(TSNode self);

    /// <summary>检查节点是否为命名节点。命名节点对应语法中的命名规则，匿名节点对应字符串字面量。</summary>
    // ts_node_is_named → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_node_is_named(TSNode self);

    /// <summary>检查节点是否为缺失节点。缺失节点是解析器为从某些语法错误中恢复而插入的。</summary>
    // ts_node_is_missing → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_node_is_missing(TSNode self);

    /// <summary>检查节点是否为额外节点。额外节点表示注释等可选内容，可出现在任何位置。</summary>
    // ts_node_is_extra → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_node_is_extra(TSNode self);

    /// <summary>检查语法节点是否已被编辑。</summary>
    // ts_node_has_changes → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_node_has_changes(TSNode self);

    /// <summary>检查节点是否为语法错误或包含语法错误。</summary>
    // ts_node_has_error → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_node_has_error(TSNode self);

    /// <summary>检查节点是否为语法错误。</summary>
    // ts_node_is_error → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_node_is_error(TSNode self);

    // =====================
    // 解析状态
    // =====================

    /// <summary>获取此节点的解析状态（TSStateId）。</summary>
    // ts_node_parse_state → ushort (TSStateId)
    [LibraryImport(LibraryName)]
    public static partial ushort ts_node_parse_state(TSNode self);

    /// <summary>获取此节点之后的解析状态（TSStateId）。</summary>
    // ts_node_next_parse_state → ushort (TSStateId)
    [LibraryImport(LibraryName)]
    public static partial ushort ts_node_next_parse_state(TSNode self);

    // =====================
    // 导航
    // =====================

    /// <summary>获取节点的直接父节点。优先使用 ts_node_child_with_descendant 遍历祖先。</summary>
    // ts_node_parent → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_parent(TSNode self);

    /// <summary>获取包含指定 descendant 的节点。可能返回 descendant 本身。</summary>
    // ts_node_child_with_descendant → TSNode, 特殊：接受两个 TSNode 参数 (self + descendant)
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_child_with_descendant(TSNode self, TSNode descendant);

    /// <summary>获取指定索引处的子节点，索引从零开始。</summary>
    // ts_node_child → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_child(TSNode self, uint child_index);

    /// <summary>获取指定索引处子节点的字段名。如果没有字段则返回 NULL。</summary>
    // ts_node_field_name_for_child → IntPtr (返回内部字符串，不需释放)
    // 返回值：原 const char*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_node_field_name_for_child(TSNode self, uint child_index);

    /// <summary>获取指定索引处命名子节点的字段名。如果没有字段则返回 NULL。</summary>
    // ts_node_field_name_for_named_child → IntPtr (返回内部字符串，不需释放)
    // 返回值：原 const char*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_node_field_name_for_named_child(TSNode self, uint named_child_index);

    /// <summary>获取节点的子节点数量。</summary>
    // ts_node_child_count → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_node_child_count(TSNode self);

    /// <summary>获取指定索引处的命名子节点。</summary>
    // ts_node_named_child → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_named_child(TSNode self, uint child_index);

    /// <summary>获取节点的命名子节点数量。</summary>
    // ts_node_named_child_count → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_node_named_child_count(TSNode self);

    /// <summary>通过字段名获取节点的子节点。</summary>
    // ts_node_child_by_field_name → TSNode (输入 string + length)
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_child_by_field_name(
        TSNode self, ReadOnlySpan<byte> name, uint name_length);

    /// <summary>通过数值字段 id 获取节点的子节点。可用 ts_language_field_id_for_name 将字段名转换为 id。</summary>
    // ts_node_child_by_field_id → TSNode (输入 ushort fieldId)
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_child_by_field_id(TSNode self, ushort field_id);

    /// <summary>获取节点的下一个兄弟节点。</summary>
    // ts_node_next_sibling → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_next_sibling(TSNode self);

    /// <summary>获取节点的上一个兄弟节点。</summary>
    // ts_node_prev_sibling → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_prev_sibling(TSNode self);

    /// <summary>获取节点的下一个命名兄弟节点。</summary>
    // ts_node_next_named_sibling → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_next_named_sibling(TSNode self);

    /// <summary>获取节点的上一个命名兄弟节点。</summary>
    // ts_node_prev_named_sibling → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_prev_named_sibling(TSNode self);

    /// <summary>获取节点的第一个包含或在给定字节偏移之后开始的子节点。</summary>
    // ts_node_first_child_for_byte → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_first_child_for_byte(TSNode self, uint byte_);

    /// <summary>获取节点的第一个包含或在给定字节偏移之后开始的命名子节点。</summary>
    // ts_node_first_named_child_for_byte → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_first_named_child_for_byte(TSNode self, uint byte_);

    /// <summary>获取节点的后代数量，包含节点自身（计数为 1）。</summary>
    // ts_node_descendant_count → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_node_descendant_count(TSNode self);

    // =====================
    // 范围查找
    // =====================

    /// <summary>获取此节点内跨越给定字节范围的最小子节点。</summary>
    // ts_node_descendant_for_byte_range → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_descendant_for_byte_range(TSNode self, uint start, uint end);

    /// <summary>获取此节点内跨越给定 (行, 列) 范围的最小子节点。</summary>
    // ts_node_descendant_for_point_range → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_descendant_for_point_range(TSNode self, TSPoint start, TSPoint end);

    /// <summary>获取此节点内跨越给定字节范围的最小命名子节点。</summary>
    // ts_node_named_descendant_for_byte_range → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_named_descendant_for_byte_range(TSNode self, uint start, uint end);

    /// <summary>获取此节点内跨越给定 (行, 列) 范围的最小命名子节点。</summary>
    // ts_node_named_descendant_for_point_range → TSNode
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_node_named_descendant_for_point_range(TSNode self, TSPoint start, TSPoint end);

    // =====================
    // 编辑
    // =====================

    /// <summary>
    /// 编辑节点以保持与已编辑源代码的同步。
    /// 通常使用 ts_tree_edit 后，后续获取的节点已反映编辑。
    /// 仅在需要保留并继续使用某个 TSNode 实例时才需调用此方法。
    /// </summary>
    // ts_node_edit → void, 参数 ref TSNode self, in TSInputEdit edit
    [LibraryImport(LibraryName)]
    public static partial void ts_node_edit(ref TSNode self, in TSInputEdit edit);

    /// <summary>检查两个节点是否相同。</summary>
    // ts_node_eq → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_node_eq(TSNode self, TSNode other);

    // =====================
    // 独立编辑工具函数
    // =====================

    /// <summary>编辑一个点以保持与已编辑源代码的同步，更新字节偏移和行列位置。</summary>
    // ts_point_edit → void, 参数 ref TSPoint point, ref uint point_byte, in TSInputEdit edit
    [LibraryImport(LibraryName)]
    public static partial void ts_point_edit(ref TSPoint point, ref uint point_byte, in TSInputEdit edit);

    /// <summary>编辑一个范围以保持与已编辑源代码的同步，更新起止位置。</summary>
    // ts_range_edit → void, 参数 ref TSRange range, in TSInputEdit edit
    [LibraryImport(LibraryName)]
    public static partial void ts_range_edit(ref TSRange range, in TSInputEdit edit);

    // =====================
    // 释放函数
    // =====================

    /// <summary>释放 ts_node_string 返回的指针。</summary>
    // ts_node_string_free → void
    [LibraryImport(LibraryName, EntryPoint = "ts_node_string_free")]
    public static partial void ts_node_string_free(IntPtr ptr); // 原 char*
}
