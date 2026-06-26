using System.Runtime.InteropServices;
using System.Text;
using TreeSitter.Wrap;

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
            Console.WriteLine("=== TSWrap Integration Test ===");

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
            TestTSWrap(filetext);
        }

        [DllImport("tree-sitter-lua.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr tree_sitter_lua();

        private static readonly IntPtr _LuaLanguagePtr = tree_sitter_lua();

        public static void TestTSWrap(string filetext)
        {
            // === 1. Language 封装 ===
            using var language = new Language(_LuaLanguagePtr);
            Console.Error.WriteLine($"Language: {language.Name}");
            Console.Error.WriteLine($"  SymbolCount: {language.SymbolCount}");
            Console.Error.WriteLine($"  FieldCount: {language.FieldCount}");
            Console.Error.WriteLine($"  AbiVersion: {language.AbiVersion}");

            // === 2. Parser 封装 ===
            using var parser = new Parser(language);
            Console.Error.WriteLine("\n=== Parser Created ===");

            // === 3. Parse (string 路径, UTF-16LE) ===
            using var tree = parser.Parse(filetext);
            Console.Error.WriteLine("\n=== Parse Complete ===");

            // === 4. Tree 属性 ===
            var root = tree.RootNode;
            Console.Error.WriteLine($"RootNode Type: {root.Type}");
            Console.Error.WriteLine($"RootNode ChildCount: {root.ChildCount}");
            Console.Error.WriteLine($"RootNode StartPoint: {root.StartPoint}");
            Console.Error.WriteLine($"RootNode EndPoint: {root.EndPoint}");

            // === 5. Node 遍历 (TreeCursor) ===
            Console.Error.WriteLine("\n=== Tree View (TreeCursor) ===");
            PrintTreeWithCursor(filetext, tree);

            // === 6. Node API 演示 ===
            Console.Error.WriteLine("\n=== Node Navigation Demo ===");
            DemonstrateNodeApi(filetext, root);

            // === 7. Query 演示 ===
            Console.Error.WriteLine("\n=== Query Demo ===");
            DemonstrateQuery(language, tree);

            // === 8. IEnumerable Matches 路径 ===
            Console.Error.WriteLine("\n=== IEnumerable Matches ===");
            DemonstrateQueryMatches(language, tree);

            // === 9. Tree.Copy 和 GetChangedRanges ===
            Console.Error.WriteLine("\n=== Tree.Copy & GetChangedRanges ===");
            DemonstrateTreeCopyAndDiff(tree);

            // === 10. Language 符号查询 ===
            Console.Error.WriteLine("\n=== Language Symbol Query ===");
            DemonstrateLanguageSymbols(language);

            Console.Error.WriteLine("\n=== All Tests Passed ===");
        }

        static void PrintTreeWithCursor(string filetext, Tree tree)
        {
            var root = tree.RootNode;
            using var cursor = new TreeCursor(root);
            PrintCursorTree(filetext, cursor, tree, indent: "", isLast: true);
        }

        static void PrintCursorTree(string filetext, TreeCursor cursor, Tree tree, string indent, bool isLast)
        {
            var node = cursor.CurrentNode;
            var connector = indent.Length == 0 ? "" : (isLast ? "└── " : "├── ");

            var fieldName = cursor.CurrentFieldName;
            var fieldPrefix = fieldName != null ? $"{fieldName}: " : "";

            // 使用 UTF-16LE 模式：字节偏移 ÷ 2 = 字符索引
            var startOffset = (int)(node.StartByte / 2);
            var endOffset = (int)(node.EndByte / 2);

            const int maxLen = 40;
            var text = "";
            if (startOffset >= 0 && endOffset <= filetext.Length && endOffset > startOffset)
            {
                var span = filetext.AsSpan(startOffset, endOffset - startOffset);
                text = span.Length <= maxLen ? span.ToString() : span.Slice(0, maxLen).ToString() + "…";
                text = text.Replace("\r", "").Replace("\n", "\\n");
            }

            Console.Error.WriteLine(
                $"{indent}{connector}{fieldPrefix}{node.Type} " +
                $"({node.ChildCount} children, {node.StartPoint}-{node.EndPoint}) " +
                $"\"{text}\"");

            var childIndent = indent + (isLast ? "    " : "│   ");

            if (cursor.GotoFirstChild())
            {
                bool hasSibling = true;
                while (hasSibling)
                {
                    var nextSibling = cursor.GotoNextSibling();
                    PrintCursorTree(filetext, cursor, tree, childIndent, !nextSibling);
                    if (!nextSibling) hasSibling = false;
                }
                cursor.GotoParent();
            }
        }

        static void DemonstrateNodeApi(string filetext, Node root)
        {
            if (root.ChildCount > 0)
            {
                var firstChild = root.Child(0);
                Console.Error.WriteLine($"First child: {firstChild.Type} at {firstChild.StartPoint}");

                if (root.NamedChildCount > 0)
                {
                    var firstNamed = root.NamedChild(0);
                    Console.Error.WriteLine($"First named child: {firstNamed.Type}");
                }

                // Sibling navigation
                var sibling = firstChild.NextSibling;
                if (!sibling.IsNull)
                    Console.Error.WriteLine($"Next sibling: {sibling.Type}");

                // Parent
                var parent = firstChild.Parent;
                Console.Error.WriteLine($"Parent of first child: {parent.Type} (eq root: {parent == root})");

                // IsNamed, IsMissing, HasError
                Console.Error.WriteLine($"First child IsNamed: {firstChild.IsNamed}");
                Console.Error.WriteLine($"First child IsMissing: {firstChild.IsMissing}");
                Console.Error.WriteLine($"First child HasError: {firstChild.HasError}");

                // ToString (S-expression)
                var sexpr = firstChild.ToString();
                var displayLen = Math.Min(sexpr.Length, 80);
                Console.Error.WriteLine($"S-expression (truncated): {sexpr[..displayLen]}...");

                // Field name
                var fn = root.FieldNameForChild(0);
                Console.Error.WriteLine($"Field name for child 0: {fn ?? "(null)"}");

                // DescendantForByteRange
                if (filetext.Length > 0)
                {
                    var descendant = root.DescendantForByteRange(0, 0);
                    Console.Error.WriteLine($"Descendant for byte 0: {descendant.Type}");
                }
            }
        }

        static void DemonstrateQuery(Language language, Tree tree)
        {
            try
            {
                using var query = new Query(language, "(identifier) @ident");
                Console.Error.WriteLine($"Query compiled: PatternCount={query.PatternCount}, CaptureCount={query.CaptureCount}");

                var captureName = query.CaptureNameForId(0);
                Console.Error.WriteLine($"Capture name 0: @{captureName}");

                var predicates = query.PredicatesForPattern(0);
                Console.Error.WriteLine($"Predicates for pattern 0: {predicates.Length} steps");

                Console.Error.WriteLine($"Is pattern 0 rooted: {query.IsPatternRooted(0)}");

                var start = query.StartByteForPattern(0);
                var end = query.EndByteForPattern(0);
                Console.Error.WriteLine($"Pattern 0 byte range: {start}-{end}");

                // 零分配路径
                using var cursor = new QueryCursor();
                cursor.Exec(query, tree.RootNode);

                int matchCount = 0;
                while (cursor.NextMatch(out QueryMatch match))
                {
                    matchCount++;
                    if (matchCount <= 5)
                    {
                        foreach (var capture in match.Captures)
                        {
                            Console.Error.WriteLine($"  Match {match.Id}: @{capture.Name} = {capture.Node.Type} at {capture.Node.StartPoint}");
                        }
                    }
                }
                Console.Error.WriteLine($"Total matches (zero-alloc path): {matchCount}");
            }
            catch (QueryException ex)
            {
                Console.Error.WriteLine($"Query error: {ex.Message} (type={ex.Error}, offset={ex.ErrorOffset})");
            }
        }

        static void DemonstrateQueryMatches(Language language, Tree tree)
        {
            try
            {
                using var query = new Query(language, "(identifier) @ident");
                using var cursor = new QueryCursor();
                cursor.Exec(query, tree.RootNode);

                // IEnumerable 路径
                var matches = cursor.Matches.ToList();
                Console.Error.WriteLine($"Total matches (IEnumerable): {matches.Count}");

                foreach (var match in matches.Take(3))
                {
                    Console.Error.WriteLine($"  Match {match.Id} (pattern {match.PatternIndex}):");
                    foreach (var capture in match.Captures)
                    {
                        Console.Error.WriteLine($"    @{capture.Name} = {capture.Node.Type}");
                    }
                }
            }
            catch (QueryException ex)
            {
                Console.Error.WriteLine($"Query error: {ex.Message}");
            }
        }

        static void DemonstrateTreeCopyAndDiff(Tree tree)
        {
            using var copy = tree.Copy();
            Console.Error.WriteLine($"Tree copy root: {copy.RootNode.Type} (children: {copy.RootNode.ChildCount})");

            var changedRanges = copy.GetChangedRanges(tree);
            Console.Error.WriteLine($"Changed ranges (no edit): {changedRanges.Length}");

            var included = tree.IncludedRanges;
            Console.Error.WriteLine($"Included ranges: {included.Length}");

            using var lang = tree.Language;
            Console.Error.WriteLine($"Tree language: {lang.Name}");
        }

        static void DemonstrateLanguageSymbols(Language language)
        {
            var identSymbol = language.SymbolForName("identifier", true);
            Console.Error.WriteLine($"Symbol for 'identifier': {identSymbol}");

            var symbolName = language.SymbolName(identSymbol);
            Console.Error.WriteLine($"Symbol name for {identSymbol}: {symbolName}");

            var symbolType = language.SymbolType(identSymbol);
            Console.Error.WriteLine($"Symbol type for {identSymbol}: {symbolType}");

            var fieldCount = language.FieldCount;
            Console.Error.WriteLine($"Field count: {fieldCount}");
            if (fieldCount > 0)
            {
                var fieldName = language.FieldNameForId(1);
                Console.Error.WriteLine($"Field name for id 1: {fieldName}");
                if (fieldName != null)
                {
                    var fieldId = language.FieldIdForName(fieldName);
                    Console.Error.WriteLine($"Field id for '{fieldName}': {fieldId}");
                }
            }

            using var langCopy = language.Copy();
            Console.Error.WriteLine($"Language copy name: {langCopy.Name}");
        }
    }
}
