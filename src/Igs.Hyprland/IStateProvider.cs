namespace Igs.Hyprland;

public interface IStateProvider
{
	MonitorCollection Monitors { get; }
	WorkspaceCollection Workspaces { get; }
	WindowCollection Windows { get; }
}