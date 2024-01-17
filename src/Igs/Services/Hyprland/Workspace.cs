namespace Igs.Services.Hyprland;

public class Workspace
{
    public int Id { get; }
    public string Name { get; }
    public int WindowsCount { get; }
    public bool HasFullScreen { get; }

    public Monitor? Monitor { get; internal set; }
    public Window? LastWindow { get; internal set; }

    internal Workspace(Hyprctl workspace)
    {
        Id = workspace.Id;
        Name = workspace.Name;
        WindowsCount = workspace.Windows;
        HasFullScreen = workspace.HasFullScreen;
    }

    internal record HyprctlReference(int Id, string Name);

    internal record Hyprctl(int Id, string Name, string Monitor, int MonitorId, int Windows, bool HasFullScreen, string LastWindow, string LastWindowTitle);
}

