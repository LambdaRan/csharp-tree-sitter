namespace TreeSitter.Wrap;

/// <summary>
/// 查询匹配结果（ref struct），Captures 为 ReadOnlySpan&lt;QueryCapture&gt;。
/// 由于 QueryCapture 包含托管 string 字段，内部仍需分配数组。
/// 主要用于 NextMatch/NextCapture 的 out 参数路径。
/// </summary>
public ref struct QueryMatch
{
    /// <summary>匹配 id。</summary>
    public int Id { get; }

    /// <summary>匹配的模式索引。</summary>
    public ushort PatternIndex { get; }

    /// <summary>捕获结果（ReadOnlySpan，底层为新分配的数组）。</summary>
    public ReadOnlySpan<QueryCapture> Captures { get; }

    internal QueryMatch(int id, ushort patternIndex, ReadOnlySpan<QueryCapture> captures)
    {
        Id = id;
        PatternIndex = patternIndex;
        Captures = captures;
    }
}

/// <summary>
/// 有分配查询匹配结果（普通 struct），可跨迭代安全使用，支持 LINQ。
/// Captures 为 QueryCapture[]，每次迭代独立拷贝。
/// </summary>
public readonly struct QueryMatchOwned
{
    /// <summary>匹配 id。</summary>
    public int Id { get; }

    /// <summary>匹配的模式索引。</summary>
    public ushort PatternIndex { get; }

    /// <summary>捕获结果数组（独立拷贝，可安全跨迭代使用）。</summary>
    public QueryCapture[] Captures { get; }

    internal QueryMatchOwned(int id, ushort patternIndex, QueryCapture[] captures)
    {
        Id = id;
        PatternIndex = patternIndex;
        Captures = captures;
    }
}
