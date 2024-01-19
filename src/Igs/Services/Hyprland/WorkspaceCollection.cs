using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Igs.Services.Hyprland;

public class WorkspaceCollection : IEnumerable<Workspace>
{
	private Dictionary<int, Workspace> _byId = new();
	private Dictionary<string, Workspace> _byName = new();

	public Workspace Active { get; private set; }

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

	internal WorkspaceCollection() => Refresh();
	[MemberNotNull(nameof(Active))]
	internal void Refresh()
	{
		Workspace.Hyprctl[] hyprctlWorkspaces = Hyprctl.Query<Workspace.Hyprctl[]>("workspaces");
		HashSet<int> current = new(hyprctlWorkspaces.Select(x => x.Id));

		foreach (Workspace.Hyprctl hyprctlWorkspace in hyprctlWorkspaces)
		{
			int id = hyprctlWorkspace.Id;
			if (_byId.ContainsKey(id))
				_byId[id].Map(hyprctlWorkspace);
			else
				_byId.TryAdd(id, new Workspace(hyprctlWorkspace));
		}

		foreach (int key in _byId.Keys.ToArray())
			if (!current.Contains(key))
				_byId.Remove(key);

		_byName = new(_byId.Values.Select(x => new KeyValuePair<string, Workspace>(x.Name, x)));

		Workspace.Hyprctl hyprctlActiveWorkspace = Hyprctl.Query<Workspace.Hyprctl>("activeworkspace");
		Active = GetById(hyprctlActiveWorkspace.Id)!;
	}

	public Workspace? GetById(int id)
	{
		_byId.TryGetValue(id, out Workspace? workspace);
		return workspace;
	}
	public Workspace? GetByName(string name)
	{
		_byName.TryGetValue(name, out Workspace? workspace);
		return workspace;
	}
	public IEnumerator<Workspace> GetEnumerator() => _byId.Values.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	internal void HandleEvents(string eventName, Workspace arg)
	{
		switch (eventName)
		{
			case "workspace": OnActiveChanged?.Invoke(arg); break;
			case "createworkspace": OnCreated?.Invoke(arg); break;
			case "destroyworkspace": OnDestroyed?.Invoke(arg); break;
			case "moveworkspace": OnMoved?.Invoke(arg); break;
			case "renameworkspace": OnRenamed?.Invoke(arg); break;
			case "activespecial": OnSpecialWorkspaceActivated?.Invoke(arg); break;
		}
	}
}