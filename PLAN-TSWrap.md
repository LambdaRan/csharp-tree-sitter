# TSWrap 实施计划

## 概述

在 `TSWrap/` 目录创建 C# 封装库项目，采用 SafeHandle + class/struct 模式包装 TSBind 的原生绑定，提供类型安全、生命周期可控的 C# 友好 API。

---

## 项目结构

```
csharp-tree-sitter/
├── csharp-tree-sitter.slnx
├── TSBind/                       ← 已有：原生绑定库
│   ├── TSBind.csproj
│   └── src/
└── TSWrap/                       ← 新建：封装库
    ├── TSWrap.csproj
    └── src/
        ├── Language.cs           ← TSLanguage 封装
        ├── Parser.cs             ← TSParser 封装
        ├── Tree.cs               ← TSTree 封装
        ├── Node.cs               ← TSNode 封装（readonly struct）
        ├── Query.cs              ← TSQuery 封装
        ├── QueryCursor.cs        ← TSQueryCursor 封装
        ├── TreeCursor.cs         ← TSTreeCursor 封装（class + IDisposable）
        ├── Point.cs              ← 位置类型
        ├── Range.cs              ← 范围类型
        ├── InputEdit.cs          ← 增量编辑描述
        ├── QueryMatch.cs         ← 查询匹配结果（ref struct + 普通 struct）
        ├── QueryCapture.cs       ← 查询捕获结果
        ├── QueryPredicateStep.cs ← 查询谓词步骤
        ├── Handles.cs            ← 所有 SafeHandle 子类
        └── Exceptions.cs         ← TreeSitterException, QueryException
```

---

## 类型封装策略

### SafeHandle 子类（Handles.cs）

所有 SafeHandle 内部类定义在同一个文件中，不对外暴露：

| SafeHandle | InvalidValue | ReleaseHandle 调用 |
|------------|-------------|-------------------|
| `ParserHandle` | Zero | `ts_parser_delete(handle)` |
| `TreeHandle` | Zero | `ts_tree_delete(handle)` |
| `LanguageHandle` | Zero | `ts_language_delete(handle)`（仅 `ownsHandle=true` 时） |
| `QueryHandle` | Zero | `ts_query_delete(handle)` |
| `QueryCursorHandle` | Zero | `ts_query_cursor_delete(handle)` |

`LanguageHandle` 特殊：构造时接受 `ownsHandle` 参数。外部加载（grammar DLL）和借用引用（从 parser/tree 获取）传 `ownsHandle: false`，`ts_language_copy()` 产生的传 `ownsHandle: true`。

### 值类型（struct）

| 类型 | 说明 |
|------|------|
| `Point` | `readonly struct`，字段：`Row`（uint）、`Column`（uint），含 `ToString()`、`==`/`!=` |
| `Range` | `readonly struct`，字段：`StartPoint`、`EndPoint`、`StartByte`、`EndByte`，含 `ToString()` |
| `InputEdit` | `readonly struct`，字段：`StartByte`、`OldEndByte`、`NewEndByte`、`StartPoint`、`OldEndPoint`、`NewEndPoint`，内部转换为 `TSInputEdit` |
| `Node` | `readonly struct`，内部持有原生 `TSNode` + `Tree` owner 引用，实现 `IEquatable<Node>` |
| `QueryMatch` | `ref struct`（NextMatch 路径），字段：`Id`（int）、`PatternIndex`（ushort）、`Captures`（`ReadOnlySpan<QueryCapture>`）；零分配，仅在下次 NextMatch 前有效 |
| `QueryMatchOwned` | 普通 `readonly struct`（IEnumerable 路径），字段同上但 `Captures` 为 `QueryCapture[]`；可跨迭代安全使用 |
| `QueryCapture` | `readonly struct`，字段：`Node`、`Name`（string） |
| `QueryPredicateStep` | `readonly struct`，字段：`Type`（`TSQueryPredicateStepType`）、`ValueId`（uint） |

---

## 各类型 API 设计

### Language（class，SafeHandle 包装）

```csharp
namespace TreeSitter.Wrap;

public sealed class Language : IDisposable
{
    // 构造：接受 IntPtr（从 grammar DLL 获取），ownsHandle=false
    public Language(IntPtr languagePtr);

    // 内部构造：带 ownsHandle 标志
    internal Language(IntPtr languagePtr, bool ownsHandle);

    // 属性
    public string? Name { get; }                    // ts_language_name
    public uint SymbolCount { get; }                // ts_language_symbol_count
    public uint StateCount { get; }                 // ts_language_state_count
    public uint FieldCount { get; }                 // ts_language_field_count
    public uint AbiVersion { get; }                 // ts_language_abi_version

    // 符号查询
    public string? SymbolName(ushort symbol);       // ts_language_symbol_name
    public TSSymbolType SymbolType(ushort symbol);  // ts_language_symbol_type
    public ushort SymbolForName(string name, bool isNamed); // ts_language_symbol_for_name

    // 字段查询
    public string? FieldNameForId(ushort id);       // ts_language_field_name_for_id
    public ushort FieldIdForName(string name);      // ts_language_field_id_for_name

    // 复制（产生拥有型引用）
    public Language Copy();                         // ts_language_copy → ownsHandle=true

    public void Dispose();
}
```

> **生命周期约束**：通过 `Language(IntPtr)` 或从 Parser/Tree/Node 获取的借用型 Language，其底层指针指向来源对象（Parser/Tree）内部数据。借用型 Language 必须在其来源对象存活期间使用，否则底层指针变为悬空。Dispose 借用型 Language 是安全的（`ownsHandle=false` 不调 `ts_language_delete`），但访问其属性需要来源仍然有效。

### Parser（class，SafeHandle 包装）

```csharp
public sealed class Parser : IDisposable
{
    // 构造
    public Parser();
    public Parser(Language language);               // 构造时设置语言，失败抛异常

    // 设置语言
    public void SetLanguage(Language language);     // 失败抛 TreeSitterException

    // 解析
    public Tree Parse(string source, Tree? oldTree = null);
    public Tree Parse(ReadOnlySpan<byte> utf8, Tree? oldTree = null);

    // TryParse 模式
    public bool TryParse(string source, out Tree? tree, Tree? oldTree = null);
    public bool TryParse(ReadOnlySpan<byte> utf8, out Tree? tree, Tree? oldTree = null);

    // 重置
    public void Reset();                            // ts_parser_reset

    // 包含范围（用于限定解析范围）
    // get: 调用 ts_parser_included_ranges（借用指针），拷贝为 Wrap Range[] 返回
    // set: 将 Wrap Range[] 转换为 TSRange[]，调用 ts_parser_set_included_ranges
    public Range[] IncludedRanges { get; set; }

    public void Dispose();
}
```

### Tree（class，SafeHandle 包装）

```csharp
public sealed class Tree : IDisposable
{
    // 根节点（计算属性，每次访问调用 ts_tree_root_node，不缓存）
    // Tree.Edit() 后自动返回更新后的根节点
    public Node RootNode { get; }

    // 带偏移的根节点
    public Node RootNodeWithOffset(uint offsetBytes, Point offsetExtent);

    // 增量编辑（接受 Wrap 层的 InputEdit，内部转换为 TSInputEdit）
    public void Edit(InputEdit edit);               // ts_tree_edit

    // 复制
    public Tree Copy();                             // ts_tree_copy

    // 获取语言（借用引用，ownsHandle=false）
    public Language Language { get; }               // ts_tree_language

    // 差异范围（内部拷贝 TSRange[] 为 Wrap Range[] 后立即调 ts_tree_get_changed_ranges_free）
    public Range[] GetChangedRanges(Tree oldTree);

    // 包含范围（内部拷贝后调 ts_tree_included_ranges_free）
    public Range[] IncludedRanges { get; }

    public void Dispose();
}
```

### Node（readonly struct，IEquatable<Node>）

```csharp
public readonly struct Node : IEquatable<Node>
{
    // ---- 内部 ----
    // 持有原生 TSNode _rawNode + Tree? _owner（防止 Tree 被 GC 回收）

    // ---- 类型/符号信息 ----
    public string Type { get; }                     // ts_node_type → PtrToStringUTF8
    public string GrammarType { get; }              // ts_node_grammar_type
    public ushort Symbol { get; }                   // ts_node_symbol
    public ushort GrammarSymbol { get; }            // ts_node_grammar_symbol
    public Language Language { get; }               // ts_node_language（借用，文档约束生命周期）

    // ---- 位置信息 ----
    public int StartByte { get; }                   // ts_node_start_byte
    public int EndByte { get; }                     // ts_node_end_byte
    public Point StartPoint { get; }                // ts_node_start_point
    public Point EndPoint { get; }                  // ts_node_end_point

    // ---- 状态检查 ----
    public bool IsNull { get; }                     // ts_node_is_null
    public bool IsNamed { get; }                    // ts_node_is_named
    public bool IsMissing { get; }                  // ts_node_is_missing
    public bool IsExtra { get; }                    // ts_node_is_extra
    public bool HasChanges { get; }                 // ts_node_has_changes
    public bool HasError { get; }                   // ts_node_has_error
    public bool IsError { get; }                    // ts_node_is_error

    // ---- 解析状态 ----
    public ushort ParseState { get; }               // ts_node_parse_state
    public ushort NextParseState { get; }           // ts_node_next_parse_state

    // ---- 计数 ----
    public uint ChildCount { get; }                 // ts_node_child_count
    public uint NamedChildCount { get; }            // ts_node_named_child_count
    public uint DescendantCount { get; }            // ts_node_descendant_count

    // ---- 导航（返回新 Node，继承 _owner 引用）----
    public Node Parent { get; }                     // ts_node_parent
    public Node Child(uint index);                  // ts_node_child
    public Node NamedChild(uint index);             // ts_node_named_child
    public Node NextSibling { get; }                // ts_node_next_sibling
    public Node PrevSibling { get; }                // ts_node_prev_sibling
    public Node NextNamedSibling { get; }           // ts_node_next_named_sibling
    public Node PrevNamedSibling { get; }           // ts_node_prev_named_sibling
    public Node ChildWithDescendant(Node descendant); // ts_node_child_with_descendant
    public Node ChildByFieldName(string name);      // ts_node_child_by_field_name
    public Node ChildByFieldId(ushort fieldId);     // ts_node_child_by_field_id
    public Node FirstChildForByte(uint byteOffset); // ts_node_first_child_for_byte
    public Node FirstNamedChildForByte(uint byteOffset); // ts_node_first_named_child_for_byte
    public Node DescendantForByteRange(uint start, uint end);    // ts_node_descendant_for_byte_range
    public Node DescendantForPointRange(Point start, Point end); // ts_node_descendant_for_point_range
    public Node NamedDescendantForByteRange(uint start, uint end);
    public Node NamedDescendantForPointRange(Point start, Point end);

    // ---- 字段名 ----
    public string? FieldNameForChild(uint index);          // ts_node_field_name_for_child
    public string? FieldNameForNamedChild(uint index);     // ts_node_field_name_for_named_child

    // ---- 编辑 ----
    // 实现：拷贝 _rawNode 到局部变量 → ref 传给 ts_node_edit → 用修改后的值构造新 Node 返回
    // 因为 Node 是 readonly struct，不能直接 ref 传内部字段
    public Node Edit(InputEdit edit);

    // ---- 字符串表示 ----
    // 调用 ts_node_string 获取 malloc'd char*，PtrToStringUTF8 后立即 ts_node_string_free
    public override string ToString();

    // ---- 等值比较 ----
    public bool Equals(Node other);                        // ts_node_eq
    public override bool Equals(object? obj);
    public override int GetHashCode();                     // 基于内部 _id 指针
    public static bool operator ==(Node left, Node right);
    public static bool operator !=(Node left, Node right);
}
```

### Query（class，SafeHandle 包装）

```csharp
public sealed class Query : IDisposable
{
    // 构造（失败抛 QueryException）
    // 内部将 source string 编码为 UTF-8 传给 ts_query_new
    // QueryException.ErrorOffset 已从 UTF-8 字节偏移转换为字符偏移
    public Query(Language language, string source);

    // 属性
    public uint PatternCount { get; }               // ts_query_pattern_count
    public uint CaptureCount { get; }               // ts_query_capture_count
    public uint StringCount { get; }                // ts_query_string_count

    // Pattern 信息
    public uint StartByteForPattern(uint index);    // ts_query_start_byte_for_pattern
    public uint EndByteForPattern(uint index);      // ts_query_end_byte_for_pattern
    public bool IsPatternRooted(uint index);        // ts_query_is_pattern_rooted
    public bool IsPatternNonLocal(uint index);      // ts_query_is_pattern_non_local
    public bool IsPatternGuaranteedAtStep(uint index); // ts_query_is_pattern_guaranteed_at_step

    // Capture/String 信息
    public string CaptureNameForId(uint id);        // ts_query_capture_name_for_id
    public string StringValueForId(uint id);        // ts_query_string_value_for_id

    // Predicates（拷贝 TSQueryPredicateStep[] 为 Wrap QueryPredicateStep[] 返回）
    public QueryPredicateStep[] PredicatesForPattern(uint patternIndex); // ts_query_predicates_for_pattern

    // 禁用
    public void DisableCapture(string name);        // ts_query_disable_capture
    public void DisablePattern(uint index);         // ts_query_disable_pattern

    public void Dispose();
}
```

### QueryCursor（class，SafeHandle 包装）

```csharp
public sealed class QueryCursor : IDisposable
{
    // 构造
    public QueryCursor();

    // 执行查询
    public void Exec(Query query, Node node);       // ts_query_cursor_exec

    // 范围限制
    public void SetByteRange(uint start, uint end); // ts_query_cursor_set_byte_range
    public void SetPointRange(Point start, Point end); // ts_query_cursor_set_point_range
    public void SetContainingByteRange(uint start, uint end);
    public void SetContainingPointRange(Point start, Point end);
    public void SetMaxStartDepth(uint depth);       // ts_query_cursor_set_max_start_depth

    // 匹配限制
    public uint MatchLimit { get; set; }            // ts_query_cursor_match_limit / set
    public bool DidExceedMatchLimit { get; }        // ts_query_cursor_did_exceed_match_limit

    // 迭代（底层，零分配路径）
    // QueryMatch 是 ref struct，Captures 为 ReadOnlySpan<QueryCapture>
    // 仅在下次 NextMatch/NextCapture 调用前有效
    public bool NextMatch(out QueryMatch match);    // ts_query_cursor_next_match
    public bool NextCapture(out QueryMatch match, out uint captureIndex); // ts_query_cursor_next_capture
    public void RemoveMatch(uint matchId);          // ts_query_cursor_remove_match

    // 迭代（高层 IEnumerable，有分配路径）
    // 内部每次迭代拷贝 captures 到 QueryCapture[]，返回 QueryMatchOwned
    // 可安全跨迭代使用，支持 LINQ
    public IEnumerable<QueryMatchOwned> Matches { get; }

    public void Dispose();
}
```

### TreeCursor（class + IDisposable + finalizer）

```csharp
public sealed class TreeCursor : IDisposable
{
    // 构造
    public TreeCursor(Node node);                   // ts_tree_cursor_new

    // 当前信息
    public Node CurrentNode { get; }                // ts_tree_cursor_current_node
    public string? CurrentFieldName { get; }        // ts_tree_cursor_current_field_name
    public ushort CurrentFieldId { get; }           // ts_tree_cursor_current_field_id
    public uint CurrentDescendantIndex { get; }     // ts_tree_cursor_current_descendant_index
    public uint CurrentDepth { get; }               // ts_tree_cursor_current_depth

    // 导航
    public bool GotoParent();                       // ts_tree_cursor_goto_parent
    public bool GotoFirstChild();                   // ts_tree_cursor_goto_first_child
    public bool GotoLastChild();                    // ts_tree_cursor_goto_last_child
    public bool GotoNextSibling();                  // ts_tree_cursor_goto_next_sibling
    public bool GotoPreviousSibling();              // ts_tree_cursor_goto_previous_sibling
    public void GotoDescendant(uint index);         // ts_tree_cursor_goto_descendant
    public long GotoFirstChildForByte(uint byteOffset);   // ts_tree_cursor_goto_first_child_for_byte
    public long GotoFirstChildForPoint(Point point);      // ts_tree_cursor_goto_first_child_for_point

    // 重置
    public void Reset(Node node);                   // ts_tree_cursor_reset
    public void ResetTo(TreeCursor other);          // ts_tree_cursor_reset_to

    // 复制
    public TreeCursor Copy();                       // ts_tree_cursor_copy

    public void Dispose();
    ~TreeCursor();                                  // finalizer 兜底
}
```

---

## 异常类型（Exceptions.cs）

```csharp
public class TreeSitterException : Exception
{
    public TreeSitterException(string message) : base(message) { }
}

public class QueryException : TreeSitterException
{
    public TSQueryError Error { get; }       // 错误类型
    public int ErrorOffset { get; }          // 查询字符串中的错误偏移（字符偏移，非字节偏移）

    public QueryException(TSQueryError error, int errorOffset, string message)
        : base(message)
    {
        Error = error;
        ErrorOffset = errorOffset;
    }
}
```

> **ErrorOffset 转换**：`ts_query_new` 返回的 error_offset 是 UTF-8 字节偏移。构造 QueryException 时，内部通过 `Encoding.UTF8.GetCharCount(utf8Bytes, 0, (int)errorOffset)` 转换为 C# 字符串的字符偏移。

---

## 实施步骤

### 步骤 1：创建项目骨架

1. 创建 `TSWrap/TSWrap.csproj`（net10.0 库项目，引用 TSBind）
2. 创建 `TSWrap/src/` 目录
3. 更新 `csharp-tree-sitter.slnx` 添加 TSWrap 项目
4. 确认编译通过

### 步骤 2：基础设施类型

1. 实现 `Handles.cs`（5 个 SafeHandle 子类）
2. 实现 `Point.cs`、`Range.cs`、`InputEdit.cs`（readonly struct）
3. 实现 `Exceptions.cs`（TreeSitterException、QueryException）

### 步骤 3：Language 封装

1. 实现 `Language.cs`
2. 重点：`ownsHandle` 标志控制 SafeHandle 释放行为
3. 借用型 Language 的生命周期约束写入 XML 文档注释

### 步骤 4：Parser 封装

1. 实现 `Parser.cs`
2. `Parse(string)` 内部转 UTF-16LE 调用 `ts_parser_parse_string_encoding`
3. `Parse(ReadOnlySpan<byte>)` 调用 `ts_parser_parse_string`
4. 支持 `oldTree` 增量解析
5. `TryParse` 返回 bool 而非抛异常
6. `IncludedRanges` getter 拷贝借用数组，setter 转换 Wrap Range[] → TSRange[]

### 步骤 5：Tree 封装

1. 实现 `Tree.cs`
2. `RootNode` 为计算属性（不缓存，每次调 `ts_tree_root_node`）
3. `Edit` 接受 Wrap 层 `InputEdit`，内部转换为 `TSInputEdit`
4. `Copy` 返回新 Tree
5. `GetChangedRanges` / `IncludedRanges` 返回 `Range[]`（拷贝后 free）

### 步骤 6：Node 封装

1. 实现 `Node.cs`（readonly struct）
2. 内部持有原生 `TSNode` + `Tree?` owner 引用
3. 所有导航方法返回新 Node（继承 owner 引用）
4. `Edit()` 实现：拷贝 `_rawNode` → `ref` 传给 `ts_node_edit` → 构造新 Node 返回
5. 实现 `IEquatable<Node>`
6. `ToString()` 调 `ts_node_string` + `ts_node_string_free`

### 步骤 7：Query + QueryCursor 封装

1. 实现 `Query.cs`
2. 构造函数中调 `ts_query_new`，失败时将字节偏移转换为字符偏移后抛 `QueryException`
3. 实现 `QueryMatch`（ref struct）和 `QueryMatchOwned`（普通 struct）
4. 实现 `QueryCursor.cs`
5. `NextMatch(out QueryMatch)` — 零分配路径，ref struct + Span
6. `NextCapture(out QueryMatch, out uint captureIndex)` — 返回完整 match + 索引
7. `Matches` — IEnumerable 路径，内部拷贝 captures 为数组
8. 实现 `QueryCapture.cs`、`QueryPredicateStep.cs`
9. `PredicatesForPattern` 返回 `QueryPredicateStep[]`

### 步骤 8：TreeCursor 封装

1. 实现 `TreeCursor.cs`
2. 内部持有 `TSTreeCursor` 值类型字段
3. class + IDisposable + finalizer
4. 所有导航方法返回 bool / void

### 步骤 9：集成测试

1. 在 `TSBind/test/lua/` 添加 TSWrap 项目引用
2. 用 TSWrap API 重写 Program.cs 的测试逻辑
3. 验证完整的 解析 → 遍历 → 查询 流程

---

## 关键设计约束

1. **Tree 必须比 Node 活得久**：Node struct 持有 Tree 引用，GC 自动保证
2. **借用型 Language 的生命周期**：通过 `Language(IntPtr)` 构造或从 Parser/Tree/Node 获取的借用型 Language，其底层指针指向来源对象内部数据。必须在来源对象存活期间使用。Dispose 借用型 Language 安全（`ownsHandle=false`），但访问属性需来源有效
3. **QueryCursor 的 captures 仅在下次迭代前有效**：`NextMatch` 路径的 `QueryMatch`（ref struct）持有 `ReadOnlySpan<QueryCapture>`，span 指向 cursor 内部数据。`Matches`（IEnumerable）路径在内部拷贝为数组，无此限制
4. **malloc'd 数组必须用对应的 free 函数释放**：`ts_tree_included_ranges_free` / `ts_tree_get_changed_ranges_free` / `ts_node_string_free`，不能用 `Marshal.FreeHGlobal`
5. **所有 Dispose 方法幂等**：SafeHandle 的 `ReleaseHandle` 只被调用一次，TreeCursor 的 `_disposed` 标志防止重复 delete
6. **readonly struct 不能直接 ref 传字段**：`Node.Edit()` 需要先拷贝 `TSNode` 到局部变量再 `ref` 传入
7. **ref struct 不能用于 IEnumerable/async**：零分配的 `QueryMatch` 是 ref struct，只能在 `NextMatch`/`NextCapture` 的 out 参数中使用；IEnumerable 路径使用普通 struct `QueryMatchOwned`
