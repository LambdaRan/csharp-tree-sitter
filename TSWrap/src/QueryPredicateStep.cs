namespace TreeSitter.Wrap;

using TreeSitter.Bind;

/// <summary>
/// 表示查询谓词中的一个步骤。
/// type 为 Capture 表示捕获名、String 表示字面量字符串、Done 表示谓词结束哨兵。
/// </summary>
public readonly struct QueryPredicateStep
{
    /// <summary>谓词步骤类型。</summary>
    public TSQueryPredicateStepType Type { get; }

    /// <summary>
    /// 值 id。对于 Capture 类型，用 <see cref="Query.CaptureNameForId"/> 获取名称；
    /// 对于 String 类型，用 <see cref="Query.StringValueForId"/> 获取值。
    /// </summary>
    public uint ValueId { get; }

    internal QueryPredicateStep(TSQueryPredicateStep native)
    {
        Type = native.type;
        ValueId = native.value_id;
    }

    public QueryPredicateStep(TSQueryPredicateStepType type, uint valueId)
    {
        Type = type;
        ValueId = valueId;
    }
}
