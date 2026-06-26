namespace TreeSitter.Wrap;

/// <summary>
/// 表示查询中的一个捕获结果（节点 + 捕获名称）。
/// </summary>
public readonly struct QueryCapture
{
    /// <summary>被捕获的节点。</summary>
    public Node Node { get; }

    /// <summary>捕获名称（对应查询中的 @name）。</summary>
    public string Name { get; }

    internal QueryCapture(Node node, string name)
    {
        Node = node;
        Name = name;
    }
}
