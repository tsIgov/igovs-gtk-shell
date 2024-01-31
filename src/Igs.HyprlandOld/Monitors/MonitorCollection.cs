using System.Collections;
using Igs.Hyprland.Ipc;

namespace Igs.Hyprland.Monitors;

public class MonitorCollection : IEnumerable<Monitor>
{
	private readonly IHyprctlClient _hyprctlClient;
	private readonly ISignalReceiver _signalReceiver;
	private readonly IHyprland _hyprland;

	public MonitorFocus Focus { get; }

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

	internal MonitorCollection(IHyprctlClient hyprctlClient, ISignalReceiver signalReceiver, IHyprland hyprland)
	{
		_hyprctlClient = hyprctlClient;
		_signalReceiver = signalReceiver;
		_signalReceiver.OnSignalReceived += handleEvents;
		_hyprland = hyprland;

		Focus = new MonitorFocus(hyprctlClient, hyprland);
	}

	public IEnumerator<Monitor> GetEnumerator()
	{
		Monitor.Hyprctl[]? monitors = _hyprctlClient.Query<Monitor.Hyprctl[]>("monitors");
		Monitor[] result = monitors == null ? Array.Empty<Monitor>() : monitors.Select(m => new Monitor(m, _hyprland)).ToArray();
		return (result as IEnumerable<Monitor>).GetEnumerator();
	}
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	private void handleEvents(string eventName, string[] args)
	{
		Action<Monitor>? @delegate = eventName switch
		{
			"monitoradded" => OnAdded,
			"monitorremoved" => OnRemoved,
			"focusedmon" => OnActiveChanged,
			_ => default
		};

		if (@delegate != null)
		{
			Monitor? monitor = this.FirstOrDefault(x => x.Name == args[0]);
			if (monitor != null)
				@delegate.Invoke(monitor);
		}
	}

}