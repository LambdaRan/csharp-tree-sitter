namespace TreeSitter.Native;

using System.Runtime.InteropServices;

public static partial class TSApi
{
    // =====================
    // 引用计数
    // =====================

    /// <summary>获取给定语言的另一个引用（增加引用计数）。</summary>
    // ts_language_copy → IntPtr
    // 返回值：原 const TSLanguage*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_language_copy(IntPtr self); // 原 const TSLanguage*

    /// <summary>释放语言的动态分配资源（如果是最后一个引用）。</summary>
    // ts_language_delete → void
    [LibraryImport(LibraryName)]
    public static partial void ts_language_delete(IntPtr self); // 原 const TSLanguage*

    // =====================
    // 符号信息
    // =====================

    /// <summary>获取语言中不同节点类型的数量。</summary>
    // ts_language_symbol_count → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_language_symbol_count(IntPtr self); // 原 const TSLanguage*

    /// <summary>获取语言中有效状态的数量。</summary>
    // ts_language_state_count → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_language_state_count(IntPtr self); // 原 const TSLanguage*

    /// <summary>根据节点类型字符串获取其数值 id（TSSymbol）。</summary>
    // ts_language_symbol_for_name → ushort (TSSymbol)
    [LibraryImport(LibraryName)]
    public static partial ushort ts_language_symbol_for_name(
        IntPtr self, // 原 const TSLanguage*
        ReadOnlySpan<byte> str,
        uint length,
        [MarshalAs(UnmanagedType.U1)] bool is_named);

    /// <summary>根据数值 id 获取节点类型字符串。返回的字符串由库内部拥有。</summary>
    // ts_language_symbol_name → IntPtr (内部拥有)
    // 返回值：原 const char*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_language_symbol_name(IntPtr self, ushort symbol); // 原 const TSLanguage*

    /// <summary>检查给定节点类型 id 属于命名节点、匿名节点还是隐藏节点。</summary>
    // ts_language_symbol_type → TSSymbolType
    [LibraryImport(LibraryName)]
    public static partial TSSymbolType ts_language_symbol_type(IntPtr self, ushort symbol); // 原 const TSLanguage*

    // =====================
    // 字段信息
    // =====================

    /// <summary>获取语言中不同字段名的数量。</summary>
    // ts_language_field_count → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_language_field_count(IntPtr self); // 原 const TSLanguage*

    /// <summary>根据数值 id 获取字段名字符串。返回的字符串由库内部拥有。</summary>
    // ts_language_field_name_for_id → IntPtr (内部拥有)
    // 返回值：原 const char*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_language_field_name_for_id(IntPtr self, ushort id); // 原 const TSLanguage*

    /// <summary>根据字段名获取其数值 id（TSFieldId）。</summary>
    // ts_language_field_id_for_name → ushort (TSFieldId)
    [LibraryImport(LibraryName)]
    public static partial ushort ts_language_field_id_for_name(
        IntPtr self, // 原 const TSLanguage*
        ReadOnlySpan<byte> name, uint name_length);

    // =====================
    // 超类型/子类型
    // =====================

    /// <summary>获取语言的所有超类型符号列表。返回的指针由库内部拥有，不需释放。</summary>
    // ts_language_supertypes → IntPtr (库内部拥有，不需释放)
    // 返回值：原 const TSSymbol*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_language_supertypes(IntPtr self, out uint length); // 原 const TSLanguage*

    /// <summary>获取指定超类型符号的所有子类型符号 id 列表。返回的指针由库内部拥有，不需释放。</summary>
    // ts_language_subtypes → IntPtr (库内部拥有，不需释放)
    // 返回值：原 const TSSymbol*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_language_subtypes(
        IntPtr self, // 原 const TSLanguage*
        ushort supertype, out uint length);

    // =====================
    // 版本/元数据
    // =====================

    /// <summary>获取语言的 ABI 版本号，用于确保语言由兼容版本的 Tree-sitter CLI 生成。</summary>
    // ts_language_abi_version → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_language_abi_version(IntPtr self); // 原 const TSLanguage*

    /// <summary>获取语言的元数据（语义版本信息）。此信息由 CLI 生成，依赖语言作者在 tree-sitter.json 中提供正确的元数据。</summary>
    // ts_language_metadata → IntPtr (库内部拥有，不需释放)
    // 返回值：原 const TSLanguageMetadata*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_language_metadata(IntPtr self); // 原 const TSLanguage*

    /// <summary>获取下一个解析状态。与前瞻迭代器结合可生成补全建议或错误节点中的有效符号。</summary>
    // ts_language_next_state → ushort (TSStateId)
    [LibraryImport(LibraryName)]
    public static partial ushort ts_language_next_state(IntPtr self, ushort state, ushort symbol); // 原 const TSLanguage*

    /// <summary>获取语言名称。在较旧的解析器中可能返回 NULL。</summary>
    // ts_language_name → IntPtr (内部拥有，可能为 NULL)
    // 返回值：原 const char*
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_language_name(IntPtr self); // 原 const TSLanguage*
}
