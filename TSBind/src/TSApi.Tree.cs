namespace TreeSitter.Native;

using System.Runtime.InteropServices;

public static partial class TSApi
{
    /// <summary>创建语法树的浅拷贝，非常快。</summary>
    // ts_tree_copy → IntPtr (需要 ts_tree_delete 释放)
    // 返回值：原 TSTree*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_tree_copy(IntPtr self); // 原 const TSTree*

    /// <summary>删除语法树，释放其使用的所有内存。</summary>
    // ts_tree_delete → void
    [LibraryImport(LibraryName)]
    public static partial void ts_tree_delete(IntPtr self); // 原 TSTree*

    /// <summary>获取语法树的根节点。</summary>
    // ts_tree_root_node → TSNode (by value)
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_tree_root_node(IntPtr self); // 原 const TSTree*

    /// <summary>获取语法树的根节点，位置按给定偏移量前移。</summary>
    // ts_tree_root_node_with_offset → TSNode (by value)
    [LibraryImport(LibraryName)]
    public static partial TSNode ts_tree_root_node_with_offset(
        IntPtr self, // 原 const TSTree*
        uint offset_bytes, TSPoint offset_extent);

    /// <summary>获取用于解析此语法树的语言。</summary>
    // ts_tree_language → IntPtr (库内部拥有，不需释放)
    // 返回值：原 const TSLanguage*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_tree_language(IntPtr self); // 原 const TSTree*

    /// <summary>获取用于解析语法树的包含范围数组。返回的指针需由调用者释放。</summary>
    // ts_tree_included_ranges → IntPtr (caller 需用 ts_tree_included_ranges_free 释放)
    // 返回值：原 TSRange*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_tree_included_ranges(
        IntPtr self, // 原 const TSTree*
        out uint length);

    /// <summary>编辑语法树以保持与已编辑源代码的同步。必须同时按字节偏移和 (行, 列) 坐标描述编辑。</summary>
    // ts_tree_edit → void, 参数 in TSInputEdit
    [LibraryImport(LibraryName)]
    public static partial void ts_tree_edit(IntPtr self, in TSInputEdit edit); // 原 TSTree*

    /// <summary>
    /// 比较已编辑的旧语法树与新语法树，返回语法结构发生变化的范围数组。
    /// 返回的数组由 malloc 分配，调用者需用 free 释放。
    /// </summary>
    // ts_tree_get_changed_ranges → IntPtr (caller 需用 ts_tree_get_changed_ranges_free 释放)
    // 返回值：原 TSRange*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_tree_get_changed_ranges(
        IntPtr old_tree, // 原 const TSTree*
        IntPtr new_tree, // 原 const TSTree*
        out uint length);

    /// <summary>将 DOT 格式的语法树描述写入给定文件。</summary>
    // ts_tree_print_dot_graph → void
    [LibraryImport(LibraryName)]
    public static partial void ts_tree_print_dot_graph(IntPtr self, int file_descriptor); // 原 const TSTree*

    // =====================
    // 释放函数（.def 中导出 free 的别名）
    // =====================

    /// <summary>释放 ts_tree_included_ranges 返回的指针。</summary>
    // ts_tree_included_ranges_free → void
    [LibraryImport(LibraryName, EntryPoint = "ts_tree_included_ranges_free")]
    public static partial void ts_tree_included_ranges_free(IntPtr ptr); // 原 TSRange*

    /// <summary>释放 ts_tree_get_changed_ranges 返回的指针。</summary>
    // ts_tree_get_changed_ranges_free → void
    [LibraryImport(LibraryName, EntryPoint = "ts_tree_get_changed_ranges_free")]
    public static partial void ts_tree_get_changed_ranges_free(IntPtr ptr); // 原 TSRange*
}
