namespace TreeSitter.Wrap;

using System.Runtime.InteropServices;
using TreeSitter.Bind;

/// <summary>
/// 查询匹配结果（ref struct），直接持有原生 TSQueryCapture 指针，
/// 按需读取捕获数据，不分配数组。
/// <para>
/// 注意：原生指针仅在下次 NextMatch/NextCapture 调用前有效，
/// ref struct 的生命周期约束确保不会在失效后继续使用。
/// </para>
/// </summary>
public ref struct QueryMatch
{
	private static readonly int _captureNativeSize = Marshal.SizeOf<TSQueryCapture>();

	private readonly IntPtr _captures;
	private readonly int _captureCount;
	private readonly Tree? _tree;
	private readonly Query? _query;

	/// <summary>匹配 id。</summary>
	public int Id { get; }

	/// <summary>匹配的模式索引。</summary>
	public ushort PatternIndex { get; }

	/// <summary>捕获数量。</summary>
	public int CaptureCount => _captureCount;

	internal QueryMatch(TSQueryMatch nativeMatch, Tree? tree, Query? query)
	{
		Id = (int)nativeMatch.id;
		PatternIndex = nativeMatch.pattern_index;
		_captures = nativeMatch.captures;
		_captureCount = nativeMatch.capture_count;
		_tree = tree;
		_query = query;
	}

	/// <summary>
	/// 按需读取指定索引处的捕获，每次访问从原生内存构造 QueryCapture。
	/// </summary>
	public QueryCapture GetCapture(int index)
	{
		if ((uint)index >= (uint)_captureCount)
			throw new IndexOutOfRangeException();
		var nativeCap = Marshal.PtrToStructure<TSQueryCapture>(
			_captures + index * _captureNativeSize);
		return new QueryCapture(
			new Node(nativeCap.node, _tree!),
			_query!.CaptureNameForId(nativeCap.index));
	}

	/// <summary>返回用于迭代捕获结果的枚举器。</summary>
	public Enumerator CaptureEnumerator() => new(this);

	/// <summary>
	/// QueryMatch 的枚举器（ref struct），支持 foreach 迭代捕获结果。
	/// </summary>
	public ref struct Enumerator
	{
		private readonly QueryMatch _match;
		private int _index;

		internal Enumerator(QueryMatch match)
		{
			_match = match;
			_index = -1;
		}

		/// <summary>当前捕获。</summary>
		public QueryCapture Current => _match.GetCapture(_index);

		/// <summary>推进到下一个捕获。</summary>
		public bool MoveNext() => ++_index < _match._captureCount;

		/// <summary>返回自身，支持 foreach。</summary>
		public Enumerator GetEnumerator() => this;
	}
}

/// <summary>
/// 有分配查询匹配结果（普通 struct），可跨迭代安全使用，支持 LINQ。
/// Captures 为 QueryCapture[]，每次迭代独立拷贝。
/// </summary>
public readonly struct QueryMatchOwned
{
	/// <summary>匹配 id。</summary>
	public int Id { get; }

	/// <summary>匹配的模式索引。</summary>
	public ushort PatternIndex { get; }

	/// <summary>捕获结果数组（独立拷贝，可安全跨迭代使用）。</summary>
	public QueryCapture[] Captures { get; }

	internal QueryMatchOwned(int id, ushort patternIndex, QueryCapture[] captures)
	{
		Id = id;
		PatternIndex = patternIndex;
		Captures = captures;
	}
}
