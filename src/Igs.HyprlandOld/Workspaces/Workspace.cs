using System.Xml.Serialization;
using Igs.Hyprland.Ipc;
using Igs.Hyprland.Windows;
using Monitor = Igs.Hyprland.Monitors.Monitor;

namespace Igs.Hyprland.Workspaces;

public class Workspace
{
	private readonly IHyprland _hyprland;
	private readonly IHyprctlClient _hyprctl;
	private string? _lastWindowAddress;
	private int? _monitorId { get; }

	public int Id { get; }
	public string Name { get; }
	public int WindowsCount { get; }
	public bool HasFullScreen { get; }

	public Window? LastWindow => _lastWindowAddress != null ? _hyprland.Windows.FirstOrDefault(x => x.Address == _lastWindowAddress) : null;
	public Monitor? Monitor => _monitorId.HasValue ? _hyprland.Monitors.SingleOrDefault(x => x.Id == _monitorId.Value) : null;

	internal Workspace(Hyprctl workspace, IHyprland hyprland, IHyprctlClient hyprctl)
	{
		_hyprland = hyprland;
		_hyprctl = hyprctl;

		Id = workspace.Id;
		Name = workspace.Name;
		WindowsCount = workspace.Windows;
		HasFullScreen = workspace.HasFullScreen;
		_monitorId = workspace.MonitorId;
		_lastWindowAddress = workspace.LastWindow;
	}

	public void Focus() => _hyprctl.Dispatch($"workspace {Id}");

	internal record HyprctlReference(int Id, string Name);

	internal record Hyprctl(int Id, string Name, string Monitor, int? MonitorId, int Windows, bool HasFullScreen, string LastWindow, string LastWindowTitle);
}