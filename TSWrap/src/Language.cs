namespace TreeSitter.Wrap;

using System.Runtime.InteropServices;
using System.Text;
using TreeSitter.Bind;

/// <summary>
/// 封装 tree-sitter 的 TSLanguage，提供对语言元数据和符号信息的类型安全访问。
/// <para>
/// <b>生命周期约束</b>：通过 <see cref="Language(IntPtr)"/> 构造或从 Parser/Tree/Node 获取的
/// 借用型 Language，其底层指针指向来源对象内部数据。必须在来源对象存活期间使用。
/// Dispose 借用型 Language 安全（<c>ownsHandle=false</c>），但访问属性需来源仍然有效。
/// </para>
/// </summary>
public sealed class Language : IDisposable
{
    private readonly LanguageHandle _handle;

    /// <summary>
    /// 从外部获取的语言指针创建 Language（借用型，不拥有所有权）。
    /// 典型场景：从 grammar DLL 的 <c>tree_sitter_xxx()</c> 函数获取的 IntPtr。
    /// </summary>
    public Language(IntPtr languagePtr)
    {
        _handle = new LanguageHandle(languagePtr, ownsHandle: false);
    }

    /// <summary>内部构造函数，带 ownsHandle 标志。</summary>
    internal Language(IntPtr languagePtr, bool ownsHandle)
    {
        _handle = new LanguageHandle(languagePtr, ownsHandle);
    }

    /// <summary>内部访问原生指针。</summary>
    internal IntPtr NativeHandle => _handle.DangerousGetHandle();

    /// <summary>获取语言名称。在较旧的解析器中可能返回 null。</summary>
    public string? Name
    {
        get
        {
            var ptr = TSApi.ts_language_name(NativeHandle);
            return ptr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(ptr);
        }
    }

    /// <summary>获取语言中不同节点类型的数量。</summary>
    public uint SymbolCount => TSApi.ts_language_symbol_count(NativeHandle);

    /// <summary>获取语言中有效状态的数量。</summary>
    public uint StateCount => TSApi.ts_language_state_count(NativeHandle);

    /// <summary>获取语言中不同字段名的数量。</summary>
    public uint FieldCount => TSApi.ts_language_field_count(NativeHandle);

    /// <summary>获取语言的 ABI 版本号。</summary>
    public uint AbiVersion => TSApi.ts_language_abi_version(NativeHandle);

    /// <summary>根据数值 id 获取节点类型名称。返回 null 表示无效 id。</summary>
    public string? SymbolName(ushort symbol)
    {
        var ptr = TSApi.ts_language_symbol_name(NativeHandle, symbol);
        return ptr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(ptr);
    }

    /// <summary>检查给定节点类型 id 属于命名节点、匿名节点还是其他类型。</summary>
    public TSSymbolType SymbolType(ushort symbol) =>
        TSApi.ts_language_symbol_type(NativeHandle, symbol);

    /// <summary>根据节点类型名称获取其数值 id（TSSymbol）。</summary>
    public ushort SymbolForName(string name, bool isNamed)
    {
        var bytes = Encoding.UTF8.GetBytes(name);
        return TSApi.ts_language_symbol_for_name(NativeHandle, bytes, (uint)bytes.Length, isNamed);
    }

    /// <summary>根据数值 id 获取字段名。返回 null 表示无效 id。</summary>
    public string? FieldNameForId(ushort id)
    {
        var ptr = TSApi.ts_language_field_name_for_id(NativeHandle, id);
        return ptr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(ptr);
    }

    /// <summary>根据字段名获取其数值 id（TSFieldId）。</summary>
    public ushort FieldIdForName(string name)
    {
        var bytes = Encoding.UTF8.GetBytes(name);
        return TSApi.ts_language_field_id_for_name(NativeHandle, bytes, (uint)bytes.Length);
    }

    /// <summary>
    /// 创建拥有型副本（增加引用计数）。返回的 Language 拥有独立的资源，
    /// 调用其 Dispose 会释放引用。
    /// </summary>
    public Language Copy()
    {
        var copied = TSApi.ts_language_copy(NativeHandle);
        return new Language(copied, ownsHandle: true);
    }

    /// <summary>释放语言资源。对于借用型 Language 此操作安全（不释放底层指针）。</summary>
    public void Dispose() => _handle.Dispose();
}
