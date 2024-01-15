namespace Igs.Services.Hyprland.Events;

public delegate void ActiveMonitorChangedEventHandler(ActiveMonitorChangedEventArgs args);
public delegate void MonitorEventHandler(MonitorEventArgs args);

public record ActiveMonitorChangedEventArgs(string MonitorName, string WorkspaceName) : MonitorEventArgs(MonitorName);
public record MonitorEventArgs(string MonitorName);

public class MonitorEvents
{
    /// <summary>
    /// Emitted on the active monitor being changed.
    /// </summary>
    public event ActiveMonitorChangedEventHandler? OnActiveMonitorChanged;
    /// <summary>
    /// Emitted when a monitor is added (connected).
    /// </summary>
    public event MonitorEventHandler? OnMonitorAdded;
    /// <summary>
    /// Emitted when a monitor is removed (disconnected).
    /// </summary>
    public event MonitorEventHandler? OnMonitorRemoved;

    internal MonitorEvents() {}
}