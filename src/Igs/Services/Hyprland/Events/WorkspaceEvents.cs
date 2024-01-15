namespace Igs.Services.Hyprland.Events;

public delegate void WorkspaceEventHandler(WorkspaceEventArgs args);
public delegate void WorkspaceMovedEventHandler(WorkspaceMovedEventArgs args);
public delegate void WorkspaceRenamedEventHandler(WorkspaceRenamedEventArgs args);

public record WorkspaceEventArgs(string WorkspaceName);
public record WorkspaceMovedEventArgs(string WorkspaceName, string MonitorName) : WorkspaceEventArgs(WorkspaceName);
public record WorkspaceRenamedEventArgs(string WorkspaceName, int WorkspaceId) : WorkspaceEventArgs(WorkspaceName);

public class WorkspaceEvents
{
    /// <summary>
    /// Emitted on workspace change. Is emitted ONLY when a user requests a workspace change, and is not emitted on mouse movements.
    /// </summary>
    public event WorkspaceEventHandler? OnWorkspaceChanged;
    /// <summary>
    /// Emitted when a workspace is created.
    /// </summary>
    public event WorkspaceEventHandler? OnWorkspaceCreated;
    /// <summary>
    /// Emitted when a workspace is destroyed.
    /// </summary>
    public event WorkspaceEventHandler? OnWorkspaceDestroyed;
    /// <summary>
    /// Emitted when a workspace is moved to a different monitor.
    /// </summary>
    public event WorkspaceMovedEventHandler? OnWorkspaceMoved;
    /// <summary>
    /// Emitted when a workspace is renamed.
    /// </summary>
    public event WorkspaceRenamedEventHandler? OnWorkspaceRenamed;
    /// <summary>
    /// Emitted when the special workspace opened in a monitor changes (closing results in an empty workspace name).
    /// </summary>
    public event WorkspaceMovedEventHandler? OnSpecialWorkspaceActivated;

    internal WorkspaceEvents() {}
}