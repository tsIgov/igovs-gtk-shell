
using Igs.Hyprland.Ipc;

namespace Igs.Hyprland.Windows;

public class WindowFocus
{
	private readonly IHyprctlClient _hyprctlClient;
	private readonly IHyprland _hyprland;

	public Window? Window
	{
		get
		{
			Window.Hyprctl? window = _hyprctlClient.Query<Window.Hyprctl>("activewindow");
			if (window == null)
				return null;

			Window result = new Window(window, _hyprland);
			return result;
		}
	}

	internal WindowFocus(IHyprctlClient hyprctlClient, IHyprland hyprland)
	{
		_hyprctlClient = hyprctlClient;
		_hyprland = hyprland;
	}
}