namespace Igs.Services.Hyprland;

public record Window(string Address, bool Mapped, bool Hidden, int X, int Y, int Width, int Height, bool Floating, 
                     string Class, string Title, string InitialClass, string InitialTitle, int Pid, bool Fullscreen)
{
    public Monitor? Monitor { get; internal set; }
    public Workspace? Workspace { get; internal set; }
}