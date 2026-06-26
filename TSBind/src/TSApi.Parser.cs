namespace TreeSitter.Bind;

using System.Runtime.InteropServices;

public static partial class TSApi
{
    public const string LibraryName = "tree-sitter";

    /// <summary>创建一个新的解析器。</summary>
    // ts_parser_new → IntPtr (需用 ts_parser_delete 释放)
    // 返回值：原 TSParser*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_parser_new();

    /// <summary>删除解析器，释放其使用的所有内存。</summary>
    // ts_parser_delete → void
    [LibraryImport(LibraryName)]
    public static partial void ts_parser_delete(IntPtr self); // 原 TSParser*

    /// <summary>获取解析器当前的语言。</summary>
    // ts_parser_language → IntPtr (库内部拥有，不需释放)
    // 返回值：原 const TSLanguage*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_parser_language(IntPtr self); // 原 const TSParser*

    /// <summary>
    /// 设置解析器使用的语言。
    /// 如果语言的 ABI 版本与库不兼容，返回 false。
    /// </summary>
    // ts_parser_set_language → bool
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_parser_set_language(
        IntPtr self, // 原 TSParser*
        IntPtr language); // 原 const TSLanguage*

    /// <summary>
    /// 设置解析器解析时包含的文本范围。
    /// 默认解析整个文档。通过此方法可以只解析文档的一部分，
    /// 同时返回的语法树范围仍与整个文档匹配。也可以传入多个不连续的范围。
    /// </summary>
    // ts_parser_set_included_ranges → bool, TSRange[] 为 blittable 数组自动 pin
    [LibraryImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool ts_parser_set_included_ranges(
        IntPtr self, // 原 TSParser*
        TSRange[] ranges, uint count);

    /// <summary>获取解析器解析时将包含的文本范围。返回的指针由解析器拥有，调用者不应释放或写入。</summary>
    // ts_parser_included_ranges → IntPtr (库内部拥有，不需释放)
    // 返回值：原 const TSRange*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_parser_included_ranges(
        IntPtr self, // 原 const TSParser*
        out uint count);

    /// <summary>
    /// 使用解析器解析源代码并创建语法树。
    /// 首次解析时 old_tree 传 IntPtr.Zero；增量解析时传入已编辑的旧语法树以复用未变更的部分。
    /// </summary>
    // ts_parser_parse → IntPtr (需用 ts_tree_delete 释放)
    // 返回值：原 TSTree*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_parser_parse(
        IntPtr self, // 原 TSParser*
        IntPtr old_tree, // 原 const TSTree*
        TSInput input);

    /// <summary>使用解析器解析源代码并创建语法树，可附加解析选项（如进度回调）。</summary>
    // ts_parser_parse_with_options → IntPtr (需用 ts_tree_delete 释放)
    // 返回值：原 TSTree*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_parser_parse_with_options(
        IntPtr self, // 原 TSParser*
        IntPtr old_tree, // 原 const TSTree*
        TSInput input, TSParseOptions parse_options);

    /// <summary>使用解析器解析存储在连续缓冲区中的源代码字符串（UTF-8 编码）。</summary>
    // ts_parser_parse_string → IntPtr (需用 ts_tree_delete 释放)
    // 返回值：原 TSTree*
    // 注意：str 为 UTF-8 字节缓冲区。返回的节点偏移也是 UTF-8 字节偏移。
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_parser_parse_string(
        IntPtr self, // 原 TSParser*
        IntPtr old_tree, // 原 const TSTree*
        ReadOnlySpan<byte> str, uint length);

    /// <summary>使用解析器解析连续缓冲区中的源代码，可指定编码（UTF8 或 UTF16LE）。</summary>
    // ts_parser_parse_string_encoding → IntPtr (需用 ts_tree_delete 释放)
    // 返回值：原 TSTree*
    // 注意：str 为原始字节缓冲区，encoding 指定其编码格式。
    //       使用 UTF16LE 时，返回的字节偏移 ÷ 2 即为 C# 字符串索引。
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_parser_parse_string_encoding(
        IntPtr self, // 原 TSParser*
        IntPtr old_tree, // 原 const TSTree*
        ReadOnlySpan<byte> str, uint length,
        TSInputEncoding encoding);

    /// <summary>
    /// 重置解析器，使下次解析从头开始。
    /// 如果之前因进度回调取消了解析，且不想从断点恢复而是解析新文档，必须先调用此方法。
    /// </summary>
    // ts_parser_reset → void
    [LibraryImport(LibraryName)]
    public static partial void ts_parser_reset(IntPtr self); // 原 TSParser*

    /// <summary>设置解析器在解析过程中使用的日志记录器。解析器不拥有日志记录器的所有权。</summary>
    // ts_parser_set_logger → void
    [LibraryImport(LibraryName)]
    public static partial void ts_parser_set_logger(IntPtr self, TSLogger logger); // 原 TSParser*

    /// <summary>获取解析器当前的日志记录器。</summary>
    // ts_parser_logger → TSLogger (by value 返回)
    [LibraryImport(LibraryName)]
    public static partial TSLogger ts_parser_logger(IntPtr self); // 原 const TSParser*

    /// <summary>设置解析器写入 DOT 格式调试图的文件描述符。传入负数可关闭此日志。</summary>
    // ts_parser_print_dot_graphs → void
    [LibraryImport(LibraryName)]
    public static partial void ts_parser_print_dot_graphs(IntPtr self, int fd); // 原 TSParser*
}
