namespace Igs.Widgets;

using Gtk;
using Igs.Services.Hyprland;

public class Workspaces : Box
{
	public Workspaces()
	{
		for (int i = 1; i <= 10; i++)
			PackStart(new WorkspaceButton(i), false, false, 0);
	}
}

public class WorkspaceButton : Button
{
	private readonly int _workspaceId;

	public WorkspaceButton(int workspaceId)
	{
		_workspaceId = workspaceId;
		Label = workspaceId.ToString();
	}

	protected override void OnClicked()
	{
		base.OnClicked();
		//Hyprland.Instance.State.Workspaces.FirstOrDefault(x => x.Id == _workspaceId)?.Activate();

		//Hyprland.Instance.ChangeWorkspace(_workspaceId);
	}
}