using Igs.Hyprland.Workspaces;

namespace Igs.Hyprland.Monitors;

public class Monitor
{
	private readonly IHyprland _hyprland;
	private readonly int? _activeWorkspceId;
	private readonly int? _specialWorkspaceId;

	public int Id { get; }
	public string Name { get; }
	public string? Description { get; }
	public string? Make { get; }
	public string? Model { get; }
	public string? Serial { get; }
	public int Width { get; }
	public int Height { get; }
	public float RefreshRate { get; }
	public int X { get; }
	public int Y { get; }
	public float Scale { get; }
	public bool Focused { get; }
	public bool VariableRefreshRateEnabled { get; }
	public int ReservedSpaceLeft { get; }
	public int ReservedSpaceTop { get; }
	public int ReservedSpaceRight { get; }
	public int ReservedSpaceBottom { get; }

	public Workspace? ActiveWorkspace => _activeWorkspceId.HasValue ? _hyprland.Workspaces.SingleOrDefault(x => x.Id == _activeWorkspceId.Value) : null;
	public Workspace? SpecialWorkspace => _specialWorkspaceId.HasValue ? _hyprland.Workspaces.SingleOrDefault(x => x.Id == _specialWorkspaceId.Value) : null;

	internal Monitor(Hyprctl monitor, IHyprland hyprland)
	{
		_hyprland = hyprland;

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