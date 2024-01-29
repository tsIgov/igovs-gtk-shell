
using Igs.Hyprland.Ipc;

namespace Igs.Hyprland.Workspaces;

public class WorkspaceFocus
{
	private readonly IHyprctlClient _hyprctlClient;
	private readonly IHyprland _hyprland;

	public Workspace? Workspace
	{
		get
		{
			Workspace.Hyprctl? workspace = _hyprctlClient.Query<Workspace.Hyprctl>("activeworkspace");

			if (workspace == null)
				return null;

			Workspace result = new Workspace(workspace, _hyprland);
			return result;
		}
	}

	internal WorkspaceFocus(IHyprctlClient hyprctlClient, IHyprland hyprland)
	{
		_hyprctlClient = hyprctlClient;
		_hyprland = hyprland;
	}
}