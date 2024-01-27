
using Igs.Hyprland.Ipc;

namespace Igs.Hyprland;

public class WindowFocus
{
	private readonly IHyprctlClient _hyprctlClient;
	private readonly IStateProvider _stateProvider;

	public Window? Window
	{
		get
		{
			Window.Hyprctl? window = _hyprctlClient.Query<Window.Hyprctl>("activewindow");
			if (window == null)
				return null;

			Window result = new Window(window, _stateProvider);
			return result;
		}
	}

	internal WindowFocus(IHyprctlClient hyprctlClient, IStateProvider stateProvider)
	{
		_hyprctlClient = hyprctlClient;
		_stateProvider = stateProvider;
	}
}