namespace TreeSitter.Native;

using System.Runtime.CompilerServices;      // InlineArray
using System.Runtime.InteropServices;       // StructLayout, MarshalAs

// api.h 中的 typedef 别名，在 C# 中直接使用对应基础类型：
//   TSStateId  = uint16_t  → ushort
//   TSSymbol   = uint16_t  → ushort
//   TSFieldId  = uint16_t  → ushort

// =====================
// Enums
// =====================

public enum TSInputEncoding : int {
    TSInputEncodingUTF8,
    TSInputEncodingUTF16LE,
    TSInputEncodingUTF16BE,
    TSInputEncodingCustom
}

public enum TSSymbolType : int {
    TSSymbolTypeRegular,
    TSSymbolTypeAnonymous,
    TSSymbolTypeSupertype,
    TSSymbolTypeAuxiliary,
}

public enum TSLogType : int {
    TSLogTypeParse,
    TSLogTypeLex,
}

public enum TSQuantifier : int {
    TSQuantifierZero = 0,
    TSQuantifierZeroOrOne,
    TSQuantifierZeroOrMore,
    TSQuantifierOne,
    TSQuantifierOneOrMore,
}

public enum TSQueryPredicateStepType : int {
    TSQueryPredicateStepTypeDone,
    TSQueryPredicateStepTypeCapture,
    TSQueryPredicateStepTypeString,
}

public enum TSQueryError : int {
    TSQueryErrorNone = 0,
    TSQueryErrorSyntax,
    TSQueryErrorNodeType,
    TSQueryErrorField,
    TSQueryErrorCapture,
    TSQueryErrorStructure,
    TSQueryErrorLanguage,
}

// =====================
// InlineArray 辅助类型
// =====================

[InlineArray(4)]
public struct InlineArray4_UInt32 { private uint _element0; }

[InlineArray(3)]
public struct InlineArray3_UInt32 { private uint _element0; }

// =====================
// Structs
// =====================

[StructLayout(LayoutKind.Sequential)]
public struct TSPoint {
	// 表示给定位置之前的换行符数量
	public uint row;
	// 表示该位置与行首之间的字节数
	public uint column;
}

[StructLayout(LayoutKind.Sequential)]
public struct TSRange {
    public TSPoint start_point;
    public TSPoint end_point;
    public uint start_byte;
    public uint end_byte;
}

[StructLayout(LayoutKind.Sequential)]
public struct TSInputEdit {
    public uint start_byte;
    public uint old_end_byte;
    public uint new_end_byte;
    public TSPoint start_point;
    public TSPoint old_end_point;
    public TSPoint new_end_point;
}

[StructLayout(LayoutKind.Sequential)]
public struct TSNode {
    public InlineArray4_UInt32 context;
    public IntPtr id; // 原 const void*
    public IntPtr tree; // 原 const TSTree*
}

[StructLayout(LayoutKind.Sequential)]
public struct TSTreeCursor {
    public IntPtr tree; // 原 const void*
    public IntPtr id; // 原 const void*（注意：实际是内部 stack.contents 指针，不代表当前节点）
    public InlineArray3_UInt32 context;
    // context[0] = stack.size（栈深度，1=root，2=子节点…）
    // context[1] = stack.capacity
    // context[2] = root_alias_symbol
}

[StructLayout(LayoutKind.Sequential)]
public struct TSQueryCapture {
    public TSNode node;
    public uint index;
}

[StructLayout(LayoutKind.Sequential)]
public struct TSQueryMatch {
    public uint id;
    public ushort pattern_index;
    public ushort capture_count;
    public IntPtr captures; // 原 const TSQueryCapture*
}

[StructLayout(LayoutKind.Sequential)]
public struct TSQueryPredicateStep {
    public TSQueryPredicateStepType type;
    public uint value_id;
}

[StructLayout(LayoutKind.Sequential)]
public struct TSInput {
    public IntPtr payload;
    public IntPtr read;       // 函数指针
    public TSInputEncoding encoding;
    public IntPtr decode;     // 函数指针
}

[StructLayout(LayoutKind.Sequential)]
public struct TSLogger {
    public IntPtr payload;
    public IntPtr log;        // 函数指针
}

[StructLayout(LayoutKind.Sequential)]
public struct TSParseState {
    public IntPtr payload;
    public uint current_byte_offset;
    [MarshalAs(UnmanagedType.U1)]
    public bool has_error;
}

[StructLayout(LayoutKind.Sequential)]
public struct TSParseOptions {
    public IntPtr payload;
    public IntPtr progress_callback;  // 函数指针
}

[StructLayout(LayoutKind.Sequential)]
public struct TSQueryCursorState {
    public IntPtr payload;
    public uint current_byte_offset;
}

[StructLayout(LayoutKind.Sequential)]
public struct TSQueryCursorOptions {
    public IntPtr payload;
    public IntPtr progress_callback;  // 函数指针
}

[StructLayout(LayoutKind.Sequential)]
public struct TSLanguageMetadata {
    public byte major_version;
    public byte minor_version;
    public byte patch_version;
}
