namespace TreeSitter.Wrap;

using TreeSitter.Bind;

/// <summary>
/// 表示语法树中的位置（行、列坐标）。
/// </summary>
public readonly struct Point : IEquatable<Point>
{
    /// <summary>从零计数的行号（表示给定位置之前的换行符数量）。</summary>
    public uint Row { get; }

    /// <summary>从零计数的列号（表示该位置与行首之间的字节数）。</summary>
    public uint Column { get; }

    public Point(uint row, uint column)
    {
        Row = row;
        Column = column;
    }

    internal Point(TSPoint native)
    {
        Row = native.row;
        Column = native.column;
    }

    internal TSPoint ToNative() => new() { row = Row, column = Column };

    public bool Equals(Point other) => Row == other.Row && Column == other.Column;
    public override bool Equals(object? obj) => obj is Point other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Row, Column);

    public static bool operator ==(Point left, Point right) => left.Equals(right);
    public static bool operator !=(Point left, Point right) => !left.Equals(right);

    public override string ToString() => $"({Row}, {Column})";
}
