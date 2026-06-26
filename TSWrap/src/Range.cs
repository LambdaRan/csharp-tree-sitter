namespace TreeSitter.Wrap;

using TreeSitter.Bind;

/// <summary>
/// 表示语法树中的一个范围（起止位置和字节偏移）。
/// </summary>
public readonly struct Range : IEquatable<Range>
{
    /// <summary>范围的起始位置。</summary>
    public Point StartPoint { get; }

    /// <summary>范围的结束位置。</summary>
    public Point EndPoint { get; }

    /// <summary>起始字节偏移。</summary>
    public uint StartByte { get; }

    /// <summary>结束字节偏移。</summary>
    public uint EndByte { get; }

    public Range(Point startPoint, Point endPoint, uint startByte, uint endByte)
    {
        StartPoint = startPoint;
        EndPoint = endPoint;
        StartByte = startByte;
        EndByte = endByte;
    }

    internal Range(TSRange native)
    {
        StartPoint = new Point(native.start_point);
        EndPoint = new Point(native.end_point);
        StartByte = native.start_byte;
        EndByte = native.end_byte;
    }

    internal TSRange ToNative() => new()
    {
        start_point = StartPoint.ToNative(),
        end_point = EndPoint.ToNative(),
        start_byte = StartByte,
        end_byte = EndByte
    };

    public bool Equals(Range other) =>
        StartPoint == other.StartPoint &&
        EndPoint == other.EndPoint &&
        StartByte == other.StartByte &&
        EndByte == other.EndByte;

    public override bool Equals(object? obj) => obj is Range other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(StartPoint, EndPoint, StartByte, EndByte);

    public static bool operator ==(Range left, Range right) => left.Equals(right);
    public static bool operator !=(Range left, Range right) => !left.Equals(right);

    public override string ToString() => $"[{StartPoint}-{EndPoint} ({StartByte}-{EndByte})]";
}
