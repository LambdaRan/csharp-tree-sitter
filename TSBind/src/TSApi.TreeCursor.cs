namespace TreeSitter.Native;

using System.Runtime.InteropServices;

public static partial class TSApi
{
    // ts_tree_cursor_new → TSTreeCursor (by value 返回), 参数 TSNode by value
    [LibraryImport(LibraryName)]
    public static partial TSTreeCursor ts_tree_cursor_new(TSNode node);

    // ts_tree_cursor_delete → void, 参数 ref TSTreeCursor
    [LibraryImport(LibraryName)]
    public static partial void ts_tree_cursor_delete(ref TSTreeCursor self);

    // ts_tree_cursor_reset → void, 参数 ref TSTreeCursor, TSNode by value
    [LibraryImport(LibraryName)]
    public static partial void ts_tree_cursor_reset(ref TSTreeCursor self, TSNode node);

    // ts_tree_cursor_reset_to → void, 参数 ref TSTreeCursor dst, in TSTreeCursor src
    [LibraryImport(LibraryName)]
    public static partial void ts_tree_cursor_reset_to(ref TSTreeCursor dst, in TSTreeCursor src);

    // ts_tree_cursor_current_node → TSNode, 参数 in TSTreeCursor
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_tree_cursor_current_node(in TSTreeCursor self);

    // ts_tree_cursor_current_field_name → IntPtr (const char*，不需释放), 参数 in TSTreeCursor
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_tree_cursor_current_field_name(in TSTreeCursor self);

    // ts_tree_cursor_current_field_id → ushort (TSFieldId), 参数 in TSTreeCursor
    [LibraryImport(LibraryName)]
    public static partial ushort ts_tree_cursor_current_field_id(in TSTreeCursor self);

    // ts_tree_cursor_goto_parent → bool, 参数 ref TSTreeCursor
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_tree_cursor_goto_parent(ref TSTreeCursor self);

    // ts_tree_cursor_goto_next_sibling → bool, 参数 ref TSTreeCursor
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_tree_cursor_goto_next_sibling(ref TSTreeCursor self);

    // ts_tree_cursor_goto_previous_sibling → bool, 参数 ref TSTreeCursor
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_tree_cursor_goto_previous_sibling(ref TSTreeCursor self);

    // ts_tree_cursor_goto_first_child → bool, 参数 ref TSTreeCursor
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_tree_cursor_goto_first_child(ref TSTreeCursor self);

    // ts_tree_cursor_goto_last_child → bool, 参数 ref TSTreeCursor
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_tree_cursor_goto_last_child(ref TSTreeCursor self);

    // ts_tree_cursor_goto_descendant → void, 参数 ref TSTreeCursor, uint index
    [LibraryImport(LibraryName)]
    public static partial void ts_tree_cursor_goto_descendant(ref TSTreeCursor self, uint goal_descendant_index);

    // ts_tree_cursor_current_descendant_index → uint, 参数 in TSTreeCursor
    [LibraryImport(LibraryName)]
    public static partial uint ts_tree_cursor_current_descendant_index(in TSTreeCursor self);

    // ts_tree_cursor_current_depth → uint, 参数 in TSTreeCursor
    [LibraryImport(LibraryName)]
    public static partial uint ts_tree_cursor_current_depth(in TSTreeCursor self);

    // ts_tree_cursor_goto_first_child_for_byte → long (int64_t! 返回索引或-1), 参数 ref TSTreeCursor
    [LibraryImport(LibraryName)]
    public static partial long ts_tree_cursor_goto_first_child_for_byte(ref TSTreeCursor self, uint goal_byte);

    // ts_tree_cursor_goto_first_child_for_point → long (int64_t! 返回索引或-1), 参数 ref TSTreeCursor
    [LibraryImport(LibraryName)]
    public static partial long ts_tree_cursor_goto_first_child_for_point(ref TSTreeCursor self, TSPoint goal_point);

    // ts_tree_cursor_copy → TSTreeCursor (by value 返回), 参数 in TSTreeCursor
    [LibraryImport(LibraryName)]
    public static partial TSTreeCursor ts_tree_cursor_copy(in TSTreeCursor cursor);
}
