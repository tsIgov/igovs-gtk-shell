namespace Igs.Hyprland;

public class Workspace
{
	private readonly IStateProvider _stateProvider;
	private string? _lastWindowAddress;

	public int Id { get; }
	public string Name { get; }
	public int WindowsCount { get; }
	public bool HasFullScreen { get; }
	public int? MonitorId { get; }

	public Window? LastWindow => _lastWindowAddress != null ? _stateProvider.Windows.GetByAddress(_lastWindowAddress) : null;

	internal Workspace(Hyprctl workspace, IStateProvider stateProvider)
	{
		_stateProvider = stateProvider;

		Id = workspace.Id;
		Name = workspace.Name;
		WindowsCount = workspace.Windows;
		HasFullScreen = workspace.HasFullScreen;
		MonitorId = workspace.MonitorId;
		_lastWindowAddress = workspace.LastWindow;
	}

	internal record HyprctlReference(int Id, string Name);

	internal record Hyprctl(int Id, string Name, string Monitor, int? MonitorId, int Windows, bool HasFullScreen, string LastWindow, string LastWindowTitle);
}