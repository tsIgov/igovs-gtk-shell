
using Igs.Hyprland.Ipc;

namespace Igs.Hyprland.Monitors;

public class MonitorFocus
{
	private readonly IHyprctlClient _hyprctlClient;
	private readonly IHyprland _hyprland;

	public Monitor? Monitor => _hyprland.Monitors.FirstOrDefault(x => x.Focused);

	internal MonitorFocus(IHyprctlClient hyprctlClient, IHyprland hyprland)
	{
		_hyprctlClient = hyprctlClient;
		_hyprland = hyprland;
	}
}