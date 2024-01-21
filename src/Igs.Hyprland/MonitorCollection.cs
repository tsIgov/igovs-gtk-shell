using System.Collections;
using Igs.Hyprland.Ipc;

namespace Igs.Hyprland;

public class MonitorCollection : IEnumerable<Monitor>
{
	private readonly IHyprctlClient _hyprctlClient;
	private readonly ISignalReciever _signalReciever;
	private readonly IStateProvider _stateProvider;

	private Monitor[] _monitors = Array.Empty<Monitor>();
	private Dictionary<int, Monitor> _byId = null!;
	private Dictionary<string, Monitor> _byName = null!;
	private bool _isDirty = true;

	#region Events
	/// <summary>
	/// Emitted on the active monitor being changed.
	/// </summary>
	public event Action<Monitor>? OnActiveChanged;
	/// <summary>
	/// Emitted when a monitor is added (connected).
	/// </summary>
	public event Action<Monitor>? OnAdded;
	/// <summary>
	/// Emitted when a monitor is removed (disconnected).
	/// </summary>
	public event Action<Monitor>? OnRemoved;
	#endregion

	internal MonitorCollection(IHyprctlClient hyprctlClient, ISignalReciever signalReciever, IStateProvider stateProvider)
	{
		_hyprctlClient = hyprctlClient;
		_signalReciever = signalReciever;
		_signalReciever.OnSignalRecieved += handleEvents;
		_stateProvider = stateProvider;
	}

	public Monitor? GetById(int id)
	{
		if (_isDirty)
			refresh();

		_byId.TryGetValue(id, out Monitor? value);
		return value;
	}
	public Monitor? GetByName(string name)
	{
		if (_isDirty)
			refresh();

		_byName.TryGetValue(name, out Monitor? value);
		return value;
	}
	public IEnumerator<Monitor> GetEnumerator() => refresh().GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	private IEnumerable<Monitor> refresh()
	{
		if (_isDirty)
		{
			Monitor.Hyprctl[] monitors = _hyprctlClient.Query<Monitor.Hyprctl[]>("monitors");
			_monitors = monitors.Select(m => new Monitor(m, _stateProvider)).ToArray();
			_byId = new Dictionary<int, Monitor>(_monitors.Select(x => new KeyValuePair<int, Monitor>(x.Id, x)));
			_byName = new Dictionary<string, Monitor>(_monitors.Select(x => new KeyValuePair<string, Monitor>(x.Name, x)));
			_isDirty = false;
		}

		return _monitors;
	}

	private readonly HashSet<string> _eventsToHandle = new() { "monitoradded", "monitorremoved", "focusedmon" };
	private void handleEvents(string eventName, string[] args)
	{
		if (!_eventsToHandle.Contains(eventName))
			return;

		_isDirty = true;

		Monitor? monitor = GetByName(args[0]);
		if (monitor == null)
			return;

		switch (eventName)
		{
			case "monitoradded": OnAdded?.Invoke(monitor); break;
			case "monitorremoved": OnRemoved?.Invoke(monitor); break;
			case "focusedmon": OnActiveChanged?.Invoke(monitor); break;
		}
	}

}