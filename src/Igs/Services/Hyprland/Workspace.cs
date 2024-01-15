namespace Igs.Services.Hyprland;

public record Workspace(int Id, string Name, int WindowsCount, bool HasFullScreen)
{
    public Monitor? Monitor { get; internal set; }
    public Window? LastWindow { get; internal set; }
}

