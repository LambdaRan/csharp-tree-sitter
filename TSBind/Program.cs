using System.Runtime.InteropServices;
using System.Text;
using TreeSitter.Native;

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
			Console.WriteLine("Hello, Tree-Sitter!");
			if (args.Length < 1) {
				Console.WriteLine("Usage: TSBind <file.lua>");
				return;
			}

			if (!File.Exists(args[0])) {
				Console.Error.WriteLine("File not found.");
				return;
			}
			TraverseTree(File.ReadAllText(args[0], gbk));
		}

		[DllImport("tree-sitter-lua.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr tree_sitter_lua();

		private static IntPtr _LuaLanguage = tree_sitter_lua();

		public static bool TraverseTree(string filetext)
		{
			var parser = TSApi.ts_parser_new();
			try {
				TSApi.ts_parser_set_language(parser, _LuaLanguage);
				// 转为 UTF-16LE 字节缓冲区，tree-sitter 返回的字节偏移 ÷ 2 = C# 字符串索引
				byte[] utf16 = Encoding.Unicode.GetBytes(filetext);
				var tree = TSApi.ts_parser_parse_string_encoding(
					parser, IntPtr.Zero, utf16, (uint)utf16.Length,
					TSInputEncoding.TSInputEncodingUTF16LE);
				if (tree == IntPtr.Zero) {
					Console.Error.WriteLine("Failed to parse the file.");
					return false;
				}
				try {
					PostOrderTraverse(filetext, TSApi.ts_tree_root_node(tree));
				}
				finally {
					TSApi.ts_tree_delete(tree);
				}
			}
			finally {
				TSApi.ts_parser_delete(parser);
			}
			return true;
		}

		public static void PostOrderTraverse(string filetext, TSNode node)
		{
			// 递归后序遍历
			uint childCount = TSApi.ts_node_child_count(node);
			for (uint i = 0; i < childCount; i++) {
				PostOrderTraverse(filetext, TSApi.ts_node_child(node, i));
			}

			// 再打印当前节点
			var startOffset = TSApi.ts_node_start_byte(node) / sizeof(ushort);
			var endOffset = TSApi.ts_node_end_byte(node) / sizeof(ushort);
			var point = TSApi.ts_node_start_point(node);
			var isymbol = TSApi.ts_node_symbol(node);
			var type = isymbol == ushort.MaxValue
				? "ERROR"
				: Marshal.PtrToStringUTF8(TSApi.ts_language_symbol_name(_LuaLanguage, isymbol));
			var span = filetext.AsSpan((int)startOffset, (int)(endOffset - startOffset));
			Console.Error.WriteLine($"Node -> point:{point.row+1}-{point.column} type: {type}, symbol: {span.ToString()}");
		}
	}
}
