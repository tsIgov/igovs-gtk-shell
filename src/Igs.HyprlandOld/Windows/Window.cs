using Igs.Hyprland.Workspaces;
using Monitor = Igs.Hyprland.Monitors.Monitor;

namespace Igs.Hyprland.Windows;

public class Window
{
	private readonly IHyprland _hyprland;
	public int? _monitorId;
	public int? _workspaceId;
	public string _swallowingAddress;
	public string[] _windowGroupAddresses;

	public string Address { get; }
	public bool Mapped { get; }
	public bool Hidden { get; }
	public int X { get; }
	public int Y { get; }
	public int Width { get; }
	public int Height { get; }
	public bool Floating { get; }
	public string? Class { get; }
	public string? Title { get; }
	public string? InitialClass { get; }
	public string? InitialTitle { get; }
	public int Pid { get; }
	public bool Fullscreen { get; }
	public bool FakeFullscreen { get; }
	public bool XWayland { get; }
	public bool Pinned { get; }
	public int FocusHistoryId { get; }

	public Monitor? Monitor => _monitorId.HasValue ? _hyprland.Monitors.SingleOrDefault(x => x.Id == _monitorId.Value) : null;
	public Workspace? Workspace => _workspaceId.HasValue ? _hyprland.Workspaces.SingleOrDefault(x => x.Id == _workspaceId.Value) : null;
	public IEnumerable<Window> Group => _hyprland.Windows.Where(x => _windowGroupAddresses.Contains(x.Address));
	public Window? Swallowing => _hyprland.Windows.FirstOrDefault(x => x.Address == _swallowingAddress);

	internal Window(Hyprctl window, IHyprland hyprland)
	{
		_hyprland = hyprland;

		Address = window.Address;
		Mapped = window.Mapped;
		Hidden = window.Hidden;
		X = window.At[0];
		Y = window.At[1];
		Width = window.Size[0];
		Height = window.Size[1];
		Floating = window.Floating;
		Class = window.Class;
		Title = window.Title;
		InitialClass = window.InitialClass;
		InitialTitle = window.InitialTitle;
		Pid = window.Pid;
		Fullscreen = window.Fullscreen;
		FakeFullscreen = window.FakeFullscreen;
		FocusHistoryId = window.FocusHistoryId;
		_monitorId = window.Monitor;
		_workspaceId = window.Workspace?.Id;
		_windowGroupAddresses = window.Grouped;
		_swallowingAddress = window.Swallowing;
	}

	internal record Hyprctl(string Address, bool Mapped, bool Hidden, int[] At, int[] Size, Workspace.HyprctlReference Workspace, bool Floating, int? Monitor, string Class, string Title, string InitialClass, string InitialTitle, int Pid, bool Fullscreen, bool FakeFullscreen, string[] Grouped, int FocusHistoryId, string Swallowing);
}