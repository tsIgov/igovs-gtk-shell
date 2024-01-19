using System.Diagnostics.CodeAnalysis;

namespace Igs.Services.Hyprland;

public class Window
{
    public string Address { get; private set; }
    public bool Mapped { get; private set; }
    public bool Hidden { get; private set; }
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool Floating { get; private set; }
    public string Class { get; private set; }
    public string Title { get; private set; }
    public string InitialClass { get; private set; }
    public string InitialTitle { get; private set; }
    public int Pid { get; private set; }
    public bool Fullscreen { get; private set; }
    public bool FakeFullscreen { get; private set; }
    public bool XWayland { get; private set; }
    public bool Pinned { get; private set; }
    public int FocusHistoryId { get; private set; }

    private int? _monitorId;
    private int? _workspaceId;
    private string _swallowingAddress;
    private HashSet<string> _groupAddresses;

    public Monitor? Monitor => _monitorId.HasValue ? Hyprland.Instance.Monitors.GetById(_monitorId.Value) : null;
    public Workspace? Workspace => _workspaceId.HasValue ? Hyprland.Instance.Workspaces.GetById(_workspaceId.Value) : null;
    public IEnumerable<Window> Group => Hyprland.Instance.Windows.Where(x => _groupAddresses.Contains(x.Address));
    public Window? Swallowing => Hyprland.Instance.Windows.GetByAddress(_swallowingAddress);

    internal Window(Hyprctl window) => Map(window);

    [MemberNotNull(nameof(Address), nameof(Class), nameof(Title), nameof(InitialClass), nameof(InitialTitle), nameof(_groupAddresses), nameof(_swallowingAddress))]
    internal void Map(Hyprctl window)
    {
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
        _groupAddresses = new HashSet<string>(window.Grouped);
        _swallowingAddress = window.Swallowing;
    }

    internal record Hyprctl(string Address, bool Mapped, bool Hidden, int[] At, int[] Size, Workspace.HyprctlReference Workspace, bool Floating, int? Monitor, string Class, string Title, string InitialClass, string InitialTitle, int Pid, bool Fullscreen, bool FakeFullscreen, string[] Grouped, int FocusHistoryId, string Swallowing);
}