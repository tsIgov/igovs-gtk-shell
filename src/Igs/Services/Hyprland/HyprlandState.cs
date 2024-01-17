using System.Diagnostics.CodeAnalysis;

namespace Igs.Services.Hyprland;

public class HyprlandState : IHyprlandState
{
    private Dictionary<int, Monitor> _monitors;
    private Dictionary<string, Window> _windows;
    private Dictionary<string, Workspace> _workspaces;

    public IEnumerable<Monitor> Monitors => _monitors.Values;
    public IEnumerable<Workspace> Workspaces => _workspaces.Values;
    public IEnumerable<Window> Windows => _windows.Values;

    public Workspace ActiveWorkspace { get; private set; }
    public Window? ActiveWindow { get; private set; }

    internal HyprlandState()
    {
        Refresh();
    }

    [MemberNotNull(nameof(_monitors), nameof(_windows), nameof(_workspaces), nameof(ActiveWorkspace))]
    public IHyprlandState Refresh()
    {
        Monitor.Hyprctl[] hyprctlMonitors = Hyprctl.Query<Monitor.Hyprctl[]>("monitors");
        Window.Hyprctl[] hyprctlWindows = Hyprctl.Query<Window.Hyprctl[]>("clients");
        Workspace.Hyprctl[] hyprctlWorkspaces = Hyprctl.Query<Workspace.Hyprctl[]>("workspaces");

        _monitors = new(hyprctlMonitors.Select(m => new KeyValuePair<int, Monitor>(m.Id, new Monitor(m))));
        _windows = new(hyprctlWindows.Select(w => new KeyValuePair<string, Window>(w.Address, new Window(w))));
        _workspaces = new(hyprctlWorkspaces.Select(ws => new KeyValuePair<string, Workspace>(ws.Name, new Workspace(ws))));

        foreach (var ws in _workspaces)
            Console.WriteLine($"{ws.Value.Id} | {ws.Value.Name}");

        fillReferences(hyprctlMonitors, hyprctlWindows, hyprctlWorkspaces);

        Workspace.Hyprctl hyprctlActiveWorkspace = Hyprctl.Query<Workspace.Hyprctl>("activeworkspace");
        Window.Hyprctl hyprctlActiveWindow = Hyprctl.Query<Window.Hyprctl>("activewindow");

        ActiveWorkspace = _workspaces[hyprctlActiveWorkspace.Name];

        Window? activeWindow = null;
        if (!string.IsNullOrEmpty(hyprctlActiveWindow.Address))
            _windows.TryGetValue(hyprctlActiveWindow.Address, out activeWindow);
        ActiveWindow = activeWindow;

        return this;
    }

    private void fillReferences(Monitor.Hyprctl[] hyprctlMonitors, Window.Hyprctl[] hyprctlWindows, Workspace.Hyprctl[] hyprctlWorkspaces)
    {
        foreach (Window.Hyprctl window in hyprctlWindows)
        {
            if (_monitors.TryGetValue(window.Monitor, out Monitor? monitor))
                _windows[window.Address].Monitor = monitor;

            if (_workspaces.TryGetValue(window.Workspace.Name, out Workspace? workspace))
                _windows[window.Address].Workspace = workspace;
        }

        foreach (Monitor.Hyprctl monitor in hyprctlMonitors)
        {
            if (_workspaces.TryGetValue(monitor.ActiveWorkspace.Name, out Workspace? monitorActiveWorkspace))
                _monitors[monitor.Id].ActiveWorkspace = monitorActiveWorkspace;

            if (_workspaces.TryGetValue(monitor.SpecialWorkspace.Name, out Workspace? monitorSpecialWorkspace))
                _monitors[monitor.Id].SpecialWorkspace = monitorSpecialWorkspace;
        }

        foreach (Workspace.Hyprctl workspace in hyprctlWorkspaces)
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
}