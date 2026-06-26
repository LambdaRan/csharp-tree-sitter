namespace TreeSitter.Wrap;

using TreeSitter.Bind;

/// <summary>
/// tree-sitter 操作的一般异常基类。
/// </summary>
public class TreeSitterException : Exception
{
    public TreeSitterException(string message) : base(message) { }
}

/// <summary>
/// 查询编译失败时抛出的异常，包含错误类型和偏移信息。
/// </summary>
public class QueryException : TreeSitterException
{
    /// <summary>查询错误的类型。</summary>
    public TSQueryError Error { get; }

    /// <summary>查询字符串中的错误偏移（字符偏移，非 UTF-8 字节偏移）。</summary>
    public int ErrorOffset { get; }

    public QueryException(TSQueryError error, int errorOffset, string message)
        : base(message)
    {
        Error = error;
        ErrorOffset = errorOffset;
    }
}
