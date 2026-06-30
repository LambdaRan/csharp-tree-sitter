namespace TreeSitter.Wrap;

using System.Runtime.InteropServices;
using System.Text;
using TreeSitter.Bind;

/// <summary>
/// 封装 tree-sitter 的 TSNode，表示语法树中的一个节点。
/// 只读值类型，内部持有原生 TSNode + Tree owner 引用（防止 Tree 被 GC 回收）。
/// 所有导航方法返回新 Node，继承 owner 引用。
/// </summary>
public readonly struct Node : IEquatable<Node>
{
	private readonly TSNode _rawNode;
	private readonly Tree? _owner;

	internal Node(TSNode rawNode, Tree? owner)
	{
		_rawNode = rawNode;
		_owner = owner;
	}

	// ---- 类型/符号信息 ----

	/// <summary>获取节点的类型名称。</summary>
	public string Type {
		get {
			var ptr = TSApi.ts_node_type(_rawNode);
			return Marshal.PtrToStringUTF8(ptr) ?? string.Empty;
		}
	}

	/// <summary>获取节点在语法中的类型名称（忽略别名）。</summary>
	public string GrammarType {
		get {
			var ptr = TSApi.ts_node_grammar_type(_rawNode);
			return Marshal.PtrToStringUTF8(ptr) ?? string.Empty;
		}
	}

	/// <summary>获取节点的类型数值 id（TSSymbol）。</summary>
	public ushort SymbolId => TSApi.ts_node_symbol(_rawNode);

	/// <summary>获取节点在语法中的类型数值 id（忽略别名）。</summary>
	public ushort GrammarSymbolId => TSApi.ts_node_grammar_symbol(_rawNode);

	/// <summary>
	/// 获取节点的语言（借用型引用，ownsHandle=false）。
	/// <b>生命周期约束</b>：返回的 Language 必须在来源 Tree 存活期间使用。
	/// </summary>
	public Language Language {
		get {
			var langPtr = TSApi.ts_node_language(_rawNode);
			return new Language(langPtr, ownsHandle: false);
		}
	}

	// ---- 位置信息 ----

	/// <summary>节点的起始字节偏移。</summary>
	public uint StartByte => TSApi.ts_node_start_byte(_rawNode);

	/// <summary>节点的结束字节偏移。</summary>
	public uint EndByte => TSApi.ts_node_end_byte(_rawNode);

	/// <summary>节点的起始位置（行、列）。</summary>
	public Point StartPoint => new(TSApi.ts_node_start_point(_rawNode));

	/// <summary>节点的结束位置（行、列）。</summary>
	public Point EndPoint => new(TSApi.ts_node_end_point(_rawNode));

	// ---- 状态检查 ----

	/// <summary>节点是否为 null（ts_node_child 等函数在找不到节点时返回 null 节点）。</summary>
	public bool IsNull => TSApi.ts_node_is_null(_rawNode);

	/// <summary>节点是否为命名节点。</summary>
	public bool IsNamed => TSApi.ts_node_is_named(_rawNode);

	/// <summary>节点是否为缺失节点（解析器为从错误中恢复而插入）。</summary>
	public bool IsMissing => TSApi.ts_node_is_missing(_rawNode);

	/// <summary>节点是否为额外节点（如注释）。</summary>
	public bool IsExtra => TSApi.ts_node_is_extra(_rawNode);

	/// <summary>节点是否已被编辑。</summary>
	public bool HasChanges => TSApi.ts_node_has_changes(_rawNode);

	/// <summary>节点是否为语法错误或包含语法错误。</summary>
	public bool HasError => TSApi.ts_node_has_error(_rawNode);

	/// <summary>节点是否为语法错误。</summary>
	public bool IsError => TSApi.ts_node_is_error(_rawNode);

	// ---- 解析状态 ----

	/// <summary>获取此节点的解析状态（TSStateId）。</summary>
	public ushort ParseState => TSApi.ts_node_parse_state(_rawNode);

	/// <summary>获取此节点之后的解析状态。</summary>
	public ushort NextParseState => TSApi.ts_node_next_parse_state(_rawNode);

	// ---- 计数 ----

	/// <summary>节点的子节点数量。</summary>
	public uint ChildCount => TSApi.ts_node_child_count(_rawNode);

	/// <summary>节点的命名子节点数量。</summary>
	public uint NamedChildCount => TSApi.ts_node_named_child_count(_rawNode);

	/// <summary>节点的后代数量（包含节点自身，计数为 1）。</summary>
	public uint DescendantCount => TSApi.ts_node_descendant_count(_rawNode);

	// ---- 导航（返回新 Node，继承 _owner 引用）----

	/// <summary>获取节点的直接父节点。</summary>
	public Node Parent => new(TSApi.ts_node_parent(_rawNode), _owner);

	/// <summary>获取指定索引处的子节点。</summary>
	public Node Child(uint index) => new(TSApi.ts_node_child(_rawNode, index), _owner);

	/// <summary>获取指定索引处的命名子节点。</summary>
	public Node NamedChild(uint index) => new(TSApi.ts_node_named_child(_rawNode, index), _owner);

	/// <summary>获取节点的下一个兄弟节点。</summary>
	public Node NextSibling => new(TSApi.ts_node_next_sibling(_rawNode), _owner);

	/// <summary>获取节点的上一个兄弟节点。</summary>
	public Node PrevSibling => new(TSApi.ts_node_prev_sibling(_rawNode), _owner);

	/// <summary>获取节点的下一个命名兄弟节点。</summary>
	public Node NextNamedSibling => new(TSApi.ts_node_next_named_sibling(_rawNode), _owner);

	/// <summary>获取节点的上一个命名兄弟节点。</summary>
	public Node PrevNamedSibling => new(TSApi.ts_node_prev_named_sibling(_rawNode), _owner);

	/// <summary>获取包含指定 descendant 的节点。可能返回 descendant 本身。</summary>
	public Node ChildWithDescendant(Node descendant) =>
		new(TSApi.ts_node_child_with_descendant(_rawNode, descendant._rawNode), _owner);

	/// <summary>通过字段名获取子节点。</summary>
	public Node ChildByFieldName(string name)
	{
		var bytes = Encoding.UTF8.GetBytes(name);
		return new Node(TSApi.ts_node_child_by_field_name(_rawNode, bytes, (uint)bytes.Length), _owner);
	}

	/// <summary>通过数值字段 id 获取子节点。</summary>
	public Node ChildByFieldId(ushort fieldId) =>
		new(TSApi.ts_node_child_by_field_id(_rawNode, fieldId), _owner);

	/// <summary>获取第一个包含或在给定字节偏移之后开始的子节点。</summary>
	public Node FirstChildForByte(uint byteOffset) =>
		new(TSApi.ts_node_first_child_for_byte(_rawNode, byteOffset), _owner);

	/// <summary>获取第一个包含或在给定字节偏移之后开始的命名子节点。</summary>
	public Node FirstNamedChildForByte(uint byteOffset) =>
		new(TSApi.ts_node_first_named_child_for_byte(_rawNode, byteOffset), _owner);

	/// <summary>获取此节点内跨越给定字节范围的最小子节点。</summary>
	public Node DescendantForByteRange(uint start, uint end) =>
		new(TSApi.ts_node_descendant_for_byte_range(_rawNode, start, end), _owner);

	/// <summary>获取此节点内跨越给定 (行, 列) 范围的最小子节点。</summary>
	public Node DescendantForPointRange(Point start, Point end) =>
		new(TSApi.ts_node_descendant_for_point_range(_rawNode, start.ToNative(), end.ToNative()), _owner);

	/// <summary>获取此节点内跨越给定字节范围的最小命名子节点。</summary>
	public Node NamedDescendantForByteRange(uint start, uint end) =>
		new(TSApi.ts_node_named_descendant_for_byte_range(_rawNode, start, end), _owner);

	/// <summary>获取此节点内跨越给定 (行, 列) 范围的最小命名子节点。</summary>
	public Node NamedDescendantForPointRange(Point start, Point end) =>
		new(TSApi.ts_node_named_descendant_for_point_range(_rawNode, start.ToNative(), end.ToNative()), _owner);

	// ---- 字段名 ----

	/// <summary>获取指定索引处子节点的字段名。如果没有字段则返回 null。</summary>
	public string? FieldNameForChild(uint index)
	{
		var ptr = TSApi.ts_node_field_name_for_child(_rawNode, index);
		return ptr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(ptr);
	}

	/// <summary>获取指定索引处命名子节点的字段名。如果没有字段则返回 null。</summary>
	public string? FieldNameForNamedChild(uint index)
	{
		var ptr = TSApi.ts_node_field_name_for_named_child(_rawNode, index);
		return ptr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(ptr);
	}

	// ---- 编辑 ----

	/// <summary>
	/// 编辑节点以保持与已编辑源代码的同步。
	/// 拷贝 _rawNode 到局部变量后 ref 传给 ts_node_edit，再用修改后的值构造新 Node 返回。
	/// </summary>
	public Node Edit(InputEdit edit)
	{
		var mutableNode = _rawNode;
		var nativeEdit = edit.ToNative();
		TSApi.ts_node_edit(ref mutableNode, in nativeEdit);
		return new Node(mutableNode, _owner);
	}

	// ---- 字符串表示 ----

	/// <summary>
	/// 获取节点的 S 表达式字符串表示。
	/// 调用 ts_node_string 获取 malloc'd char*，拷贝后立即 ts_node_string_free 释放。
	/// </summary>
	public override string ToString()
	{
		var ptr = TSApi.ts_node_string(_rawNode);
		try {
			return ptr == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUTF8(ptr) ?? string.Empty;
		}
		finally {
			if (ptr != IntPtr.Zero) {
				TSApi.ts_node_string_free(ptr);
			}
		}
	}

	// ---- 等值比较 ----

	/// <summary>使用 ts_node_eq 比较两个节点是否相同。</summary>
	public bool Equals(Node other) => TSApi.ts_node_eq(_rawNode, other._rawNode);

	/// <summary>比较两个节点是否相同。</summary>
	public override bool Equals(object? obj) => obj is Node other && Equals(other);

	/// <summary>基于内部 _id 指针的哈希码。</summary>
	public override int GetHashCode() => _rawNode.id.GetHashCode();

	public static bool operator ==(Node left, Node right) => left.Equals(right);
	public static bool operator !=(Node left, Node right) => !left.Equals(right);

	/// <summary>获取内部原生 TSNode（供 QueryCursor.Exec 等内部使用）。</summary>
	internal TSNode RawNode => _rawNode;

	/// <summary>获取持有此节点的 Tree 引用。</summary>
	internal Tree? Owner => _owner;
}
