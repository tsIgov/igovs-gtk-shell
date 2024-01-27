namespace Igs.Hyprland;

public class Workspace
{
	private readonly IStateProvider _stateProvider;
	private string? _lastWindowAddress;
	private int? _monitorId { get; }

	public int Id { get; }
	public string Name { get; }
	public int WindowsCount { get; }
	public bool HasFullScreen { get; }

	public Window? LastWindow => _lastWindowAddress != null ? _stateProvider.Windows.FirstOrDefault(x => x.Address == _lastWindowAddress) : null;
	public Monitor? Monitor => _monitorId.HasValue ? _stateProvider.Monitors.SingleOrDefault(x => x.Id == _monitorId.Value) : null;

	internal Workspace(Hyprctl workspace, IStateProvider stateProvider)
	{
		_stateProvider = stateProvider;

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