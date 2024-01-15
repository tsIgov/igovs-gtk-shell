namespace Igs.Services.Hyprland;

public record State(Workspace ActiveWorkspace, Window? ActiveWindow, Monitor[] Monitors, Workspace[] Workspaces, Window[] Windows);