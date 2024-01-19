using System.Diagnostics.CodeAnalysis;

namespace Igs.Services.Hyprland;

public class Workspace
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public int WindowsCount { get; private set; }
    public bool HasFullScreen { get; private set; }

    private int? _monitorId;
    private string _lastWindowAddress;

    public Monitor? Monitor => _monitorId.HasValue ? Hyprland.Instance.Monitors.GetById(_monitorId.Value) : null;
    public Window? LastWindow => Hyprland.Instance.Windows.GetByAddress(_lastWindowAddress);

    internal Workspace(Hyprctl workspace) => Map(workspace);

    [MemberNotNull(nameof(Name), nameof(_lastWindowAddress))]
    internal void Map(Hyprctl workspace)
    {
        Id = workspace.Id;
        Name = workspace.Name;
        WindowsCount = workspace.Windows;
        HasFullScreen = workspace.HasFullScreen;
        _monitorId = workspace.MonitorId;
        _lastWindowAddress = workspace.LastWindow;
    }

    internal record HyprctlReference(int Id, string Name);

    internal record Hyprctl(int Id, string Name, string Monitor, int? MonitorId, int Windows, bool HasFullScreen, string LastWindow, string LastWindowTitle);
}

