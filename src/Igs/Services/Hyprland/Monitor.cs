namespace Igs.Services.Hyprland;

public class Monitor
{
    public int Id { get; }
    public string Name { get; }
    public string Description { get; }
    public string Make { get; }
    public string Model { get; }
    public string Serial { get; }
    public int Width { get; }
    public int Height { get; }
    public float RefreshRate { get; }
    public int X { get; }
    public int Y { get; }
    public float Scale { get; }
    public bool Focused { get; }

    public Workspace? ActiveWorkspace { get; internal set; }
    public Workspace? SpecialWorkspace { get; internal set; }

    internal Monitor(Hyprctl monitor)
    {
        Id = monitor.Id;
        Name = monitor.Name;
        Description = monitor.Description;
        Make = monitor.Make;
        Model = monitor.Model;
        Serial = monitor.Serial;
        Width = monitor.Width;
        Height = monitor.Height;
        RefreshRate = monitor.RefreshRate;
        X = monitor.X;
        Y = monitor.Y;
        Scale = monitor.Scale;
        Focused = monitor.Focused;
    }

    internal record Hyprctl(int Id, string Name, string Description, string Make, string Model, string Serial, int Width, int Height, float RefreshRate, int X, int Y, Workspace.HyprctlReference ActiveWorkspace, Workspace.HyprctlReference SpecialWorkspace, float Scale, bool Focused);
}