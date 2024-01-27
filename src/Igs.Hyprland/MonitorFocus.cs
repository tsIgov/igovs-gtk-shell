
using Igs.Hyprland.Ipc;

namespace Igs.Hyprland;

public class MonitorFocus
{
	private readonly IHyprctlClient _hyprctlClient;
	private readonly IStateProvider _stateProvider;

	public Monitor? Monitor => _stateProvider.Monitors.FirstOrDefault(x => x.Focused);

	internal MonitorFocus(IHyprctlClient hyprctlClient, IStateProvider stateProvider)
	{
		_hyprctlClient = hyprctlClient;
		_stateProvider = stateProvider;
	}
}