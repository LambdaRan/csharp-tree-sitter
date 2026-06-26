namespace TreeSitter.Wrap;

using System.Runtime.InteropServices;
using System.Text;
using TreeSitter.Bind;

/// <summary>
/// 封装 tree-sitter 的 TSQuery，提供对查询模式和捕获信息的访问。
/// 构造时将 source string 编码为 UTF-8 传给 ts_query_new；
/// 失败时将字节偏移转换为字符偏移后抛 QueryException。
/// </summary>
public sealed class Query : IDisposable
{
    private readonly QueryHandle _handle;

    /// <summary>
    /// 从语言编译查询。source 中的 S 表达式模式无效时抛出 QueryException。
    /// </summary>
    public Query(Language language, string source)
    {
        var utf8 = Encoding.UTF8.GetBytes(source);
        var ptr = TSApi.ts_query_new(
            language.NativeHandle, utf8, (uint)utf8.Length,
            out uint errorOffset, out TSQueryError errorType);

        if (ptr == IntPtr.Zero)
        {
            // 将 UTF-8 字节偏移转换为 C# 字符串字符偏移
            var charOffset = Encoding.UTF8.GetCharCount(utf8, 0, (int)errorOffset);
            throw new QueryException(errorType, charOffset,
                $"Query compilation failed: {errorType} at offset {charOffset}.");
        }

        _handle = new QueryHandle(ptr);
    }

    /// <summary>内部访问原生指针。</summary>
    internal IntPtr NativeHandle => _handle.DangerousGetHandle();

    /// <summary>查询中的模式数量。</summary>
    public uint PatternCount => TSApi.ts_query_pattern_count(NativeHandle);

    /// <summary>查询中的捕获数量。</summary>
    public uint CaptureCount => TSApi.ts_query_capture_count(NativeHandle);

    /// <summary>查询中的字符串字面量数量。</summary>
    public uint StringCount => TSApi.ts_query_string_count(NativeHandle);

    /// <summary>获取指定模式在查询源文本中的起始字节偏移。</summary>
    public uint StartByteForPattern(uint index) =>
        TSApi.ts_query_start_byte_for_pattern(NativeHandle, index);

    /// <summary>获取指定模式在查询源文本中的结束字节偏移。</summary>
    public uint EndByteForPattern(uint index) =>
        TSApi.ts_query_end_byte_for_pattern(NativeHandle, index);

    /// <summary>检查指定模式是否有唯一的根节点。</summary>
    public bool IsPatternRooted(uint index) =>
        TSApi.ts_query_is_pattern_rooted(NativeHandle, index);

    /// <summary>检查指定模式是否为"非局部"的。</summary>
    public bool IsPatternNonLocal(uint index) =>
        TSApi.ts_query_is_pattern_non_local(NativeHandle, index);

    /// <summary>检查指定模式是否在达到给定字节偏移后保证匹配。</summary>
    public bool IsPatternGuaranteedAtStep(uint byteOffset) =>
        TSApi.ts_query_is_pattern_guaranteed_at_step(NativeHandle, byteOffset);

    /// <summary>获取指定索引处捕获的名称。</summary>
    public string CaptureNameForId(uint id)
    {
        var ptr = TSApi.ts_query_capture_name_for_id(NativeHandle, id, out uint length);
        if (ptr == IntPtr.Zero || length == 0)
            return string.Empty;
        return Marshal.PtrToStringUTF8(ptr, (int)length) ?? string.Empty;
    }

    /// <summary>获取指定索引处字符串字面量的值。</summary>
    public string StringValueForId(uint id)
    {
        var ptr = TSApi.ts_query_string_value_for_id(NativeHandle, id, out uint length);
        if (ptr == IntPtr.Zero || length == 0)
            return string.Empty;
        return Marshal.PtrToStringUTF8(ptr, (int)length) ?? string.Empty;
    }

    /// <summary>获取指定模式的所有谓词步骤。</summary>
    public QueryPredicateStep[] PredicatesForPattern(uint patternIndex)
    {
        var ptr = TSApi.ts_query_predicates_for_pattern(NativeHandle, patternIndex, out uint stepCount);
        if (ptr == IntPtr.Zero || stepCount == 0)
            return Array.Empty<QueryPredicateStep>();

        var result = new QueryPredicateStep[stepCount];
        var nativeSize = Marshal.SizeOf<TSQueryPredicateStep>();
        for (uint i = 0; i < stepCount; i++)
        {
            var native = Marshal.PtrToStructure<TSQueryPredicateStep>(ptr + (int)(i * nativeSize));
            result[i] = new QueryPredicateStep(native);
        }
        return result;
    }

    /// <summary>禁用查询中的指定捕获。禁用后该捕获不会在匹配结果中返回，无法撤销。</summary>
    public void DisableCapture(string name)
    {
        var bytes = Encoding.UTF8.GetBytes(name);
        TSApi.ts_query_disable_capture(NativeHandle, bytes, (uint)bytes.Length);
    }

    /// <summary>禁用查询中的指定模式。禁用后该模式不会匹配，无法撤销。</summary>
    public void DisablePattern(uint index) =>
        TSApi.ts_query_disable_pattern(NativeHandle, index);

    /// <summary>释放查询及其使用的所有内存。</summary>
    public void Dispose() => _handle.Dispose();
}
