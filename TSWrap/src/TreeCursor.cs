namespace TreeSitter.Wrap;

using System.Runtime.InteropServices;
using TreeSitter.Bind;

/// <summary>
/// 封装 tree-sitter 的 TSTreeCursor，提供比 TSNode 函数更高效的语法树遍历。
/// class + IDisposable + finalizer，内部持有 TSTreeCursor 值类型字段。
/// </summary>
public sealed class TreeCursor : IDisposable
{
	private TSTreeCursor _cursor;
	private bool _disposed;
	private Tree? _owner;

	/// <summary>从给定节点创建树游标。给定节点被视为游标的根，游标不能走出此节点。</summary>
	public TreeCursor(Node node)
	{
		_cursor = TSApi.ts_tree_cursor_new(node.RawNode);
		_owner = node.Owner;
	}

	/// <summary>内部构造函数（从已复制的 TSTreeCursor 创建）。</summary>
	private TreeCursor(TSTreeCursor cursor, Tree? owner)
	{
		_cursor = cursor;
		_owner = owner;
	}

	// ---- 当前信息 ----

	/// <summary>获取游标的当前节点。</summary>
	public Node CurrentNode {
		get {
			var rawNode = TSApi.ts_tree_cursor_current_node(in _cursor);
			return new Node(rawNode, _owner);
		}
	}

	/// <summary>获取当前节点的字段名。如果当前节点没有字段则返回 null。</summary>
	public string? CurrentFieldName {
		get {
			var ptr = TSApi.ts_tree_cursor_current_field_name(in _cursor);
			return ptr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(ptr);
		}
	}

	/// <summary>获取当前节点的字段 id。如果没有字段则返回 0。</summary>
	public ushort CurrentFieldId => TSApi.ts_tree_cursor_current_field_id(in _cursor);

	/// <summary>获取当前节点在原始节点所有后代中的索引。</summary>
	public uint CurrentDescendantIndex => TSApi.ts_tree_cursor_current_descendant_index(in _cursor);

	/// <summary>获取当前节点相对于原始节点的深度。</summary>
	public uint CurrentDepth => TSApi.ts_tree_cursor_current_depth(in _cursor);

	// ---- 导航 ----

	/// <summary>移动到父节点。成功返回 true，已在根节点返回 false。</summary>
	public bool GotoParent() => TSApi.ts_tree_cursor_goto_parent(ref _cursor);

	/// <summary>移动到第一个子节点。成功返回 true，无子节点返回 false。</summary>
	public bool GotoFirstChild() => TSApi.ts_tree_cursor_goto_first_child(ref _cursor);

	/// <summary>移动到最后一个子节点。成功返回 true，无子节点返回 false。</summary>
	public bool GotoLastChild() => TSApi.ts_tree_cursor_goto_last_child(ref _cursor);

	/// <summary>移动到下一个兄弟节点。成功返回 true，无下一个兄弟返回 false。</summary>
	public bool GotoNextSibling() => TSApi.ts_tree_cursor_goto_next_sibling(ref _cursor);

	/// <summary>移动到上一个兄弟节点。成功返回 true，无上一个兄弟返回 false。</summary>
	public bool GotoPreviousSibling() => TSApi.ts_tree_cursor_goto_previous_sibling(ref _cursor);

	/// <summary>移动到原始节点的第 N 个后代节点。</summary>
	public void GotoDescendant(uint index) =>
		TSApi.ts_tree_cursor_goto_descendant(ref _cursor, index);

	/// <summary>移动到第一个包含或在给定字节偏移之后开始的子节点。返回子节点索引，未找到返回 -1。</summary>
	public long GotoFirstChildForByte(uint byteOffset) =>
		TSApi.ts_tree_cursor_goto_first_child_for_byte(ref _cursor, byteOffset);

	/// <summary>移动到第一个包含或在给定位置之后开始的子节点。返回子节点索引，未找到返回 -1。</summary>
	public long GotoFirstChildForPoint(Point point) =>
		TSApi.ts_tree_cursor_goto_first_child_for_point(ref _cursor, point.ToNative());

	// ---- 重置 ----

	/// <summary>重新初始化游标，回到给定节点。</summary>
	public void Reset(Node node)
	{
		TSApi.ts_tree_cursor_reset(ref _cursor, node.RawNode);
		_owner = node.Owner;
	}

	/// <summary>将游标重新初始化到另一个游标的相同位置。</summary>
	public void ResetTo(TreeCursor other) =>
		TSApi.ts_tree_cursor_reset_to(ref _cursor, in other._cursor);

	// ---- 复制 ----

	/// <summary>创建树游标的副本。</summary>
	public TreeCursor Copy()
	{
		var copied = TSApi.ts_tree_cursor_copy(in _cursor);
		return new TreeCursor(copied, _owner);
	}

	// ---- 释放 ----

	/// <summary>释放树游标及其使用的内存。</summary>
	public void Dispose()
	{
		if (!_disposed) {
			_disposed = true;
			TSApi.ts_tree_cursor_delete(ref _cursor);
			_owner = null;
		}
	}

	/// <summary>finalizer 兜底释放。</summary>
	~TreeCursor()
	{
		Dispose();
	}
}
