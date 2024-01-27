
using Igs.Hyprland.Ipc;

namespace Igs.Hyprland;

public class WorkspaceFocus
{
	private readonly IHyprctlClient _hyprctlClient;
	private readonly IStateProvider _stateProvider;

	public Workspace? Workspace
	{
		get
		{
			Workspace.Hyprctl? workspace = _hyprctlClient.Query<Workspace.Hyprctl>("activeworkspace");

			if (workspace == null)
				return null;

			Workspace result = new Workspace(workspace, _stateProvider);
			return result;
		}
	}

	internal WorkspaceFocus(IHyprctlClient hyprctlClient, IStateProvider stateProvider)
	{
		_hyprctlClient = hyprctlClient;
		_stateProvider = stateProvider;
	}
}