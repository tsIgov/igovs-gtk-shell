using System.Diagnostics;
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
            {
                _instance = new Hyprland();
                _instance.initialize();
            }

            return _instance;
        }
    }

    private Socket _socket;
    private CancellationTokenSource _cancellationTokenSource = new ();

    public string Signature { get; private set; }

    public MonitorCollection Monitors { get; private set; } = null!;
    public WorkspaceCollection Workspaces { get; private set; } = null!;
    public WindowCollection Windows { get; private set; } = null!;

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
        _socket = new(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        Signature = getSignature();
    }
    private void initialize()
    {
        Monitors = new MonitorCollection();
        Workspaces = new WorkspaceCollection();
        Windows = new WindowCollection();

        startListeningForSignals();
    }

    private string getSignature()
    {
        ProcessStartInfo psi = new("bash")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = "-c \"echo $HYPRLAND_INSTANCE_SIGNATURE\""
        };

        Process process = new() { StartInfo = psi };
        process.Start();

        string output = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();

        if (string.IsNullOrWhiteSpace(output))
            throw new InvalidOperationException("Hyprland session not found");

        return output;
    }

    private void startListeningForSignals()
    {
        string socketPath = $"/tmp/hypr/{Signature}/.socket2.sock";
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
                    Console.Error.WriteLine($"Failed handling Hyprland signal.\nMessage: {message}\nException: {ex}");
                }
            }
        }, _cancellationTokenSource.Token);
    }

    private void handleSignal(string signal)
    {
        if (tryParseSignal(signal, out string eventName, out string[] parameters))
        {
            switch (eventName)
            {
                case "activewindowv2":
                case "openwindow":
                case "closewindow":
                case "movewindow":
                case "changefloatingmode":
                case "windowtitle":
                case "urgent":
                case "fullscreen":
                    {
                        Monitors.Refresh();
                        Workspaces.Refresh();
                        Windows.Refresh();

                        Windows.HandleEvents(eventName, Windows.GetByAddress(parameters[0])!);
                        break;
                    }

                case "monitoradded":
                case "monitorremoved":
                case "focusedmon":
                    {
                        Monitors.Refresh();
                        Workspaces.Refresh();
                        Windows.Refresh();

                        Monitors.HandleEvents(eventName, Monitors.GetByName(parameters[0])!);
                        break;
                    }

                case "createworkspace":
                case "destroyworkspace":
                case "workspace":
                case "moveworkspace":
                case "renameworkspace":
                case "activespecial":
                    {
                        Monitors.Refresh();
                        Workspaces.Refresh();
                        Windows.Refresh();

                        Workspaces.HandleEvents(eventName, Workspaces.GetByName(parameters[0])!);
                        break;
                    }

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