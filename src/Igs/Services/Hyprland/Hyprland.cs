using System.Net.Sockets;

namespace Igs.Services.Hyprland;

public class Hyprland : IDisposable
{
    private static Hyprland? _instance;
    public static Hyprland Instance 
    { 
        get 
        {
            if (_instance == null)
                _instance = new Hyprland();

            return _instance;
        }
    }

    private Socket _socket;
    private CancellationTokenSource _cancellationTokenSource = new ();

    public IHyprlandState State { get; }

    #region Window Events
    /// <summary>
    /// Emitted on the active window being changed.
    /// </summary>
    public event Action<Window>? OnActiveWindowChanged;
    /// <summary>
    /// Emitted when a window is opened.
    /// </summary>
    public event Action<Window>? OnWindowOpened;
    /// <summary>
    /// Emitted when a window is closed.
    /// </summary>
    public event Action<Window>? OnWindowClosed;
    /// <summary>
    /// Emitted when a window is moved to a workspace.
    /// </summary>
    public event Action<Window>? OnWindowMoved;
    /// <summary>
    /// Emitted when a window changes its floating mode.
    /// </summary>
    public event Action<Window>? OnWindowFloatingModeChanged;
    /// <summary>
    /// Emitted when a window requests an urgent state.
    /// </summary>
    public event Action<Window>? OnWindowUrgentStateRequested;
    /// <summary>
    /// Emitted when a window title changes.
    /// </summary>
    public event Action<Window>? OnWindowTitleChanged;
    /// <summary>
    /// Emitted when a fullscreen status of a window changes. A fullscreen event is not guaranteed to fire on/off once in succession. A window might do for example 3 requests to be fullscreen’d, which would result in 3 fullscreen events.
    /// </summary>
    public event Action<Window>? OnWindowFullscreenStateRequested;
    #endregion

    #region Monitor Events
    /// <summary>
    /// Emitted on the active monitor being changed.
    /// </summary>
    public event Action<Monitor>? OnActiveMonitorChanged;
    /// <summary>
    /// Emitted when a monitor is added (connected).
    /// </summary>
    public event Action<Monitor>? OnMonitorAdded;
    /// <summary>
    /// Emitted when a monitor is removed (disconnected).
    /// </summary>
    public event Action<Monitor>? OnMonitorRemoved;
    #endregion

    #region Workspace Events
    /// <summary>
    /// Emitted on workspace change. Is emitted ONLY when a user requests a workspace change, and is not emitted on mouse movements.
    /// </summary>
    public event Action<Workspace>? OnWorkspaceChanged;
    /// <summary>
    /// Emitted when a workspace is created.
    /// </summary>
    public event Action<Workspace>? OnWorkspaceCreated;
    /// <summary>
    /// Emitted when a workspace is destroyed.
    /// </summary>
    public event Action<Workspace>? OnWorkspaceDestroyed;
    /// <summary>
    /// Emitted when a workspace is moved to a different monitor.
    /// </summary>
    public event Action<Workspace>? OnWorkspaceMoved;
    /// <summary>
    /// Emitted when a workspace is renamed.
    /// </summary>
    public event Action<Workspace>? OnWorkspaceRenamed;
    /// <summary>
    /// Emitted when the special workspace opened in a monitor changes (the argument is null when closing).
    /// </summary>
    public event Action<Workspace?>? OnSpecialWorkspaceActivated;
    #endregion

    #region KeyboardEvents
    public delegate void KeyboardLayoutEventHandler(string KeyboardName, string LayoutName);
    public delegate void KeybindSubmapEventHandler(string SubmapName);

    /// <summary>
    /// Emitted on a layout change of the active keyboard.
    /// </summary>
    public event KeyboardLayoutEventHandler? OnKeyboardLayoutChanged;
    /// <summary>
    /// Emitted when a keybind submap changes. Empty means default.
    /// </summary>
    public event KeybindSubmapEventHandler? OnKeybindSubmapChanged;
    #endregion

    private Hyprland()
    {
        State = new HyprlandState();
        _socket = new (AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

        startListeningForSignals();
    }

    private void startListeningForSignals()
    {
        string socketPath = $"/tmp/hypr/{State.InstanceSignature}/.socket2.sock";
        UnixDomainSocketEndPoint endPoint = new (socketPath);
        _socket.Connect(endPoint);

        Task _ = Task.Run(() =>
        {
            using NetworkStream stream = new (_socket);
            using StreamReader reader = new (stream);

            while(true)
            {
                if(_cancellationTokenSource.Token.IsCancellationRequested)
                    return;

                string? message = null;
                try 
                {
                    message = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(message))
                        handleSignal(message);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Failed handling Hyprland signal.\nMessage: {message}\nException: {ex}");
                }
            }
        }, _cancellationTokenSource.Token);
    }

    private void handleSignal(string signal)
    {
        if (tryParseSignal(signal, out string eventName, out string[] parameters))
        {
            switch(eventName)
            {
                case "activewindowv2": OnActiveWindowChanged?.Invoke(State.Refresh().GetWindow(parameters[0])!); break;
                case "openwindow": OnWindowOpened?.Invoke(State.Refresh().GetWindow(parameters[0])!); break;
                case "closewindow": OnWindowClosed?.Invoke(State.Refresh().GetWindow(parameters[0])!); break;
                case "movewindow": OnWindowMoved?.Invoke(State.Refresh().GetWindow(parameters[0])!); break;
                case "changefloatingmode": OnWindowFloatingModeChanged?.Invoke(State.Refresh().GetWindow(parameters[0])!); break;
                case "windowtitle": OnWindowTitleChanged?.Invoke(State.Refresh().GetWindow(parameters[0])!); break;
                case "urgent": OnWindowUrgentStateRequested?.Invoke(State.Refresh().GetWindow(parameters[0])!); break;
                case "fullscreen": OnWindowFullscreenStateRequested?.Invoke(State.Refresh().GetWindow(parameters[0])!); break;

                case "monitoradded": OnMonitorAdded?.Invoke(State.Refresh().Monitors.FirstOrDefault(m => m.Name == parameters[0])!); break;
                case "monitorremoved": OnMonitorRemoved?.Invoke(State.Refresh().Monitors.FirstOrDefault(m => m.Name == parameters[0])!); break;
                case "focusedmon": OnActiveMonitorChanged?.Invoke(State.Refresh().Monitors.FirstOrDefault(m => m.Name == parameters[0])!); break;

                case "createworkspace": OnWorkspaceCreated?.Invoke(State.Refresh().GetWorkspace(parameters[0])!); break;
                case "destroyworkspace": OnWorkspaceDestroyed?.Invoke(State.Refresh().GetWorkspace(parameters[0])!); break;
                case "workspace": OnWorkspaceChanged?.Invoke(State.Refresh().GetWorkspace(parameters[0])!); break;
                case "moveworkspace": OnWorkspaceMoved?.Invoke(State.Refresh().GetWorkspace(parameters[0])!); break;
                case "renameworkspace": OnWorkspaceRenamed?.Invoke(State.Refresh().Workspaces.FirstOrDefault(ws => ws.Id.ToString() == parameters[0])!); break;
                case "activespecial": OnSpecialWorkspaceActivated?.Invoke(State.Refresh().GetWorkspace(parameters[0])!); break;

                case "activelayout": OnKeyboardLayoutChanged?.Invoke(parameters[0], parameters[1]); break;
                case "submap": OnKeybindSubmapChanged?.Invoke(parameters[0]); break;

                default: HandleCustomSignal(eventName, parameters); break;
            }
        }
    }

    private bool tryParseSignal(string signal, out string eventName, out string[] parameters)
    {
        signal = signal.Trim();
        int index = signal.IndexOf(">>");
        if (index < 0)
        {
            Console.Error.WriteLine($"Unable to parse Hyprland signal: \"{signal}\"");
            eventName = string.Empty;
            parameters = Array.Empty<string>();
            return false;
        }

        eventName = signal[..index];
        parameters = signal[(index + ">>".Length)..].Split(',');
        return true;
    }

    protected virtual void HandleCustomSignal(string eventName, string[] parameters) {}

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cancellationTokenSource.Cancel();
            _socket?.Dispose();
        }
    }
}