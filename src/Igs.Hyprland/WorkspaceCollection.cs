using System.Collections;
using Igs.Hyprland.Ipc;

namespace Igs.Hyprland;

public class WorkspaceCollection : IEnumerable<Workspace>
{
	private readonly IHyprctlClient _hyprctlClient;
	private readonly ISignalReciever _signalReciever;
	private readonly IStateProvider _stateProvider;

	public WorkspaceFocus Focus { get; }

	#region Events
	/// <summary>
	/// Emitted on workspace change. Is emitted ONLY when a user requests a workspace change, and is not emitted on mouse movements.
	/// </summary>
	public event Action<Workspace>? OnActiveChanged;
	/// <summary>
	/// Emitted when a workspace is created.
	/// </summary>
	public event Action<Workspace>? OnCreated;
	/// <summary>
	/// Emitted when a workspace is destroyed.
	/// </summary>
	public event Action<Workspace>? OnDestroyed;
	/// <summary>
	/// Emitted when a workspace is moved to a different monitor.
	/// </summary>
	public event Action<Workspace>? OnMoved;
	/// <summary>
	/// Emitted when a workspace is renamed.
	/// </summary>
	public event Action<Workspace>? OnRenamed;
	/// <summary>
	/// Emitted when the special workspace opened in a monitor changes.
	/// </summary>
	public event Action<Workspace>? OnSpecialWorkspaceOpened;
	/// <summary>
	/// Emitted when the special workspace opened in a monitor closes.
	/// </summary>
	public event Action? OnSpecialWorkspaceClosed;

	#endregion

	internal WorkspaceCollection(IHyprctlClient hyprctlClient, ISignalReciever signalReciever, IStateProvider stateProvider)
	{
		_hyprctlClient = hyprctlClient;
		_signalReciever = signalReciever;
		_signalReciever.OnSignalRecieved += handleEvents;
		_stateProvider = stateProvider;

		Focus = new WorkspaceFocus(hyprctlClient, stateProvider);
	}

	public IEnumerator<Workspace> GetEnumerator()
	{
		Workspace.Hyprctl[]? workspaces = _hyprctlClient.Query<Workspace.Hyprctl[]>("workspaces");
		Workspace[] result = workspaces == null ? Array.Empty<Workspace>() : workspaces.Select(ws => new Workspace(ws, _stateProvider)).ToArray();
		return (result as IEnumerable<Workspace>).GetEnumerator();
	}
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


	private void handleEvents(string eventName, string[] args)
	{

		Action<Workspace>? @delegate = eventName switch
		{
			"workspace" => OnActiveChanged,
			"createworkspace" => OnCreated,
			"destroyworkspace" => OnDestroyed,
			"moveworkspace" => OnMoved,
			"renameworkspace" => OnRenamed,
			"activespecial" => OnSpecialWorkspaceOpened,
			_ => default
		};

		if (@delegate != null)
		{
			Workspace? workspace = this.FirstOrDefault(x => x.Name == args[0]);
			if (workspace != null)
				@delegate.Invoke(workspace);
		}

		if (eventName == "activespecial" && string.IsNullOrEmpty(args[0]))
			OnSpecialWorkspaceClosed?.Invoke();
	}
}