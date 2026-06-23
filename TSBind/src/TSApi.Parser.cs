namespace TreeSitter.Native;

using System.Runtime.InteropServices;

public static partial class TSApi
{
    public const string LibraryName = "tree-sitter";

    // ts_parser_new → IntPtr (需用 ts_parser_delete 释放)
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_parser_new();

    // ts_parser_delete → void
    [LibraryImport(LibraryName)]
    public static partial void ts_parser_delete(IntPtr self);

    // ts_parser_language → IntPtr (库内部拥有，不需释放，返回 const TSLanguage*)
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_parser_language(IntPtr self);

    // ts_parser_set_language → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_parser_set_language(IntPtr self, IntPtr language);

    // ts_parser_set_included_ranges → bool, TSRange[] 为 blittable 数组自动 pin
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_parser_set_included_ranges(
        IntPtr self, TSRange[] ranges, uint count);

    // ts_parser_included_ranges → IntPtr (库内部拥有，不需释放，返回 const TSRange*)
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_parser_included_ranges(IntPtr self, out uint count);

    // ts_parser_parse → IntPtr (需用 ts_tree_delete 释放，返回 TSTree*)
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_parser_parse(IntPtr self, IntPtr old_tree, TSInput input);

    // ts_parser_parse_with_options → IntPtr (需用 ts_tree_delete 释放，返回 TSTree*)
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_parser_parse_with_options(
        IntPtr self, IntPtr old_tree, TSInput input, TSParseOptions parse_options);

    // ts_parser_parse_string → IntPtr (需用 ts_tree_delete 释放，返回 TSTree*)
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_parser_parse_string(
        IntPtr self, IntPtr old_tree,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string str, uint length);

    // ts_parser_parse_string_encoding → IntPtr (需用 ts_tree_delete 释放，返回 TSTree*)
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_parser_parse_string_encoding(
        IntPtr self, IntPtr old_tree,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string str, uint length,
        TSInputEncoding encoding);

    // ts_parser_reset → void
    [LibraryImport(LibraryName)]
    public static partial void ts_parser_reset(IntPtr self);

    // ts_parser_set_logger → void
    [LibraryImport(LibraryName)]
    public static partial void ts_parser_set_logger(IntPtr self, TSLogger logger);

    // ts_parser_logger → TSLogger (by value 返回)
    [LibraryImport(LibraryName)]
    public static partial TSLogger ts_parser_logger(IntPtr self);

    // ts_parser_print_dot_graphs → void
    [LibraryImport(LibraryName)]
    public static partial void ts_parser_print_dot_graphs(IntPtr self, int fd);
}
