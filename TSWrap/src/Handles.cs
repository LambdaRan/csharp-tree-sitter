namespace TreeSitter.Wrap;

using System.Runtime.InteropServices;
using TreeSitter.Bind;

/// <summary>TSParser 的 SafeHandle 包装。</summary>
internal sealed class ParserHandle : SafeHandle
{
	public ParserHandle(IntPtr handle) : base(IntPtr.Zero, ownsHandle: true)
	{
		SetHandle(handle);
	}

	public override bool IsInvalid => handle == IntPtr.Zero;

	protected override bool ReleaseHandle()
	{
		TSApi.ts_parser_delete(handle);
		return true;
	}
}

/// <summary>TSTree 的 SafeHandle 包装。</summary>
internal sealed class TreeHandle : SafeHandle
{
	public TreeHandle(IntPtr handle) : base(IntPtr.Zero, ownsHandle: true)
	{
		SetHandle(handle);
	}

	public override bool IsInvalid => handle == IntPtr.Zero;

	protected override bool ReleaseHandle()
	{
		TSApi.ts_tree_delete(handle);
		return true;
	}
}

/// <summary>TSLanguage 的 SafeHandle 包装，支持 ownsHandle 标志控制释放行为。</summary>
internal sealed class LanguageHandle : SafeHandle
{
	private readonly bool _ownsLanguage;

	public LanguageHandle(IntPtr handle, bool ownsHandle) : base(IntPtr.Zero, ownsHandle: true)
	{
		_ownsLanguage = ownsHandle;
		SetHandle(handle);
	}

	public override bool IsInvalid => handle == IntPtr.Zero;

	protected override bool ReleaseHandle()
	{
		if (_ownsLanguage) {
			TSApi.ts_language_delete(handle);
		}
		return true;
	}
}

/// <summary>TSQuery 的 SafeHandle 包装。</summary>
internal sealed class QueryHandle : SafeHandle
{
	public QueryHandle(IntPtr handle) : base(IntPtr.Zero, ownsHandle: true)
	{
		SetHandle(handle);
	}

	public override bool IsInvalid => handle == IntPtr.Zero;

	protected override bool ReleaseHandle()
	{
		TSApi.ts_query_delete(handle);
		return true;
	}
}

/// <summary>TSQueryCursor 的 SafeHandle 包装。</summary>
internal sealed class QueryCursorHandle : SafeHandle
{
	public QueryCursorHandle(IntPtr handle) : base(IntPtr.Zero, ownsHandle: true)
	{
		SetHandle(handle);
	}

	public override bool IsInvalid => handle == IntPtr.Zero;

	protected override bool ReleaseHandle()
	{
		TSApi.ts_query_cursor_delete(handle);
		return true;
	}
}
