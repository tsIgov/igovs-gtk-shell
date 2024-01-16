namespace Igs.Services.Hyprland;

public interface IHyprlandState
{
    string InstanceSignature { get; }

    Workspace ActiveWorkspace { get; }
    Window? ActiveWindow { get; }

    IEnumerable<Monitor> Monitors { get; }
    IEnumerable<Workspace> Workspaces { get; }
    IEnumerable<Window> Windows { get; }

    /// <summary>
    /// Queries Hyprland for its current state and saves it.
    /// </summary>
    /// <returns>Returns this object to allow chaining.</returns>
    IHyprlandState Refresh();

    Window? GetWindow(string address);
    Workspace? GetWorkspace(string name);
    Monitor? GetMonitor(int id);  
}