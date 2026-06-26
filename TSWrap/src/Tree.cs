namespace TreeSitter.Wrap;

using System.Runtime.InteropServices;
using TreeSitter.Bind;

/// <summary>
/// 封装 tree-sitter 的 TSTree，提供对语法树的访问。
/// </summary>
public sealed class Tree : IDisposable
{
    internal readonly TreeHandle _handle;
    internal readonly bool _utf16LEMode;

    /// <summary>
    /// 指示此 Tree 是否使用 UTF-16LE 模式解析。
    /// 若为 true，<see cref="Node.StartByte"/>/<see cref="Node.EndByte"/> 返回的字节偏移 ÷ 2 即为 C# 字符串字符索引。
    /// </summary>
    public bool IsUtf16LE => _utf16LEMode;

    /// <summary>内部构造函数。</summary>
    internal Tree(IntPtr treePtr, bool utf16LEMode)
    {
        _handle = new TreeHandle(treePtr);
        _utf16LEMode = utf16LEMode;
    }

    /// <summary>内部访问原生指针。</summary>
    internal IntPtr NativeHandle => _handle.DangerousGetHandle();

    /// <summary>
    /// 获取语法树的根节点。每次访问调用 <c>ts_tree_root_node</c>，不缓存。
    /// Tree.Edit() 后自动返回更新后的根节点。
    /// </summary>
    public Node RootNode
    {
        get
        {
            var rawNode = TSApi.ts_tree_root_node(NativeHandle);
            return new Node(rawNode, this);
        }
    }

    /// <summary>获取带偏移的根节点。</summary>
    public Node RootNodeWithOffset(uint offsetBytes, Point offsetExtent)
    {
        var rawNode = TSApi.ts_tree_root_node_with_offset(
            NativeHandle, offsetBytes, offsetExtent.ToNative());
        return new Node(rawNode, this);
    }

    /// <summary>
    /// 编辑语法树以保持与已编辑源代码的同步。
    /// 必须同时按字节偏移和 (行, 列) 坐标描述编辑。
    /// </summary>
    public void Edit(InputEdit edit)
    {
        var nativeEdit = edit.ToNative();
        TSApi.ts_tree_edit(NativeHandle, in nativeEdit);
    }

    /// <summary>创建语法树的浅拷贝，非常快。</summary>
    public Tree Copy()
    {
        var copied = TSApi.ts_tree_copy(NativeHandle);
        return new Tree(copied, _utf16LEMode);
    }

    /// <summary>
    /// 获取用于解析此语法树的语言（借用型引用，ownsHandle=false）。
    /// <b>生命周期约束</b>：返回的 Language 必须在此 Tree 存活期间使用。
    /// </summary>
    public Language Language
    {
        get
        {
            var langPtr = TSApi.ts_tree_language(NativeHandle);
            return new Language(langPtr, ownsHandle: false);
        }
    }

    /// <summary>
    /// 比较已编辑的旧语法树与新语法树，返回语法结构发生变化的范围数组。
    /// 内部使用 malloc 分配的内存，拷贝后立即释放。
    /// </summary>
    public Range[] GetChangedRanges(Tree oldTree)
    {
        var ptr = TSApi.ts_tree_get_changed_ranges(
            oldTree.NativeHandle, NativeHandle, out uint count);

        try
        {
            if (ptr == IntPtr.Zero || count == 0)
                return Array.Empty<Range>();

            return CopyNativeRangeArray(ptr, count);
        }
        finally
        {
            if (ptr != IntPtr.Zero)
                TSApi.ts_tree_get_changed_ranges_free(ptr);
        }
    }

    /// <summary>
    /// 获取用于解析语法树的包含范围数组。
    /// 内部拷贝后调用 ts_tree_included_ranges_free 释放原生数组。
    /// </summary>
    public Range[] IncludedRanges
    {
        get
        {
            var ptr = TSApi.ts_tree_included_ranges(NativeHandle, out uint count);
            try
            {
                if (ptr == IntPtr.Zero || count == 0)
                    return Array.Empty<Range>();
                return CopyNativeRangeArray(ptr, count);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    TSApi.ts_tree_included_ranges_free(ptr);
            }
        }
    }

    /// <summary>辅助方法：将 malloc'd 的 TSRange 数组拷贝为 Wrap Range[]。</summary>
    private static Range[] CopyNativeRangeArray(IntPtr ptr, uint count)
    {
        var result = new Range[count];
        var nativeSize = Marshal.SizeOf<TSRange>();
        for (uint i = 0; i < count; i++)
        {
            var native = Marshal.PtrToStructure<TSRange>(ptr + (int)(i * nativeSize));
            result[i] = new Range(native);
        }
        return result;
    }

    /// <summary>释放语法树及其使用的所有内存。</summary>
    public void Dispose() => _handle.Dispose();
}
