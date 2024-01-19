using System.Collections;

namespace Igs.Services.Hyprland;

public class MonitorCollection : IEnumerable<Monitor>
{
	private Dictionary<int, Monitor> _byId = new();
	private Dictionary<string, Monitor> _byName = new();

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

	internal MonitorCollection() => Refresh();
	internal void Refresh()
	{
		Monitor.Hyprctl[] hyprctlMonitors = Hyprctl.Query<Monitor.Hyprctl[]>("monitors");
		HashSet<int> current = new(hyprctlMonitors.Select(x => x.Id));

		foreach (Monitor.Hyprctl hyprctlMonitor in hyprctlMonitors)
		{
			int id = hyprctlMonitor.Id;
			if (_byId.ContainsKey(id))
				_byId[id].Map(hyprctlMonitor);
			else
				_byId.TryAdd(id, new Monitor(hyprctlMonitor));
		}

		foreach (int key in _byId.Keys.ToArray())
			if (!current.Contains(key))
				_byId.Remove(key);

		_byName = new(_byId.Values.Select(x => new KeyValuePair<string, Monitor>(x.Name, x)));
	}

	public Monitor? GetById(int id)
	{
		_byId.TryGetValue(id, out Monitor? monitor);
		return monitor;
	}
	public Monitor? GetByName(string name)
	{
		_byName.TryGetValue(name, out Monitor? monitor);
		return monitor;
	}
	public IEnumerator<Monitor> GetEnumerator() => _byId.Values.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	internal void HandleEvents(string eventName, Monitor arg)
	{
		switch (eventName)
		{
			case "monitoradded": OnAdded?.Invoke(arg); break;
			case "monitorremoved": OnRemoved?.Invoke(arg); break;
			case "focusedmon": OnActiveChanged?.Invoke(arg); break;
		}
	}
}