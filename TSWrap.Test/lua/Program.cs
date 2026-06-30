using System.Runtime.InteropServices;
using System.Text;
using TreeSitter.Wrap;

namespace TSWrap.TestLua;

internal class Program
{
	static void Main(string[] args)
	{
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		var gbk = Encoding.GetEncoding("GBK");
		Console.OutputEncoding = gbk;
		Console.SetError(new StreamWriter(Console.OpenStandardError(), gbk) { AutoFlush = true });
		Console.WriteLine("=== TSWrap Test ===");

		if (args.Length < 1) {
			Console.WriteLine("Usage: TSWrap.TestLua <file.lua>");
			return;
		}

		if (!File.Exists(args[0])) {
			Console.Error.WriteLine($"File not found: {args[0]}");
			return;
		}

		var filetext = File.ReadAllText(args[0], gbk);
		Run(filetext);
	}

	[DllImport("tree-sitter-lua.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr tree_sitter_lua();

	private static readonly IntPtr _LuaLanguagePtr = tree_sitter_lua();

	static void Run(string filetext)
	{
		// === 1. Language ===
		using var language = new Language(_LuaLanguagePtr);
		Console.Error.WriteLine($"Language: {language.Name}");
		Console.Error.WriteLine($"  SymbolCount: {language.SymbolCount}");
		Console.Error.WriteLine($"  FieldCount: {language.FieldCount}");
		Console.Error.WriteLine($"  AbiVersion: {language.AbiVersion}");

		// === 2. Parser ===
		using var parser = new Parser(language);

		// === 3. Parse (string, UTF-16LE) ===
		using var tree = parser.Parse(filetext);
		Console.Error.WriteLine($"\nParse OK — IsUtf16LE: {tree.IsUtf16LE}");

		var root = tree.RootNode;
		Console.Error.WriteLine($"Root: {root.Type}  children={root.ChildCount}  {root.StartPoint}-{root.EndPoint}");

		// === 4. TreeCursor 遍历 ===
		Console.Error.WriteLine("\n=== Tree View ===");
		PrintTree(filetext, tree);

		// === 5. Node API ===
		Console.Error.WriteLine("\n=== Node API ===");
		TestNodeApi(filetext, root);

		// === 6. Query (ref struct 路径) ===
		Console.Error.WriteLine("\n=== Query (NextMatch) ===");
		TestQueryRefStruct(language, tree);

		// === 7. Query (IEnumerable 路径) ===
		Console.Error.WriteLine("\n=== Query (Matches) ===");
		TestQueryEnumerable(language, tree);

		// === 8. Query (NextCapture 路径) ===
		Console.Error.WriteLine("\n=== Query (NextCapture) ===");
		TestQueryNextCapture(language, tree);

		// === 9. Tree.Copy / GetChangedRanges / IncludedRanges ===
		Console.Error.WriteLine("\n=== Tree Copy & Diff ===");
		TestTreeCopyAndDiff(tree);

		// === 10. Language 符号 & 字段 ===
		Console.Error.WriteLine("\n=== Language Symbols ===");
		TestLanguageSymbols(language);

		// === 11. Incremental edit ===
		Console.Error.WriteLine("\n=== Incremental Edit ===");
		TestIncrementalEdit(parser, tree);

		// === 12. Parser.IncludedRanges ===
		Console.Error.WriteLine("\n=== Parser IncludedRanges ===");
		TestParserIncludedRanges(parser);

		Console.Error.WriteLine("\n=== All Tests Passed ===");
	}

	// ---------- TreeCursor 树形打印 ----------

	static void PrintTree(string filetext, Tree tree)
	{
		using var cursor = new TreeCursor(tree.RootNode);
		PrintCursorNode(filetext, cursor, indent: "", isLast: true);
	}

	static void PrintCursorNode(string filetext, TreeCursor cursor, string indent, bool isLast)
	{
		var node = cursor.CurrentNode;
		var connector = indent.Length == 0 ? "" : (isLast ? "└── " : "├── ");
		var fieldPrefix = cursor.CurrentFieldName is { } fn ? $"{fn}: " : "";

		var start = (int)(node.StartByte / 2);
		var end = (int)(node.EndByte / 2);

		const int maxLen = 40;
		var text = "";
		if (start >= 0 && end <= filetext.Length && end > start) {
			var span = filetext.AsSpan(start, end - start);
			text = span.Length <= maxLen ? span.ToString() : span[..maxLen].ToString() + "…";
			text = text.Replace("\r", "").Replace("\n", "\\n");
		}

		Console.Error.WriteLine(
			$"{indent}{connector}{fieldPrefix}{node.Type} " +
			$"(children:{node.ChildCount} {node.StartPoint}-{node.EndPoint}) \"{text}\"");

		var childIndent = indent + (isLast ? "    " : "│   ");
		if (cursor.GotoFirstChild()) {
			while (true) {
				var hasNext = cursor.GotoNextSibling();
				PrintCursorNode(filetext, cursor, childIndent, !hasNext);
				if (!hasNext)
					break;
			}
			cursor.GotoParent();
		}
	}

	// ---------- Node API ----------

	static void TestNodeApi(string filetext, Node root)
	{
		var first = root.Child(0);
		Console.Error.WriteLine($"Child(0): {first.Type} at {first.StartPoint}");

		if (root.NamedChildCount > 0)
			Console.Error.WriteLine($"NamedChild(0): {root.NamedChild(0).Type}");

		var sibling = first.NextSibling;
		if (!sibling.IsNull)
			Console.Error.WriteLine($"NextSibling: {sibling.Type}");

		var parent = first.Parent;
		Console.Error.WriteLine($"Parent == root: {parent == root}");

		Console.Error.WriteLine($"IsNamed={first.IsNamed}  IsMissing={first.IsMissing}  IsExtra={first.IsExtra}  HasError={first.HasError}");

		var sexpr = first.ToString();
		Console.Error.WriteLine($"S-expr: {sexpr[..Math.Min(sexpr.Length, 80)]}...");

		Console.Error.WriteLine($"FieldNameForChild(0): {root.FieldNameForChild(0) ?? "(null)"}");

		var desc = root.DescendantForByteRange(0, 0);
		Console.Error.WriteLine($"DescendantForByteRange(0,0): {desc.Type}");

		// DescendantForPointRange
		var descPt = root.DescendantForPointRange(new Point(0, 0), new Point(0, 0));
		Console.Error.WriteLine($"DescendantForPointRange(0,0): {descPt.Type}");

		// NamedDescendantForByteRange
		if (filetext.Length > 2) {
			var named = root.NamedDescendantForByteRange(0, 4);
			Console.Error.WriteLine($"NamedDescendantForByteRange(0,4): {named.Type}");
		}

		// GrammarType / GrammarSymbolId
		Console.Error.WriteLine($"GrammarType: {first.GrammarType}  GrammarSymbolId: {first.GrammarSymbolId}");

		// ParseState / NextParseState
		Console.Error.WriteLine($"ParseState: {first.ParseState}  NextParseState: {first.NextParseState}");

		// DescendantCount
		Console.Error.WriteLine($"DescendantCount: {root.DescendantCount}");
	}

	// ---------- Query (ref struct) ----------

	static void TestQueryRefStruct(Language language, Tree tree)
	{
		using var query = new Query(language, "(identifier) @ident");
		Console.Error.WriteLine($"PatternCount={query.PatternCount}  CaptureCount={query.CaptureCount}");
		Console.Error.WriteLine($"CaptureName(0): @{query.CaptureNameForId(0)}");
		Console.Error.WriteLine($"IsPatternRooted(0): {query.IsPatternRooted(0)}");
		Console.Error.WriteLine($"Pattern byte range: {query.StartByteForPattern(0)}-{query.EndByteForPattern(0)}");

		using var cursor = new QueryCursor();
		cursor.Exec(query, tree.RootNode);

		int count = 0;
		while (cursor.NextMatch(out QueryMatch m)) {
			count++;
			if (count <= 3) {
				foreach (var c in m.CaptureEnumerator())
					Console.Error.WriteLine($"  Match {m.Id}: @{c.Name} = {c.Node.Type} at {c.Node.StartPoint}");
			}
		}
		Console.Error.WriteLine($"Total: {count}");
	}

	// ---------- Query (IEnumerable) ----------

	static void TestQueryEnumerable(Language language, Tree tree)
	{
		using var query = new Query(language, "(identifier) @ident");
		using var cursor = new QueryCursor();
		cursor.Exec(query, tree.RootNode);

		var matches = cursor.Matches.ToList();
		Console.Error.WriteLine($"Count: {matches.Count}");
		foreach (var m in matches.Take(3)) {
			Console.Error.WriteLine($"  Match {m.Id} (pattern {m.PatternIndex}):");
			foreach (var c in m.Captures)
				Console.Error.WriteLine($"    @{c.Name} = {c.Node.Type}");
		}

		// LINQ
		var identCount = matches.Count(m => m.Captures.Any(c => c.Name == "ident"));
		Console.Error.WriteLine($"Idents via LINQ: {identCount}");
	}

	// ---------- Query (NextCapture) ----------

	static void TestQueryNextCapture(Language language, Tree tree)
	{
		using var query = new Query(language, "(identifier) @ident");
		using var cursor = new QueryCursor();
		cursor.Exec(query, tree.RootNode);

		int count = 0;
		while (cursor.NextCapture(out QueryMatch m, out uint captureIndex)) {
			count++;
			if (count <= 3) {
				var cap = m.GetCapture((int)captureIndex);
				Console.Error.WriteLine($"  Capture {captureIndex}: @{cap.Name} = {cap.Node.Type}");
			}
		}
		Console.Error.WriteLine($"Total captures: {count}");
	}

	// ---------- Tree.Copy / GetChangedRanges ----------

	static void TestTreeCopyAndDiff(Tree tree)
	{
		using var copy = tree.Copy();
		Console.Error.WriteLine($"Copy root: {copy.RootNode.Type} (children: {copy.RootNode.ChildCount})");

		var changed = copy.GetChangedRanges(tree);
		Console.Error.WriteLine($"Changed ranges (no edit): {changed.Length}");

		var included = tree.IncludedRanges;
		Console.Error.WriteLine($"Included ranges: {included.Length}");
		if (included.Length > 0)
			Console.Error.WriteLine($"  [0]: {included[0]}");

		using var lang = tree.Language;
		Console.Error.WriteLine($"Tree.Language: {lang.Name}");

		// RootNodeWithOffset
		var offsetRoot = tree.RootNodeWithOffset(0, new Point(0, 0));
		Console.Error.WriteLine($"RootNodeWithOffset: {offsetRoot.Type}");
	}

	// ---------- Language symbols ----------

	static void TestLanguageSymbols(Language language)
	{
		var sym = language.SymbolForName("identifier", true);
		Console.Error.WriteLine($"SymbolForName('identifier', true): {sym}");
		Console.Error.WriteLine($"SymbolName({sym}): {language.SymbolName(sym)}");
		Console.Error.WriteLine($"SymbolType({sym}): {language.SymbolType(sym)}");

		Console.Error.WriteLine($"StateCount: {language.StateCount}");

		var fc = language.FieldCount;
		Console.Error.WriteLine($"FieldCount: {fc}");
		if (fc > 0) {
			var name = language.FieldNameForId(1);
			Console.Error.WriteLine($"FieldNameForId(1): {name}");
			if (name != null)
				Console.Error.WriteLine($"FieldIdForName('{name}'): {language.FieldIdForName(name)}");
		}

		using var copy = language.Copy();
		Console.Error.WriteLine($"Copy().Name: {copy.Name}");
	}

	// ---------- Incremental edit ----------

	static void TestIncrementalEdit(Parser parser, Tree tree)
	{
		// 复制一份用于编辑
		using var editedTree = tree.Copy();

		// 模拟在字节偏移 0 处插入 4 个字节（UTF-16LE → 2 个字符）
		var edit = new InputEdit(
			startByte: 0,
			oldEndByte: 0,
			newEndByte: 4,
			startPoint: new Point(0, 0),
			oldEndPoint: new Point(0, 0),
			newEndPoint: new Point(0, 2));

		editedTree.Edit(edit);

		// 编辑后的根节点应有 HasChanges 标记
		var root = editedTree.RootNode;
		Console.Error.WriteLine($"After edit — HasChanges: {root.HasChanges}");

		// 用 Node.Edit 编辑一个已缓存的 Node
		var cachedNode = tree.RootNode.Child(0);
		var editedNode = cachedNode.Edit(edit);
		Console.Error.WriteLine($"Node.Edit — original StartByte: {cachedNode.StartByte}, edited StartByte: {editedNode.StartByte}");
	}

	// ---------- Parser.IncludedRanges ----------

	static void TestParserIncludedRanges(Parser parser)
	{
		// 默认：解析全部
		var defaultRanges = parser.IncludedRanges;
		Console.Error.WriteLine($"Default IncludedRanges count: {defaultRanges.Length}");

		// 设置限制范围
		var ranges = new[] { new TreeSitter.Wrap.Range(new Point(0, 0), new Point(10, 0), 0, 20) };
		parser.IncludedRanges = ranges;

		var setRanges = parser.IncludedRanges;
		Console.Error.WriteLine($"After set — count: {setRanges.Length}");
		if (setRanges.Length > 0)
			Console.Error.WriteLine($"  [0]: {setRanges[0]}");

		// 恢复默认
		parser.IncludedRanges = Array.Empty<TreeSitter.Wrap.Range>();
		Console.Error.WriteLine($"After reset — count: {parser.IncludedRanges.Length}");
	}
}
