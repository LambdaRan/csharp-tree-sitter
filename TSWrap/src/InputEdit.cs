namespace TreeSitter.Wrap;

using TreeSitter.Bind;

/// <summary>
/// 描述对源代码的增量编辑操作，用于通知 tree-sitter 文本变更。
/// </summary>
public readonly struct InputEdit
{
    /// <summary>编辑起始字节偏移。</summary>
    public uint StartByte { get; }

    /// <summary>编辑前旧文本的结束字节偏移。</summary>
    public uint OldEndByte { get; }

    /// <summary>编辑后新文本的结束字节偏移。</summary>
    public uint NewEndByte { get; }

    /// <summary>编辑起始位置（行、列）。</summary>
    public Point StartPoint { get; }

    /// <summary>编辑前旧文本的结束位置。</summary>
    public Point OldEndPoint { get; }

    /// <summary>编辑后新文本的结束位置。</summary>
    public Point NewEndPoint { get; }

    public InputEdit(
        uint startByte, uint oldEndByte, uint newEndByte,
        Point startPoint, Point oldEndPoint, Point newEndPoint)
    {
        StartByte = startByte;
        OldEndByte = oldEndByte;
        NewEndByte = newEndByte;
        StartPoint = startPoint;
        OldEndPoint = oldEndPoint;
        NewEndPoint = newEndPoint;
    }

    internal TSInputEdit ToNative() => new()
    {
        start_byte = StartByte,
        old_end_byte = OldEndByte,
        new_end_byte = NewEndByte,
        start_point = StartPoint.ToNative(),
        old_end_point = OldEndPoint.ToNative(),
        new_end_point = NewEndPoint.ToNative()
    };
}
