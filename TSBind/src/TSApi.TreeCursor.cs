namespace TreeSitter.Bind;

using System.Runtime.InteropServices;

public static partial class TSApi
{
    /// <summary>
    /// 从给定节点创建一个新的树游标。
    /// 树游标比 TSNode 函数更高效地遍历语法树。它是一个可变对象，
    /// 始终位于某个语法节点上，可以命令式地移动到不同节点。
    /// 给定的节点被视为游标的根，游标不能走出此节点。
    /// </summary>
    // ts_tree_cursor_new → TSTreeCursor (by value 返回), 参数 TSNode by value
    [LibraryImport(LibraryName)]
    public static partial TSTreeCursor ts_tree_cursor_new(TSNode node);

    /// <summary>删除树游标，释放其使用的所有内存。</summary>
    // ts_tree_cursor_delete → void, 参数 ref TSTreeCursor
    [LibraryImport(LibraryName)]
    public static partial void ts_tree_cursor_delete(ref TSTreeCursor self);

    /// <summary>重新初始化树游标，回到创建时的原始节点。</summary>
    // ts_tree_cursor_reset → void, 参数 ref TSTreeCursor, TSNode by value
    [LibraryImport(LibraryName)]
    public static partial void ts_tree_cursor_reset(ref TSTreeCursor self, TSNode node);

    /// <summary>将树游标重新初始化到另一个游标的相同位置。不会丢失父节点信息，可复用已创建的游标。</summary>
    // ts_tree_cursor_reset_to → void, 参数 ref TSTreeCursor dst, in TSTreeCursor src
    [LibraryImport(LibraryName)]
    public static partial void ts_tree_cursor_reset_to(ref TSTreeCursor dst, in TSTreeCursor src);

    /// <summary>获取树游标的当前节点。</summary>
    // ts_tree_cursor_current_node → TSNode, 参数 in TSTreeCursor
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_tree_cursor_current_node(in TSTreeCursor self);

    /// <summary>获取树游标当前节点的字段名。如果当前节点没有字段则返回 NULL。</summary>
    // ts_tree_cursor_current_field_name → IntPtr (返回内部字符串，不需释放)
    // 返回值：原 const char*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_tree_cursor_current_field_name(in TSTreeCursor self);

    /// <summary>获取树游标当前节点的字段 id（TSFieldId）。如果当前节点没有字段则返回零。</summary>
    // ts_tree_cursor_current_field_id → ushort (TSFieldId), 参数 in TSTreeCursor
    [LibraryImport(LibraryName)]
    public static partial ushort ts_tree_cursor_current_field_id(in TSTreeCursor self);

    /// <summary>将游标移动到当前节点的父节点。成功返回 true，已在根节点则返回 false。</summary>
    // ts_tree_cursor_goto_parent → bool, 参数 ref TSTreeCursor
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_tree_cursor_goto_parent(ref TSTreeCursor self);

    /// <summary>将游标移动到当前节点的下一个兄弟节点。成功返回 true，无下一个兄弟则返回 false。</summary>
    // ts_tree_cursor_goto_next_sibling → bool, 参数 ref TSTreeCursor
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_tree_cursor_goto_next_sibling(ref TSTreeCursor self);

    /// <summary>将游标移动到当前节点的上一个兄弟节点。成功返回 true，无上一个兄弟则返回 false。可能比 goto_next_sibling 慢。</summary>
    // ts_tree_cursor_goto_previous_sibling → bool, 参数 ref TSTreeCursor
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_tree_cursor_goto_previous_sibling(ref TSTreeCursor self);

    /// <summary>将游标移动到当前节点的第一个子节点。成功返回 true，无子节点则返回 false。</summary>
    // ts_tree_cursor_goto_first_child → bool, 参数 ref TSTreeCursor
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_tree_cursor_goto_first_child(ref TSTreeCursor self);

    /// <summary>将游标移动到当前节点的最后一个子节点。成功返回 true，无子节点则返回 false。可能比 goto_first_child 慢。</summary>
    // ts_tree_cursor_goto_last_child → bool, 参数 ref TSTreeCursor
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_tree_cursor_goto_last_child(ref TSTreeCursor self);

    /// <summary>将游标移动到原始节点的第 N 个后代节点，零表示原始节点本身。</summary>
    // ts_tree_cursor_goto_descendant → void, 参数 ref TSTreeCursor, uint index
    [LibraryImport(LibraryName)]
    public static partial void ts_tree_cursor_goto_descendant(ref TSTreeCursor self, uint goal_descendant_index);

    /// <summary>获取游标当前节点在原始节点所有后代中的索引。</summary>
    // ts_tree_cursor_current_descendant_index → uint, 参数 in TSTreeCursor
    [LibraryImport(LibraryName)]
    public static partial uint ts_tree_cursor_current_descendant_index(in TSTreeCursor self);

    /// <summary>获取游标当前节点相对于原始节点的深度。</summary>
    // ts_tree_cursor_current_depth → uint, 参数 in TSTreeCursor
    [LibraryImport(LibraryName)]
    public static partial uint ts_tree_cursor_current_depth(in TSTreeCursor self);

    /// <summary>
    /// 将游标移动到当前节点的第一个包含或在给定字节偏移之后开始的子节点。
    /// 返回子节点索引，未找到则返回 -1。
    /// </summary>
    // ts_tree_cursor_goto_first_child_for_byte → long (int64_t! 返回索引或-1), 参数 ref TSTreeCursor
    [LibraryImport(LibraryName)]
    public static partial long ts_tree_cursor_goto_first_child_for_byte(ref TSTreeCursor self, uint goal_byte);

    /// <summary>
    /// 将游标移动到当前节点的第一个包含或在给定位置之后开始的子节点。
    /// 返回子节点索引，未找到则返回 -1。
    /// </summary>
    // ts_tree_cursor_goto_first_child_for_point → long (int64_t! 返回索引或-1), 参数 ref TSTreeCursor
    [LibraryImport(LibraryName)]
    public static partial long ts_tree_cursor_goto_first_child_for_point(ref TSTreeCursor self, TSPoint goal_point);

    /// <summary>创建树游标的副本。</summary>
    // ts_tree_cursor_copy → TSTreeCursor (by value 返回), 参数 in TSTreeCursor
    [LibraryImport(LibraryName)]
    public static partial TSTreeCursor ts_tree_cursor_copy(in TSTreeCursor cursor);
}
