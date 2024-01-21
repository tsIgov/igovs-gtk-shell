using Igs.Hyprland.Ipc;
using Microsoft.Extensions.DependencyInjection;

namespace Igs.Hyprland;

public interface IHyprland : IStateProvider, IDisposable
{
	public event Action? OnConfigReloaded;

	void Initialize();
}

public class Hyprland : IHyprland
{
	private readonly IHyprctlClient _hyprctlClient;
	private readonly ISignalReciever _signalReciever;

	public MonitorCollection Monitors { get; }
	public WorkspaceCollection Workspaces { get; }
	public WindowCollection Windows { get; }

	/// <summary>
	/// Emitted when the config is done reloading
	/// </summary>
	public event Action? OnConfigReloaded;

	public Hyprland(IHyprctlClient hyprctlClient, ISignalReciever signalReciever)
	{
		_hyprctlClient = hyprctlClient;
		_signalReciever = signalReciever;
		_signalReciever.OnSignalRecieved += handleSignal;

		Monitors = new MonitorCollection(_hyprctlClient, _signalReciever, this);
		Workspaces = new WorkspaceCollection(_hyprctlClient, _signalReciever, this);
		Windows = new WindowCollection(_hyprctlClient, _signalReciever, this);
	}

	public void Initialize() => _signalReciever.StartListening();

	private void handleSignal(string eventName, string[] args)
	{
		if (eventName == "configreloaded")
			OnConfigReloaded?.Invoke();
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
			_signalReciever?.Dispose();
	}
}
