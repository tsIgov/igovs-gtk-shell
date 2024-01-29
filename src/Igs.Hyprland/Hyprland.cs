using Igs.Hyprland.Input;
using Igs.Hyprland.Ipc;
using Igs.Hyprland.Monitors;
using Igs.Hyprland.Windows;
using Igs.Hyprland.Workspaces;

namespace Igs.Hyprland;

public interface IHyprland : IDisposable
{
	MonitorCollection Monitors { get; }
	WorkspaceCollection Workspaces { get; }
	WindowCollection Windows { get; }

	Keyboard Keyboard { get; }

	event Action? OnConfigReloaded;

	void Initialize();
}

public class Hyprland : IHyprland
{
	private readonly IHyprctlClient _hyprctlClient;
	private readonly ISignalReceiver _signalReceiver;

	public MonitorCollection Monitors { get; }
	public WorkspaceCollection Workspaces { get; }
	public WindowCollection Windows { get; }

	public Keyboard Keyboard { get; }


	/// <summary>
	/// Emitted when the config is done reloading
	/// </summary>
	public event Action? OnConfigReloaded;

	public Hyprland(IHyprctlClient hyprctlClient, ISignalReceiver signalReciever)
	{
		_hyprctlClient = hyprctlClient;
		_signalReceiver = signalReciever;
		_signalReceiver.OnSignalReceived += handleSignal;

		Monitors = new MonitorCollection(_hyprctlClient, _signalReceiver, this);
		Workspaces = new WorkspaceCollection(_hyprctlClient, _signalReceiver, this);
		Windows = new WindowCollection(_hyprctlClient, _signalReceiver, this);

		Keyboard = new Keyboard(_signalReceiver);
	}

	public void Initialize() => _signalReceiver.StartListening();

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
			_signalReceiver?.Dispose();
	}
}
