using System.Collections;

namespace Igs.Services.Hyprland;

public class WindowCollection : IEnumerable<Window>
{
	private Dictionary<string, Window> _byAddress = new();

	public Window? Active { get; private set; }

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

	internal WindowCollection() => Refresh();
	internal void Refresh()
	{
		Window.Hyprctl[] hyprctlWindows = Hyprctl.Query<Window.Hyprctl[]>("clients");
		HashSet<string> current = new(hyprctlWindows.Select(x => x.Address));

		foreach (Window.Hyprctl hyprctlWindow in hyprctlWindows)
		{
			string address = hyprctlWindow.Address;
			if (_byAddress.ContainsKey(address))
				_byAddress[address].Map(hyprctlWindow);
			else
				_byAddress.TryAdd(address, new Window(hyprctlWindow));
		}

		foreach (string key in _byAddress.Keys.ToArray())
			if (!current.Contains(key))
				_byAddress.Remove(key);


		_byAddress = new(hyprctlWindows.Select(w => new KeyValuePair<string, Window>(w.Address, new Window(w))));

		setActiveWindow();
	}
	private void setActiveWindow()
	{
		Window.Hyprctl hyprctlActiveWindow = Hyprctl.Query<Window.Hyprctl>("activewindow");
		Window? activeWindow = null;
		if (!string.IsNullOrEmpty(hyprctlActiveWindow.Address))
			_byAddress.TryGetValue(hyprctlActiveWindow.Address, out activeWindow);
		Active = activeWindow;
	}

	internal void HandleEvents(string eventName, Window arg)
	{
		switch (eventName)
		{
			case "activewindowv2": OnActiveWindowChanged?.Invoke(arg); break;
			case "openwindow": OnOpened?.Invoke(arg); break;
			case "closewindow": OnClosed?.Invoke(arg); break;
			case "movewindow": OnMovedToWorkspace?.Invoke(arg); break;
			case "changefloatingmode": OnFloatingModeChanged?.Invoke(arg); break;
			case "windowtitle": OnTitleChanged?.Invoke(arg); break;
			case "urgent": OnUrgentStateRequested?.Invoke(arg); break;
			case "fullscreen": OnFullscreenStateChangeRequested?.Invoke(arg); break;
			case "minimize": OnMinimizedStateChangeRequested?.Invoke(arg); break;
		}
	}

	public Window? GetByAddress(string address)
	{
		_byAddress.TryGetValue(address, out Window? window);
		return window;
	}
	public IEnumerator<Window> GetEnumerator() => _byAddress.Values.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}