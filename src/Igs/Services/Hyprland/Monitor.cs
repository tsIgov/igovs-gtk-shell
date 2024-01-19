using System.Diagnostics.CodeAnalysis;

namespace Igs.Services.Hyprland;

public class Monitor
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Make { get; private set; }
    public string Model { get; private set; }
    public string Serial { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public float RefreshRate { get; private set; }
    public int X { get; private set; }
    public int Y { get; private set; }
    public float Scale { get; private set; }
    public bool Focused { get; private set; }
    public bool VariableRefreshRateEnabled { get; private set; }
    public int ReservedSpaceLeft { get; private set; }
    public int ReservedSpaceTop { get; private set; }
    public int ReservedSpaceRight { get; private set; }
    public int ReservedSpaceBottom { get; private set; }

    private int? _activeWorkspceId;
    private int? _specialWorkspaceId;

    public Workspace? ActiveWorkspace => _activeWorkspceId.HasValue ? Hyprland.Instance.Workspaces.GetById(_activeWorkspceId.Value) : null;

    public Workspace? SpecialWorkspace => _specialWorkspaceId.HasValue ? Hyprland.Instance.Workspaces.GetById(_specialWorkspaceId.Value) : null;

    internal Monitor(Hyprctl monitor) => Map(monitor);

    [MemberNotNull(nameof(Name), nameof(Description), nameof(Make), nameof(Model), nameof(Serial))]
    internal void Map(Hyprctl monitor)
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
        ReservedSpaceLeft = monitor.Reserved[0];
        ReservedSpaceTop = monitor.Reserved[1];
        ReservedSpaceRight = monitor.Reserved[2];
        ReservedSpaceBottom = monitor.Reserved[3];
        Scale = monitor.Scale;
        Focused = monitor.Focused;
        VariableRefreshRateEnabled = monitor.Vrr;
        _activeWorkspceId = monitor.ActiveWorkspace?.Id;
        _specialWorkspaceId = monitor.SpecialWorkspace?.Id;
    }

    internal record Hyprctl(int Id, string Name, string Description, string Make, string Model, string Serial, int Width, int Height, float RefreshRate, int X, int Y, Workspace.HyprctlReference ActiveWorkspace, Workspace.HyprctlReference SpecialWorkspace, int[] Reserved, float Scale, bool Focused, bool Vrr);
}