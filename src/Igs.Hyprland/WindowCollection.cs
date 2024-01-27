using System.Collections;
using Igs.Hyprland.Ipc;

namespace Igs.Hyprland;

public class WindowCollection : IEnumerable<Window>
{
	private readonly IHyprctlClient _hyprctlClient;
	private readonly ISignalReciever _signalReciever;
	private readonly IStateProvider _stateProvider;

	public WindowFocus Focus { get; }

	#region Events
	/// <summary>
	/// Emitted on the active window being changed.
	/// </summary>
	public event Action<Window>? OnActiveWindowChanged;
	/// <summary>
	/// Emitted when a window is opened.
	/// </summary>
	public event Action<Window>? OnOpened;
	/// <summary>
	/// Emitted when a window is closed.
	/// </summary>
	public event Action<Window>? OnClosed;
	/// <summary>
	/// Emitted when a window is moved to a workspace.
	/// </summary>
	public event Action<Window>? OnMovedToWorkspace;
	/// <summary>
	/// Emitted when a window changes its floating mode.
	/// </summary>
	public event Action<Window>? OnFloatingModeChanged;
	/// <summary>
	/// Emitted when a window requests an urgent state.
	/// </summary>
	public event Action<Window>? OnUrgentStateRequested;
	/// <summary>
	/// Emitted when a window title changes.
	/// </summary>
	public event Action<Window>? OnTitleChanged;
	/// <summary>
	/// Emitted when a fullscreen status of a window changes. A fullscreen event is not guaranteed to fire on/off once in succession. A window might do for example 3 requests to be fullscreened, which would result in 3 fullscreen events.
	/// </summary>
	public event Action<Window>? OnFullscreenStateChangeRequested;
	/// <summary>
	/// Emitted when a window requests a change to its minimized state.
	/// </summary>
	public event Action<Window>? OnMinimizedStateChangeRequested;
	#endregion

	internal WindowCollection(IHyprctlClient hyprctlClient, ISignalReciever signalReciever, IStateProvider stateProvider)
	{
		_hyprctlClient = hyprctlClient;
		_signalReciever = signalReciever;
		_signalReciever.OnSignalRecieved += handleEvents;
		_stateProvider = stateProvider;

		Focus = new WindowFocus(hyprctlClient, stateProvider);
	}

	public IEnumerator<Window> GetEnumerator()
	{
		Window.Hyprctl[]? windows = _hyprctlClient.Query<Window.Hyprctl[]>("clients");
		Window[] result = windows == null ? Array.Empty<Window>() : windows.Select(w => new Window(w, _stateProvider)).ToArray();
		return (result as IEnumerable<Window>).GetEnumerator();
	}
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	private void handleEvents(string eventName, string[] args)
	{
		Action<Window>? @delegate = eventName switch
		{
			"activewindowv2" => OnActiveWindowChanged,
			"openwindow" => OnOpened,
			"closewindow" => OnClosed,
			"movewindow" => OnMovedToWorkspace,
			"changefloatingmode" => OnFloatingModeChanged,
			"windowtitle" => OnTitleChanged,
			"urgent" => OnUrgentStateRequested,
			"fullscreen" => OnFullscreenStateChangeRequested,
			"minimize" => OnMinimizedStateChangeRequested,
			_ => default
		};

		if (@delegate != null)
		{
			Window? window = this.SingleOrDefault(x => x.Address == args[0]);
			if (window != null)
				@delegate.Invoke(window);
		}
	}
}