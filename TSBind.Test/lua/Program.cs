using System.Runtime.InteropServices;
using System.Text;
using TreeSitter.Bind;

namespace TSBind
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var gbk = Encoding.GetEncoding("GBK");
            Console.OutputEncoding = gbk;
            Console.SetError(new StreamWriter(Console.OpenStandardError(), gbk) { AutoFlush = true });
            Console.WriteLine("=== TSBind Integration Test ===");

            if (args.Length < 1)
            {
                Console.WriteLine("Usage: TSBind <file.lua>");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.Error.WriteLine("File not found.");
                return;
            }

            var filetext = File.ReadAllText(args[0], gbk);
            TestTSBind(filetext);
        }

        [DllImport("tree-sitter-lua.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tree_sitter_lua();

        private static readonly IntPtr _LuaLanguagePtr = tree_sitter_lua();

        // ---- 辅助方法 ----

        static string ReadUtf8(IntPtr ptr) =>
            ptr == IntPtr.Zero ? "(null)" : Marshal.PtrToStringUTF8(ptr) ?? "";

        static string FormatPoint(TSPoint p) => $"({p.row}, {p.column})";

        public static void TestTSBind(string filetext)
        {
            // === 1. Language ===
            var language = TSApi.ts_language_copy(_LuaLanguagePtr);
            try
            {
                Console.Error.WriteLine($"Language: {ReadUtf8(TSApi.ts_language_name(language))}");
                Console.Error.WriteLine($"  SymbolCount: {TSApi.ts_language_symbol_count(language)}");
                Console.Error.WriteLine($"  FieldCount: {TSApi.ts_language_field_count(language)}");
                Console.Error.WriteLine($"  AbiVersion: {TSApi.ts_language_abi_version(language)}");

                // === 2. Parser ===
                var parser = TSApi.ts_parser_new();
                try
                {
                    TSApi.ts_parser_set_language(parser, language);
                    Console.Error.WriteLine("\n=== Parser Created ===");

                    // === 3. Parse (UTF-16LE) ===
                    var utf16 = Encoding.Unicode.GetBytes(filetext);
                    var tree = TSApi.ts_parser_parse_string_encoding(
                        parser, IntPtr.Zero, utf16, (uint)utf16.Length,
                        TSInputEncoding.TSInputEncodingUTF16LE);
                    if (tree == IntPtr.Zero)
                    {
                        Console.Error.WriteLine("Parse failed.");
                        return;
                    }
                    try
                    {
                        Console.Error.WriteLine("\n=== Parse Complete ===");

                        // === 4. Tree 属性 ===
                        var root = TSApi.ts_tree_root_node(tree);
                        Console.Error.WriteLine($"RootNode Type: {ReadUtf8(TSApi.ts_node_type(root))}");
                        Console.Error.WriteLine($"RootNode ChildCount: {TSApi.ts_node_child_count(root)}");
                        Console.Error.WriteLine($"RootNode StartPoint: {FormatPoint(TSApi.ts_node_start_point(root))}");
                        Console.Error.WriteLine($"RootNode EndPoint: {FormatPoint(TSApi.ts_node_end_point(root))}");

                        // === 5. TreeCursor 遍历 ===
                        Console.Error.WriteLine("\n=== Tree View (TreeCursor) ===");
                        PrintTreeWithCursor(filetext, root);

                        // === 6. Node API ===
                        Console.Error.WriteLine("\n=== Node Navigation Demo ===");
                        DemonstrateNodeApi(filetext, root);

                        // === 7. Query (零分配路径) ===
                        Console.Error.WriteLine("\n=== Query Demo ===");
                        DemonstrateQuery(language, tree, root);

                        // === 8. Query (手动迭代) ===
                        Console.Error.WriteLine("\n=== Query Matches ===");
                        DemonstrateQueryMatches(language, tree, root);

                        // === 9. Tree.Copy & GetChangedRanges ===
                        Console.Error.WriteLine("\n=== Tree.Copy & GetChangedRanges ===");
                        DemonstrateTreeCopyAndDiff(tree);

                        // === 10. Language 符号查询 ===
                        Console.Error.WriteLine("\n=== Language Symbol Query ===");
                        DemonstrateLanguageSymbols(language);

                        Console.Error.WriteLine("\n=== All Tests Passed ===");
                    }
                    finally { TSApi.ts_tree_delete(tree); }
                }
                finally { TSApi.ts_parser_delete(parser); }
            }
            finally { TSApi.ts_language_delete(language); }
        }

        static void PrintTreeWithCursor(string filetext, TSNode root)
        {
            var cursor = TSApi.ts_tree_cursor_new(root);
            try
            {
                PrintCursorTree(filetext, ref cursor, indent: "", isLast: true);
            }
            finally { TSApi.ts_tree_cursor_delete(ref cursor); }
        }

        static void PrintCursorTree(string filetext, ref TSTreeCursor cursor, string indent, bool isLast)
        {
            var node = TSApi.ts_tree_cursor_current_node(in cursor);
            var connector = indent.Length == 0 ? "" : (isLast ? "└── " : "├── ");

            var fieldNamePtr = TSApi.ts_tree_cursor_current_field_name(in cursor);
            var fieldName = fieldNamePtr != IntPtr.Zero ? ReadUtf8(fieldNamePtr) : null;
            var fieldPrefix = fieldName != null ? $"{fieldName}: " : "";

            // UTF-16LE: 字节偏移 ÷ 2 = 字符索引
            var startOffset = (int)(TSApi.ts_node_start_byte(node) / 2);
            var endOffset = (int)(TSApi.ts_node_end_byte(node) / 2);

            const int maxLen = 40;
            var text = "";
            if (startOffset >= 0 && endOffset <= filetext.Length && endOffset > startOffset)
            {
                var span = filetext.AsSpan(startOffset, endOffset - startOffset);
                text = span.Length <= maxLen ? span.ToString() : span.Slice(0, maxLen).ToString() + "…";
                text = text.Replace("\r", "").Replace("\n", "\\n");
            }

            var nodeType = ReadUtf8(TSApi.ts_node_type(node));
            var childCount = TSApi.ts_node_child_count(node);
            var startPoint = FormatPoint(TSApi.ts_node_start_point(node));
            var endPoint = FormatPoint(TSApi.ts_node_end_point(node));

            Console.Error.WriteLine(
                $"{indent}{connector}{fieldPrefix}{nodeType} " +
                $"({childCount} children, {startPoint}-{endPoint}) " +
                $"\"{text}\"");

            var childIndent = indent + (isLast ? "    " : "│   ");

            if (TSApi.ts_tree_cursor_goto_first_child(ref cursor))
            {
                bool hasSibling = true;
                while (hasSibling)
                {
                    var nextSibling = TSApi.ts_tree_cursor_goto_next_sibling(ref cursor);
                    PrintCursorTree(filetext, ref cursor, childIndent, !nextSibling);
                    if (!nextSibling) hasSibling = false;
                }
                TSApi.ts_tree_cursor_goto_parent(ref cursor);
            }
        }

        static void DemonstrateNodeApi(string filetext, TSNode root)
        {
            var childCount = TSApi.ts_node_child_count(root);
            if (childCount > 0)
            {
                var firstChild = TSApi.ts_node_child(root, 0);
                Console.Error.WriteLine($"First child: {ReadUtf8(TSApi.ts_node_type(firstChild))} at {FormatPoint(TSApi.ts_node_start_point(firstChild))}");

                var namedChildCount = TSApi.ts_node_named_child_count(root);
                if (namedChildCount > 0)
                {
                    var firstNamed = TSApi.ts_node_named_child(root, 0);
                    Console.Error.WriteLine($"First named child: {ReadUtf8(TSApi.ts_node_type(firstNamed))}");
                }

                // Sibling
                var sibling = TSApi.ts_node_next_sibling(firstChild);
                if (!TSApi.ts_node_is_null(sibling))
                    Console.Error.WriteLine($"Next sibling: {ReadUtf8(TSApi.ts_node_type(sibling))}");

                // Parent
                var parent = TSApi.ts_node_parent(firstChild);
                Console.Error.WriteLine($"Parent of first child: {ReadUtf8(TSApi.ts_node_type(parent))} (eq root: {TSApi.ts_node_eq(parent, root)})");

                // IsNamed, IsMissing, HasError
                Console.Error.WriteLine($"First child IsNamed: {TSApi.ts_node_is_named(firstChild)}");
                Console.Error.WriteLine($"First child IsMissing: {TSApi.ts_node_is_missing(firstChild)}");
                Console.Error.WriteLine($"First child HasError: {TSApi.ts_node_has_error(firstChild)}");

                // S-expression
                var sexprPtr = TSApi.ts_node_string(firstChild);
                try
                {
                    var sexpr = ReadUtf8(sexprPtr);
                    var displayLen = Math.Min(sexpr.Length, 80);
                    Console.Error.WriteLine($"S-expression (truncated): {sexpr[..displayLen]}...");
                }
                finally { TSApi.ts_node_string_free(sexprPtr); }

                // Field name
                var fnPtr = TSApi.ts_node_field_name_for_child(root, 0);
                Console.Error.WriteLine($"Field name for child 0: {(fnPtr != IntPtr.Zero ? ReadUtf8(fnPtr) : "(null)")}");

                // DescendantForByteRange
                if (filetext.Length > 0)
                {
                    var descendant = TSApi.ts_node_descendant_for_byte_range(root, 0, 0);
                    Console.Error.WriteLine($"Descendant for byte 0: {ReadUtf8(TSApi.ts_node_type(descendant))}");
                }
            }
        }

        static void DemonstrateQuery(IntPtr language, IntPtr tree, TSNode root)
        {
            var querySource = "(identifier) @ident"u8;
            var queryPtr = TSApi.ts_query_new(language, querySource, (uint)querySource.Length,
                out uint errorOffset, out TSQueryError errorType);

            if (queryPtr == IntPtr.Zero)
            {
                Console.Error.WriteLine($"Query error: type={errorType}, offset={errorOffset}");
                return;
            }
            try
            {
                Console.Error.WriteLine($"Query compiled: PatternCount={TSApi.ts_query_pattern_count(queryPtr)}, CaptureCount={TSApi.ts_query_capture_count(queryPtr)}");

                var captureNamePtr = TSApi.ts_query_capture_name_for_id(queryPtr, 0, out uint nameLen);
                Console.Error.WriteLine($"Capture name 0: @{ReadUtf8(captureNamePtr)}");

                var predicatesPtr = TSApi.ts_query_predicates_for_pattern(queryPtr, 0, out uint stepCount);
                Console.Error.WriteLine($"Predicates for pattern 0: {stepCount} steps");

                Console.Error.WriteLine($"Is pattern 0 rooted: {TSApi.ts_query_is_pattern_rooted(queryPtr, 0)}");

                var start = TSApi.ts_query_start_byte_for_pattern(queryPtr, 0);
                var end = TSApi.ts_query_end_byte_for_pattern(queryPtr, 0);
                Console.Error.WriteLine($"Pattern 0 byte range: {start}-{end}");

                // 零分配路径：直接迭代 TSQueryMatch
                var cursor = TSApi.ts_query_cursor_new();
                try
                {
                    TSApi.ts_query_cursor_exec(cursor, queryPtr, root);

                    int matchCount = 0;
                    while (TSApi.ts_query_cursor_next_match(cursor, out TSQueryMatch match))
                    {
                        matchCount++;
                        if (matchCount <= 5)
                        {
                            var nativeSize = Marshal.SizeOf<TSQueryCapture>();
                            for (int i = 0; i < match.capture_count; i++)
                            {
                                var cap = Marshal.PtrToStructure<TSQueryCapture>(
                                    match.captures + i * nativeSize);
                                var capNamePtr = TSApi.ts_query_capture_name_for_id(queryPtr, cap.index, out _);
                                Console.Error.WriteLine(
                                    $"  Match {match.id}: @{ReadUtf8(capNamePtr)} = " +
                                    $"{ReadUtf8(TSApi.ts_node_type(cap.node))} at {FormatPoint(TSApi.ts_node_start_point(cap.node))}");
                            }
                        }
                    }
                    Console.Error.WriteLine($"Total matches: {matchCount}");
                }
                finally { TSApi.ts_query_cursor_delete(cursor); }
            }
            finally { TSApi.ts_query_delete(queryPtr); }
        }

        static void DemonstrateQueryMatches(IntPtr language, IntPtr tree, TSNode root)
        {
            var querySource = "(identifier) @ident"u8;
            var queryPtr = TSApi.ts_query_new(language, querySource, (uint)querySource.Length,
                out uint errorOffset, out TSQueryError errorType);

            if (queryPtr == IntPtr.Zero)
            {
                Console.Error.WriteLine($"Query error: type={errorType}, offset={errorOffset}");
                return;
            }
            try
            {
                var cursor = TSApi.ts_query_cursor_new();
                try
                {
                    TSApi.ts_query_cursor_exec(cursor, queryPtr, root);

                    // 收集所有匹配
                    var allMatches = new List<(uint id, ushort patternIndex, List<(string name, string type)> captures)>();
                    var nativeSize = Marshal.SizeOf<TSQueryCapture>();

                    while (TSApi.ts_query_cursor_next_match(cursor, out TSQueryMatch match))
                    {
                        var caps = new List<(string, string)>();
                        for (int i = 0; i < match.capture_count; i++)
                        {
                            var cap = Marshal.PtrToStructure<TSQueryCapture>(
                                match.captures + i * nativeSize);
                            var namePtr = TSApi.ts_query_capture_name_for_id(queryPtr, cap.index, out _);
                            caps.Add((ReadUtf8(namePtr), ReadUtf8(TSApi.ts_node_type(cap.node))));
                        }
                        allMatches.Add((match.id, match.pattern_index, caps));
                    }

                    Console.Error.WriteLine($"Total matches (collected): {allMatches.Count}");

                    foreach (var m in allMatches.Take(3))
                    {
                        Console.Error.WriteLine($"  Match {m.id} (pattern {m.patternIndex}):");
                        foreach (var c in m.captures)
                            Console.Error.WriteLine($"    @{c.name} = {c.type}");
                    }
                }
                finally { TSApi.ts_query_cursor_delete(cursor); }
            }
            finally { TSApi.ts_query_delete(queryPtr); }
        }

        static void DemonstrateTreeCopyAndDiff(IntPtr tree)
        {
            var copy = TSApi.ts_tree_copy(tree);
            try
            {
                var copyRoot = TSApi.ts_tree_root_node(copy);
                Console.Error.WriteLine($"Tree copy root: {ReadUtf8(TSApi.ts_node_type(copyRoot))} (children: {TSApi.ts_node_child_count(copyRoot)})");

                var changedPtr = TSApi.ts_tree_get_changed_ranges(tree, copy, out uint changedCount);
                try
                {
                    Console.Error.WriteLine($"Changed ranges (no edit): {changedCount}");
                }
                finally { TSApi.ts_tree_get_changed_ranges_free(changedPtr); }

                var includedPtr = TSApi.ts_tree_included_ranges(tree, out uint includedCount);
                try
                {
                    Console.Error.WriteLine($"Included ranges: {includedCount}");
                }
                finally { TSApi.ts_tree_included_ranges_free(includedPtr); }

                var langPtr = TSApi.ts_tree_language(tree);
                Console.Error.WriteLine($"Tree language: {ReadUtf8(TSApi.ts_language_name(langPtr))}");
            }
            finally { TSApi.ts_tree_delete(copy); }
        }

        static void DemonstrateLanguageSymbols(IntPtr language)
        {
            var identName = "identifier"u8;
            var identSymbol = TSApi.ts_language_symbol_for_name(language, identName, (uint)identName.Length, true);
            Console.Error.WriteLine($"Symbol for 'identifier': {identSymbol}");

            var symbolName = ReadUtf8(TSApi.ts_language_symbol_name(language, identSymbol));
            Console.Error.WriteLine($"Symbol name for {identSymbol}: {symbolName}");

            var symbolType = TSApi.ts_language_symbol_type(language, identSymbol);
            Console.Error.WriteLine($"Symbol type for {identSymbol}: {symbolType}");

            var fieldCount = TSApi.ts_language_field_count(language);
            Console.Error.WriteLine($"Field count: {fieldCount}");
            if (fieldCount > 0)
            {
                var fieldName = ReadUtf8(TSApi.ts_language_field_name_for_id(language, 1));
                Console.Error.WriteLine($"Field name for id 1: {fieldName}");
                if (fieldName != "(null)")
                {
                    var fieldNameBytes = Encoding.UTF8.GetBytes(fieldName);
                    var fieldId = TSApi.ts_language_field_id_for_name(language, fieldNameBytes, (uint)fieldNameBytes.Length);
                    Console.Error.WriteLine($"Field id for '{fieldName}': {fieldId}");
                }
            }

            var langCopy = TSApi.ts_language_copy(language);
            try
            {
                Console.Error.WriteLine($"Language copy name: {ReadUtf8(TSApi.ts_language_name(langCopy))}");
            }
            finally { TSApi.ts_language_delete(langCopy); }
        }
    }
}
