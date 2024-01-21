using System.Collections;
using Igs.Hyprland.Ipc;

namespace Igs.Hyprland;

public class WindowCollection : IEnumerable<Window>
{
	private readonly IHyprctlClient _hyprctlClient;
	private readonly ISignalReciever _signalReciever;
	private readonly IStateProvider _stateProvider;

	private Window[] _windows = null!;
	private Dictionary<string, Window> _byAddress = null!;
	private bool _isDirty = true;

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
	/// Emitted when a fullscreen status of a window changes. A fullscreen event is not guaranteed to fire on/off once in succession. A window might do for example 3 requests to be fullscreen’d, which would result in 3 fullscreen events.
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
	}

	public Window? GetByAddress(string name)
	{
		if (_isDirty)
			refresh();

		_byAddress.TryGetValue(name, out Window? value);
		return value;
	}
	public IEnumerator<Window> GetEnumerator() => refresh().GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	private IEnumerable<Window> refresh()
	{
		if (_isDirty)
		{
			Window.Hyprctl[] windows = _hyprctlClient.Query<Window.Hyprctl[]>("clients");
			_windows = windows.Select(w => new Window(w, _stateProvider)).ToArray();
			_byAddress = new Dictionary<string, Window>(_windows.Select(x => new KeyValuePair<string, Window>(x.Address, x)));
			_isDirty = false;
		}

		return _windows;
	}

	private readonly HashSet<string> _eventsToHandle = new() { "activewindowv2", "openwindow", "closewindow", "movewindow", "changefloatingmode", "windowtitle", "urgent", "fullscreen", "minimize" };
	private void handleEvents(string eventName, string[] args)
	{
		if (!_eventsToHandle.Contains(eventName))
			return;

		_isDirty = true;

		Window? window = GetByAddress(args[0]);
		if (window == null)
			return;

		switch (eventName)
		{
			case "activewindowv2": OnActiveWindowChanged?.Invoke(window); break;
			case "openwindow": OnOpened?.Invoke(window); break;
			case "closewindow": OnClosed?.Invoke(window); break;
			case "movewindow": OnMovedToWorkspace?.Invoke(window); break;
			case "changefloatingmode": OnFloatingModeChanged?.Invoke(window); break;
			case "windowtitle": OnTitleChanged?.Invoke(window); break;
			case "urgent": OnUrgentStateRequested?.Invoke(window); break;
			case "fullscreen": OnFullscreenStateChangeRequested?.Invoke(window); break;
			case "minimize": OnMinimizedStateChangeRequested?.Invoke(window); break;
		}
	}
}