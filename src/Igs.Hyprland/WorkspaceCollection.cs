using System.Collections;
using Igs.Hyprland.Ipc;

namespace Igs.Hyprland;

public class WorkspaceCollection : IEnumerable<Workspace>
{
	private readonly IHyprctlClient _hyprctlClient;
	private readonly ISignalReciever _signalReciever;
	private readonly IStateProvider _stateProvider;

	private Workspace[] _workspaces = null!;
	private Dictionary<int, Workspace> _byId = null!;
	private Dictionary<string, Workspace> _byName = null!;
	private bool _isDirty = true;

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
	/// Emitted when the special workspace opened in a monitor changes (the argument is null when closing).
	/// </summary>
	public event Action<Workspace?>? OnSpecialWorkspaceActivated;
	#endregion

	internal WorkspaceCollection(IHyprctlClient hyprctlClient, ISignalReciever signalReciever, IStateProvider stateProvider)
	{
		_hyprctlClient = hyprctlClient;
		_signalReciever = signalReciever;
		_signalReciever.OnSignalRecieved += handleEvents;
		_stateProvider = stateProvider;
	}

	public Workspace? GetById(int id)
	{
		if (_isDirty)
			refresh();

		_byId.TryGetValue(id, out Workspace? value);
		return value;
	}
	public Workspace? GetByName(string name)
	{
		if (_isDirty)
			refresh();

		_byName.TryGetValue(name, out Workspace? value);
		return value;
	}
	public IEnumerator<Workspace> GetEnumerator() => refresh().GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	private IEnumerable<Workspace> refresh()
	{
		if (_isDirty)
		{
			Workspace.Hyprctl[] workspaces = _hyprctlClient.Query<Workspace.Hyprctl[]>("workspaces");
			_workspaces = workspaces.Select(ws => new Workspace(ws, _stateProvider)).ToArray();
			_byId = new Dictionary<int, Workspace>(_workspaces.Select(x => new KeyValuePair<int, Workspace>(x.Id, x)));
			_byName = new Dictionary<string, Workspace>(_workspaces.Select(x => new KeyValuePair<string, Workspace>(x.Name, x)));
			_isDirty = false;
		}

		return _workspaces;
	}

	private readonly HashSet<string> _eventsToHandle = new() { "workspace", "createworkspace", "destroyworkspace", "moveworkspace", "renameworkspace", "activespecial" };
	private void handleEvents(string eventName, string[] args)
	{
		if (!_eventsToHandle.Contains(eventName))
			return;

		_isDirty = true;

		Workspace? workspace = GetByName(args[0]);
		if (workspace == null)
			return;

		switch (eventName)
		{
			case "workspace": OnActiveChanged?.Invoke(workspace); break;
			case "createworkspace": OnCreated?.Invoke(workspace); break;
			case "destroyworkspace": OnDestroyed?.Invoke(workspace); break;
			case "moveworkspace": OnMoved?.Invoke(workspace); break;
			case "renameworkspace": OnRenamed?.Invoke(workspace); break;
			case "activespecial": OnSpecialWorkspaceActivated?.Invoke(workspace); break;
		}
	}
}