using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Text.Json;

namespace Igs.Services.Hyprland;

public class HyprlandState : IHyprlandState
{
    private Dictionary<int, Monitor> _monitors;
    private Dictionary<string, Window> _windows;
    private Dictionary<string, Workspace> _workspaces;

    public string InstanceSignature { get; }

    public IEnumerable<Monitor> Monitors => _monitors.Values;
    public IEnumerable<Workspace> Workspaces => _workspaces.Values;
    public IEnumerable<Window> Windows => _windows.Values;

    public Workspace ActiveWorkspace { get; private set; }
    public Window? ActiveWindow { get; private set; }

    internal HyprlandState()
    {
        InstanceSignature = getHyprlandInstanceSignature();

        if (string.IsNullOrWhiteSpace(InstanceSignature))
            throw new InvalidOperationException("Hyprland session not found");

        Refresh();
    }
    
    private string getHyprlandInstanceSignature()
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

        Process process = new () { StartInfo = psi };
        process.Start();

        string output = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();

        return output;
    }

    [MemberNotNull(nameof(_monitors), nameof(_windows), nameof(_workspaces), nameof(ActiveWorkspace))]
    public IHyprlandState Refresh()
    {
        HyprctlMonitor[] hyprctlMonitors = getHyprctlData<HyprctlMonitor[]>("monitors");
        HyprctlWindow[] hyprctlWindows = getHyprctlData<HyprctlWindow[]>("clients");
        HyprctlWorkspace[] hyprctlWorkspaces = getHyprctlData<HyprctlWorkspace[]>("workspaces");

        _monitors = new(hyprctlMonitors.Select(m => new KeyValuePair<int, Monitor>(m.Id, m.Map())));
        _windows = new(hyprctlWindows.Select(w => new KeyValuePair<string, Window>(w.Address, w.Map())));
        _workspaces = new(hyprctlWorkspaces.Select(ws => new KeyValuePair<string, Workspace>(ws.Name, ws.Map())));

        fillReferences(hyprctlMonitors, hyprctlWindows, hyprctlWorkspaces);

        HyprctlWorkspace hyprctlActiveWorkspace = getHyprctlData<HyprctlWorkspace>("activeworkspace");
        HyprctlWindow hyprctlActiveWindow = getHyprctlData<HyprctlWindow>("activewindow");

        ActiveWorkspace = _workspaces[hyprctlActiveWorkspace.Name];

        Window? activeWindow = null;
        if (!string.IsNullOrEmpty(hyprctlActiveWindow.Address))
            _windows.TryGetValue(hyprctlActiveWindow.Address, out activeWindow);
        ActiveWindow = activeWindow;

        return this;
    }
    private T getHyprctlData<T>(string command)
    {
        string socketPath = $"/tmp/hypr/{InstanceSignature}/.socket.sock";
        UnixDomainSocketEndPoint endpoint = new (socketPath);
        using Socket socket = new (AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        socket.Connect(endpoint);

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
    private void fillReferences(HyprctlMonitor[] hyprctlMonitors, HyprctlWindow[] hyprctlWindows, HyprctlWorkspace[] hyprctlWorkspaces)
    {
        foreach(HyprctlWindow window in hyprctlWindows)
        {
            if (_monitors.TryGetValue(window.Monitor, out Monitor? monitor))
                _windows[window.Address].Monitor = monitor;

            if (_workspaces.TryGetValue(window.Workspace.Name, out Workspace? workspace))
                _windows[window.Address].Workspace = workspace;
        }

        foreach(HyprctlMonitor monitor in hyprctlMonitors)
        {
            if (_workspaces.TryGetValue(monitor.ActiveWorkspace.Name, out Workspace? monitorActiveWorkspace))
                _monitors[monitor.Id].ActiveWorkspace = monitorActiveWorkspace;

            if (_workspaces.TryGetValue(monitor.SpecialWorkspace.Name, out Workspace? monitorSpecialWorkspace))
                _monitors[monitor.Id].SpecialWorkspace = monitorSpecialWorkspace;
        }

        foreach(HyprctlWorkspace workspace in hyprctlWorkspaces)
        {
            if (_monitors.TryGetValue(workspace.MonitorId, out Monitor? monitor))
                _workspaces[workspace.Name].Monitor = monitor;

            if (_windows.TryGetValue(workspace.LastWindow, out Window? window))
                _workspaces[workspace.Name].LastWindow = window;
        }
    }

    public Window? GetWindow(string address)
    { 
        // Hyprland sometimes emits signals containing window adresses without the "0x" prefix 
        if (!address.StartsWith("0x"))
            address = "0x" + address;

        return _windows.GetValueOrDefault(address);
    }
    public Workspace? GetWorkspace(string name) => _workspaces.GetValueOrDefault(name);
    public Monitor? GetMonitor(int id) => _monitors.GetValueOrDefault(id);

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