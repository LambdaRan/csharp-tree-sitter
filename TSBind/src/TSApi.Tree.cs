namespace TreeSitter.Native;

using System.Runtime.InteropServices;

public static partial class TSApi
{
    // ts_tree_copy → IntPtr (需要 ts_tree_delete 释放)
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_tree_copy(IntPtr self);

    // ts_tree_delete → void
    [LibraryImport(LibraryName)]
    public static partial void ts_tree_delete(IntPtr self);

    // ts_tree_root_node → TSNode (by value)
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_tree_root_node(IntPtr self);

    // ts_tree_root_node_with_offset → TSNode (by value)
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_tree_root_node_with_offset(
        IntPtr self, uint offset_bytes, TSPoint offset_extent);

    // ts_tree_language → IntPtr (库内部拥有，不需释放)
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_tree_language(IntPtr self);

    // ts_tree_included_ranges → IntPtr (caller 需用 ts_tree_included_ranges_free 释放)
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_tree_included_ranges(IntPtr self, out uint length);

    // ts_tree_edit → void, 参数 in TSInputEdit
    [LibraryImport(LibraryName)]
    public static partial void ts_tree_edit(IntPtr self, in TSInputEdit edit);

    // ts_tree_get_changed_ranges → IntPtr (caller 需用 ts_tree_get_changed_ranges_free 释放)
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_tree_get_changed_ranges(
        IntPtr old_tree, IntPtr new_tree, out uint length);

    // ts_tree_print_dot_graph → void
    [LibraryImport(LibraryName)]
    public static partial void ts_tree_print_dot_graph(IntPtr self, int file_descriptor);

    // =====================
    // 释放函数（.def 中导出 free 的别名）
    // =====================

    // ts_tree_included_ranges_free → void (释放 ts_tree_included_ranges 返回的指针)
    [LibraryImport(LibraryName, EntryPoint = "ts_tree_included_ranges_free")]
    public static partial void ts_tree_included_ranges_free(IntPtr ptr);

    // ts_tree_get_changed_ranges_free → void (释放 ts_tree_get_changed_ranges 返回的指针)
    [LibraryImport(LibraryName, EntryPoint = "ts_tree_get_changed_ranges_free")]
    public static partial void ts_tree_get_changed_ranges_free(IntPtr ptr);
}
