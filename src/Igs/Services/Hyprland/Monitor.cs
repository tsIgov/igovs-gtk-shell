namespace Igs.Services.Hyprland;

public record Monitor(int Id, string Name, string Description, string Make, string Model,
                      string Serial, int Width, int Height, float RefreshRate, int X, int Y, 
                      float Scale, bool Focused)
{
    public Workspace? ActiveWorkspace { get; internal set; }
    public Workspace? SpecialWorkspace { get; internal set; }
}