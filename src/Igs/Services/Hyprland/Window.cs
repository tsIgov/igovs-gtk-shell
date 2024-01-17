namespace Igs.Services.Hyprland;

public class Window
{
    public string Address { get; }
    public bool Mapped { get; }
    public bool Hidden { get; }
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }
    public bool Floating { get; }
    public string Class { get; }
    public string Title { get; }
    public string InitialClass { get; }
    public string InitialTitle { get; }
    public int Pid { get; }
    public bool Fullscreen { get; }

    public Monitor? Monitor { get; internal set; }
    public Workspace? Workspace { get; internal set; }

    internal Window(Hyprctl window)
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
    }

    internal record Hyprctl(string Address, bool Mapped, bool Hidden, int[] At, int[] Size, Workspace.HyprctlReference Workspace, bool Floating, int Monitor, string Class, string Title, string InitialClass, string InitialTitle, int Pid, bool Fullscreen);
}