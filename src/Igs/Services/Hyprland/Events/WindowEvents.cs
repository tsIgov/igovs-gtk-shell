namespace Igs.Services.Hyprland.Events;

public delegate void WindowEventHandler(WindowEventArgs args);
public delegate void WindowHandleEventHandler(WindowHandleEventArgs args);
public delegate void WindowOpenedEventHandler(WindowOpenedEventArgs args);
public delegate void WindowMovedEventHandler(WindowMovedEventArgs args);
public delegate void WindowFloatingEventHandler(WindowFloatingEventArgs args);
public delegate void WindowMinimizationEventHandler(WindowMinimizationEventArgs args);
public delegate void WindowFullscreenStateRequestedEventHandler(WindowFullscreenStateRequestedEventArgs args);

public record WindowEventArgs(string WindowClass, string WindowTitle);
public record WindowHandleEventArgs(string WindowAddress);
public record WindowOpenedEventArgs(string WindowAddress, string WindowClass, string WindowTitle, string WorkspaceName) : WindowHandleEventArgs(WindowAddress);
public record WindowMovedEventArgs(string WindowAddress, string WorkspaceName) : WindowHandleEventArgs(WindowAddress);
public record WindowFloatingEventArgs(string WindowAddress, bool IsFloating) : WindowHandleEventArgs(WindowAddress);
public record WindowMinimizationEventArgs(string WindowAddress, bool IsMinimized) : WindowHandleEventArgs(WindowAddress);
public record WindowFullscreenStateRequestedEventArgs(bool IsFullscreen);

public class WindowEvents
{
    /// <summary>
    /// Emitted on the active window being changed.
    /// </summary>
    public event WindowEventHandler? OnActiveWindowChanged;
    /// <summary>
    /// Emitted on the active window being changed.
    /// </summary>
    public event WindowHandleEventHandler? OnActiveWindowHandleChanged;
    /// <summary>
    /// Emitted when a window is opened.
    /// </summary>
    public event WindowOpenedEventHandler? OnWindowOpened;
    /// <summary>
    /// Emitted when a window is closed.
    /// </summary>
    public event WindowHandleEventHandler? OnWindowClosed;
    /// <summary>
    /// Emitted when a window is moved to a workspace.
    /// </summary>
    public event WindowMovedEventHandler? OnWindowMoved;
    /// <summary>
    /// Emitted when a window changes its floating mode.
    /// </summary>
    public event WindowFloatingEventHandler? OnWindowFloatingModeChanged;
    /// <summary>
    /// Emitted when a window requests an urgent state.
    /// </summary>
    public event WindowHandleEventHandler? OnWindowUrgentStateRequested;
    /// <summary>
    /// Emitted when a window requests a change to its minimized state.
    /// </summary>
    public event WindowMinimizationEventHandler? OnWindowMinimizedStateChanged;
    /// <summary>
    /// Emitted when a window title changes.
    /// </summary>
    public event WindowHandleEventHandler? OnWindowTitleChanged;
    /// <summary>
    /// Emitted when a fullscreen status of a window changes. A fullscreen event is not guaranteed to fire on/off once in succession. A window might do for example 3 requests to be fullscreen’d, which would result in 3 fullscreen events.
    /// </summary>
    public event WindowFullscreenStateRequestedEventHandler? OnWindowFullscreenStateRequested;

    internal WindowEvents() {}

    internal void InvokeOnActiveWindowChanged(WindowEventArgs args) =>
        OnActiveWindowChanged?.Invoke(args);
}