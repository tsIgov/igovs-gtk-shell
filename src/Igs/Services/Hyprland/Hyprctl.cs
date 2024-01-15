using System.Net.Sockets;
using System.Text.Json;

namespace Igs.Services.Hyprland;

internal class Hyprctl
{
    private UnixDomainSocketEndPoint _endPoint;

    internal Hyprctl(string hyprlandInstanceSignature)
    {
        string socketPath = $"/tmp/hypr/{hyprlandInstanceSignature}/.socket.sock";
        _endPoint = new (socketPath);
    }

    internal State GetState()
    {
        HyprctlMonitor[] hyprctlMonitors = getHyprctlData<HyprctlMonitor[]>("monitors");
        HyprctlWindow[] hyprctlWindows = getHyprctlData<HyprctlWindow[]>("clients");
        HyprctlWorkspace[] hyprctlWorkspaces = getHyprctlData<HyprctlWorkspace[]>("workspaces");

        Dictionary<int, Monitor> monitors = new(hyprctlMonitors.Select(ws => new KeyValuePair<int, Monitor>(ws.Id, ws.Map())));
        Dictionary<string, Window> windows = new(hyprctlWindows.Select(ws => new KeyValuePair<string, Window>(ws.Address, ws.Map())));
        Dictionary<int, Workspace> workspaces = new(hyprctlWorkspaces.Select(ws => new KeyValuePair<int, Workspace>(ws.Id, ws.Map())));

        foreach(HyprctlWindow window in hyprctlWindows)
        {
            if (monitors.TryGetValue(window.Monitor, out Monitor? monitor))
                windows[window.Address].Monitor = monitor;

            if (workspaces.TryGetValue(window.Workspace.Id, out Workspace? workspace))
                windows[window.Address].Workspace = workspace;
        }

        foreach(HyprctlMonitor monitor in hyprctlMonitors)
        {
            if (workspaces.TryGetValue(monitor.ActiveWorkspace.Id, out Workspace? monitorActiveWorkspace))
                monitors[monitor.Id].ActiveWorkspace = monitorActiveWorkspace;

            if (workspaces.TryGetValue(monitor.SpecialWorkspace.Id, out Workspace? monitorSpecialWorkspace))
                monitors[monitor.Id].SpecialWorkspace = monitorSpecialWorkspace;
        }

        foreach(HyprctlWorkspace workspace in hyprctlWorkspaces)
        {
            if (monitors.TryGetValue(workspace.MonitorId, out Monitor? monitor))
                workspaces[workspace.Id].Monitor = monitor;

            if (windows.TryGetValue(workspace.LastWindow, out Window? window))
                workspaces[workspace.Id].LastWindow = window;
        }

        HyprctlWorkspace hyprctlActiveWorkspace = getHyprctlData<HyprctlWorkspace>("activeworkspace");
        HyprctlWindow hyprctlActiveWindow = getHyprctlData<HyprctlWindow>("activewindow");

        Workspace activeWorkspace = workspaces[hyprctlActiveWorkspace.Id];
        windows.TryGetValue(hyprctlActiveWindow.Address, out Window? activeWindow);

        State state = new(
            ActiveWorkspace: activeWorkspace,
            ActiveWindow: activeWindow,
            Monitors: monitors.Values.ToArray(),
            Workspaces: workspaces.Values.ToArray(),
            Windows: windows.Values.ToArray()
        );

        return state;
    }

    private T getHyprctlData<T>(string command)
    {
        using Socket socket = new (AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        socket.Connect(_endPoint);
        using NetworkStream stream = new(socket);
        using StreamWriter writer = new(stream);
        using StreamReader reader = new(stream);
        
        writer.Write($"j/{command}");
        writer.Flush();
        string rawResponse = reader.ReadToEnd();
        socket.Close();

        T response = JsonSerializer.Deserialize<T>(rawResponse, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        return response;
    }


    private record HyprctlMonitor(int Id, string Name, string Description, string Make, string Model, string Serial, int Width, int Height, float RefreshRate,
                                  int X, int Y, HyprctlWorkspaceReference ActiveWorkspace, HyprctlWorkspaceReference SpecialWorkspace,
                                  float Scale, bool Focused)
    {
        public Monitor Map() => new (
            Id: Id,
            Name: Name,
            Description: Description,
            Make: Make,
            Model: Model,
            Serial: Serial,
            Width: Width,
            Height: Height,
            RefreshRate: RefreshRate,
            X: X,
            Y: Y,
            Scale: Scale,
            Focused: Focused
        );
    }
    private record HyprctlWorkspace(int Id, string Name, string Monitor, int MonitorId, int Windows, bool HasFullScreen, string LastWindow, string LastWindowTitle)
    {
        public Workspace Map() => new (
            Id: Id,
            Name: Name,
            WindowsCount: Windows,
            HasFullScreen: HasFullScreen
        );
    }
    private record HyprctlWorkspaceReference(int Id, string Name);
    private record HyprctlWindow(string Address, bool Mapped, bool Hidden, int[] At, int[] Size, HyprctlWorkspaceReference Workspace, bool Floating,
                                 int Monitor, string Class, string Title, string InitialClass, string InitialTitle, int Pid, bool Fullscreen)
    {
        public Window Map() => new (
            Address: Address,
            Mapped: Mapped,
            Hidden: Hidden,
            X: At[0],
            Y: At[1],
            Width: Size[0],
            Height: Size[1],
            Floating: Floating,
            Class: Class,
            Title: Title,
            InitialClass: InitialClass,
            InitialTitle: InitialTitle,
            Pid: Pid,
            Fullscreen: Fullscreen
        );
    }
}