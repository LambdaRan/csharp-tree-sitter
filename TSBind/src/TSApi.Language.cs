namespace TreeSitter.Native;

using System.Runtime.InteropServices;

public static partial class TSApi
{
    // =====================
    // 引用计数
    // =====================

    // ts_language_copy → IntPtr
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_language_copy(IntPtr self);

    // ts_language_delete → void
    [LibraryImport(LibraryName)]
    public static partial void ts_language_delete(IntPtr self);

    // =====================
    // 符号信息
    // =====================

    // ts_language_symbol_count → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_language_symbol_count(IntPtr self);

    // ts_language_state_count → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_language_state_count(IntPtr self);

    // ts_language_symbol_for_name → ushort (TSSymbol), 参数含 string + uint length + bool is_named
    [LibraryImport(LibraryName)]
    public static partial ushort ts_language_symbol_for_name(
        IntPtr self, [MarshalAs(UnmanagedType.LPUTF8Str)] string str, uint length,
        [MarshalAs(UnmanagedType.U1)] bool is_named);

    // ts_language_symbol_name → IntPtr (const char*，内部拥有)
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_language_symbol_name(IntPtr self, ushort symbol);

    // ts_language_symbol_type → TSSymbolType
    [LibraryImport(LibraryName)]
    public static partial TSSymbolType ts_language_symbol_type(IntPtr self, ushort symbol);

    // =====================
    // 字段信息
    // =====================

    // ts_language_field_count → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_language_field_count(IntPtr self);

    // ts_language_field_name_for_id → IntPtr (const char*，内部拥有)
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_language_field_name_for_id(IntPtr self, ushort id);

    // ts_language_field_id_for_name → ushort (TSFieldId), 参数含 string + uint length
    [LibraryImport(LibraryName)]
    public static partial ushort ts_language_field_id_for_name(
        IntPtr self, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, uint name_length);

    // =====================
    // 超类型/子类型
    // =====================

    // ts_language_supertypes → IntPtr (const TSSymbol*，库内部拥有，不需释放), 参数 out uint length
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_language_supertypes(IntPtr self, out uint length);

    // ts_language_subtypes → IntPtr (const TSSymbol*，库内部拥有，不需释放), 参数 ushort supertype + out uint length
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_language_subtypes(IntPtr self, ushort supertype, out uint length);

    // =====================
    // 版本/元数据
    // =====================

    // ts_language_abi_version → uint
    [LibraryImport(LibraryName)]
    public static partial uint ts_language_abi_version(IntPtr self);

    // ts_language_metadata → IntPtr (const TSLanguageMetadata*，库内部拥有，不需释放)
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_language_metadata(IntPtr self);

    // ts_language_next_state → ushort (TSStateId), 参数 ushort state + ushort symbol
    [LibraryImport(LibraryName)]
    public static partial ushort ts_language_next_state(IntPtr self, ushort state, ushort symbol);

    // ts_language_name → IntPtr (const char*，内部拥有，可能为 NULL)
    [LibraryImport(LibraryName)]
    public static partial IntPtr ts_language_name(IntPtr self);
}
