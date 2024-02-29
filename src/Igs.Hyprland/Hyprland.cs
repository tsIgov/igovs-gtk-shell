using Igs.TypedIds;

namespace Igs.Hyprland;

public class Hyprland
{
	private readonly IHyprctlClient _hyprctlClient;

	public Hyprland(IHyprctlClient hyprctlClient, ISignalReceiver signalReceiver)
	{
		_hyprctlClient = hyprctlClient;

		signalReceiver.OnSignalReceived += handleEvents;
		signalReceiver.StartListening();
	}

	/// <summary>
	/// Enters kill mode, where you can kill an app by clicking on it. You can exit it with ESCAPE.
	/// </summary> 
	public bool EnterKillMode() => _hyprctlClient.Execute("kill");

	/// <summary>
	/// Forces a config reload.
	/// </summary>
	public bool Reload() => _hyprctlClient.Execute("reload");

	/// <summary>
	/// Sets the xkb layout for a keyboard.
	/// </summary>
	/// <param name="device">The keyboard</param>
	/// <param name="layout">The xkb layout</param>
	public bool SwitchKeyboardLayout(KeyboardReference device, KeyboardLayoutIndex layout) => _hyprctlClient.Execute($"switchxkblayout {device} {layout}");

	/// <summary>
	/// Sends a notification using the built-in Hyprland notification system.
	/// </summary>
	/// <param name="icon">The notification icon</param>
	/// <param name="message">The notification message</param>
	/// <param name="duration">The notification duration</param>
	public void SendNotification(NotificationIcon? icon, string message, TimeSpan duration) =>
		_hyprctlClient.Execute($"notify {(icon == null ? -1 : (int)icon)} {duration.TotalMilliseconds} 0 {message}");

	/// <summary>
	/// Lists active monitors with their properties.
	/// </summary>
	/// <param name="includeInactive">If true will include inactive monitors as well.</param>
	public Monitor[] GetMonitors(bool includeInactive = false) =>
		_hyprctlClient.Query<Monitor[]>($"monitors {(includeInactive ? "all" : "")}")!;

	/// <summary>
	/// Gets the active workspace.
	/// </summary>
	public Workspace? GetActiveWorkspace() => _hyprctlClient.Query<Workspace>("activeworkspace");

	/// <summary>
	/// Lists all workspaces.
	/// </summary>
	public Workspace[] GetWorkspaces() => _hyprctlClient.Query<Workspace[]>("workspaces")!;

	/// <summary>
	/// Lists all windows.
	/// </summary>
	public Window[] GetWindows() => _hyprctlClient.Query<Window[]>("clients")!;

	/// <summary>
	/// Gets the active window.
	/// </summary>
	public Window? GetActiveWindow() => _hyprctlClient.Query<Window>("activewindow")!;

	/// <summary>
	/// Lists all connected keyboards and mice.
	/// </summary>
	public Devices GetDevices() => _hyprctlClient.Query<Devices>("devices")!;

	/// <summary>
	/// Lists all registered binds.
	/// </summary>
	public Bind[] GetBinds() => _hyprctlClient.Query<Bind[]>("binds")!;


	public event Action<MonitorReference>? OnMonitorAdded;
	public event Action<MonitorReference>? OnMonitorRemoved;
	public event Action<MonitorReference, WorkspaceReference>? OnActiveMonitorChanged;

	public event Action<WorkspaceReference>? OnWorkspaceCreated;
	public event Action<WorkspaceReference>? OnWorkspaceDestroyed;
	public event Action<WorkspaceReference>? OnWorkspaceChanged;
	public event Action<WorkspaceReference, MonitorReference>? OnWorkspaceMoved;
	public event Action<WorkspaceReference, string>? OnWorkspaceRenamed;
	public event Action<WorkspaceReference, MonitorReference>? OnSpecialWorkspaceActivated;

	public event Action<WindowReference, WorkspaceReference, string, string>? OnWindowOpened;
	public event Action<WindowReference>? OnWindowClosed;
	public event Action<WindowReference>? OnActiveWindowChanged;
	public event Action<string, string>? OnActiveWindowChanged_Metadata;
	public event Action<WindowReference, WorkspaceReference>? OnWindowMoved;

	public event Action<WindowReference>? OnWindowTitleChanged;
	public event Action<bool>? OnWindowFullscreenToggled;
	public event Action<WindowReference, bool>? OnWindowFloatingToggled;
	public event Action<WindowReference, bool>? OnWindowMinimizationToggled;
	public event Action<WindowReference>? OnWindowRequestUrgentState;

	public event Action<bool>? OnIgnoreGroupLocksToggled;
	public event Action<bool>? OnLockGroupsToggled;

	public event Action<KeyboardReference, string>? OnKeyboardLayoutChanged;
	public event Action<string>? OnSubmapChanged;

	public event Action<string>? OnLayerSurfaceMapped;
	public event Action<string>? OnLayerSurfaceUnmapped;

	public event Action? OnConfigReloaded;

	private void handleEvents(string eventName, string[] args)
	{
		switch (eventName)
		{
			case "monitoradded": OnMonitorAdded?.Invoke(new MonitorReference(args[0])); break;
			case "monitorremoved": OnMonitorRemoved?.Invoke(new MonitorReference(args[0])); break;
			case "focusedmon": OnActiveMonitorChanged?.Invoke(new MonitorReference(args[0]), new WorkspaceReference(args[1])); break;

			case "createworkspace": OnWorkspaceCreated?.Invoke(new WorkspaceReference(args[0])); break;
			case "destroyworkspace": OnWorkspaceDestroyed?.Invoke(new WorkspaceReference(args[0])); break;
			case "workspace": OnWorkspaceChanged?.Invoke(new WorkspaceReference(args[0])); break;
			case "moveworkspace": OnWorkspaceMoved?.Invoke(new WorkspaceReference(args[0]), new MonitorReference(args[1])); break;
			case "renameworkspace": OnWorkspaceRenamed?.Invoke(new WorkspaceReference(args[0]), args[1]); break;
			case "activespecial": OnSpecialWorkspaceActivated?.Invoke(new WorkspaceReference(args[0]), new MonitorReference(args[1])); break;

			case "openwindow": OnWindowOpened?.Invoke(new WindowReference(args[0]), new WorkspaceReference(args[1]), args[2], args[3]); break;
			case "closewindow": OnWindowClosed?.Invoke(new WindowReference(args[0])); break;
			case "activewindowv2": OnActiveWindowChanged?.Invoke(new WindowReference(args[0])); break;
			case "activewindow": OnActiveWindowChanged_Metadata?.Invoke(args[0], args[1]); break;
			case "movewindow": OnWindowMoved?.Invoke(new WindowReference(args[0]), new WorkspaceReference(args[1])); break;

			case "windowtitle": OnWindowTitleChanged?.Invoke(new WindowReference(args[0])); break;
			case "fullscreen": OnWindowFullscreenToggled?.Invoke(ipcParamToBool(args[0])); break;
			case "changefloatingmode": OnWindowFloatingToggled?.Invoke(new WindowReference(args[0]), ipcParamToBool(args[1])); break;
			case "minimize": OnWindowMinimizationToggled?.Invoke(new WindowReference(args[0]), ipcParamToBool(args[1])); break;
			case "urgent": OnWindowRequestUrgentState?.Invoke(new WindowReference(args[0])); break;

			case "ignoregrouplock": OnIgnoreGroupLocksToggled?.Invoke(ipcParamToBool(args[0])); break;
			case "lockgroups": OnLockGroupsToggled?.Invoke(ipcParamToBool(args[0])); break;

			case "activelayout": OnKeyboardLayoutChanged?.Invoke(new KeyboardReference(args[0]), args[1]); break;
			case "submap": OnSubmapChanged?.Invoke(args[0]); break;

			case "openlayer": OnLayerSurfaceMapped?.Invoke(args[0]); break;
			case "closelayer": OnLayerSurfaceUnmapped?.Invoke(args[0]); break;

			case "configreloaded": OnConfigReloaded?.Invoke(); break;
		}
	}

	private static bool ipcParamToBool(string param) => param == "0";



	public bool ExecuteShellCommand(string command, bool enableRules = false) => _hyprctlClient.Execute(enableRules ? $"dispatch execr {command}" : $"dispatch exec {command}");

	public bool CloseActiveWindow() => _hyprctlClient.Execute("dispatch killactive");
	public bool CloseWindow(WindowQuery query) => _hyprctlClient.Execute($"dispatch closewindow {query}");
	public bool ChangeWorkspace(WorkspacePointer workspace) => _hyprctlClient.Execute($"dispatch workspace {workspace}");
	public bool MoveActiveWindowToWorkspace(WorkspacePointer workspace, bool silent = false) =>
		_hyprctlClient.Execute($"dispatch movetoworkspace{(silent ? "silent" : "")} {workspace}");
	public bool MoveWindowToWorkspace(WindowQuery window, WorkspacePointer workspace, bool silent = false) =>
		_hyprctlClient.Execute($"dispatch movetoworkspace{(silent ? "silent" : "")} {workspace},{window}");
	public bool ToggleFloatingOnActiveWindow() => _hyprctlClient.Execute("dispatch togglefloating active");
	public bool ToggleFloatingOnWindow(WindowQuery query) => _hyprctlClient.Execute($"dispatch togglefloating {query}");
	public bool ToggleFullscreenOnActiveWindow(bool fake = false) => _hyprctlClient.Execute(fake ? "dispatch fakefullscreen" : "dispatch fullscreen 0");
	public bool ToggleMaximizeOnActiveWindow() => _hyprctlClient.Execute("dispatch fullscreen 1");
	public bool SetDpmsStatusOfAllMonitors(bool enabled) => _hyprctlClient.Execute($"dispatch dpms {(enabled ? "on" : "off")}");
	public bool ToggleDpmsStatusOfAllMonitors() => _hyprctlClient.Execute("dispatch dpms toggle");
	public bool PinActiveWindow() => _hyprctlClient.Execute("dispatch pin active");
	public bool PinWindow(WindowQuery query) => _hyprctlClient.Execute($"dispatch pin {query}");
	public bool MoveFocus(Direction direction) => _hyprctlClient.Execute($"dispatch movefocus {direction}");
	public bool MoveActiveWindow(Direction direction) => _hyprctlClient.Execute($"dispatch movewindow {direction}");
	public bool MoveActiveWindow(MonitorPointer monitor) => _hyprctlClient.Execute($"dispatch movewindow mon:{monitor}");
	public bool MoveActiveWindow(ResizeParameter position) => _hyprctlClient.Execute($"dispatch moveactive {position}");
	public bool SwapActiveWindow(Direction direction) => _hyprctlClient.Execute($"dispatch swapwindow {direction}");
	public bool CenterWindow(bool respectReservedArea = false) => _hyprctlClient.Execute($"dispatch centerwindow{(respectReservedArea ? " 1" : "")}");
	public bool ResizeActiveWindow(ResizeParameter dimentions) => _hyprctlClient.Execute($"dispatch resizeactive {dimentions}");

	public bool MoveWindow(ResizeParameter position, WindowQuery window) => _hyprctlClient.Execute($"dispatch movewindowpixel {position},{window}");
	public bool ResizeWindow(ResizeParameter dimensions, WindowQuery window) => _hyprctlClient.Execute($"dispatch resizewindowpixel {dimensions},{window}");

	public bool CycleFocus(bool backwards = false) => _hyprctlClient.Execute($"dispatch cyclenext{(backwards ? " prev" : "")}");
	public bool CycleFocusTiled(bool backwards = false) => _hyprctlClient.Execute($"dispatch cyclenext{(backwards ? " prev" : "")} tiled");
	public bool CycleFocusFloation(bool backwards = false) => _hyprctlClient.Execute($"dispatch cyclenext{(backwards ? " prev" : "")} floating");

	public bool SwapActiveWindowWithNext() => _hyprctlClient.Execute("dispatch swapnext");
	public bool SwapActiveWindowWithPrevious() => _hyprctlClient.Execute("dispatch swapnext prev");

	public bool FocusWindow(WindowQuery window) => _hyprctlClient.Execute($"dispatch focuswindow {window}");
	public bool FocusMonitor(MonitorPointer monitor) => _hyprctlClient.Execute($"dispatch focusmonitor {monitor}");

	public bool SetSplitRatio(float ratio, bool relative = false) =>
		_hyprctlClient.Execute($"dispatch splitratio {(relative ? ratio.ToString("+0;-#") : $"exact {ratio}")}");

	public bool ToggleActiveWindowOpacity() => _hyprctlClient.Execute("dispatch toggleopaque");

	public bool MoveCursorToBottomLeftCornerOfActiveWindow() => _hyprctlClient.Execute("dispatch movecursortocorner 0");
	public bool MoveCursorToBottomRightCornerOfActiveWindow() => _hyprctlClient.Execute("dispatch movecursortocorner 1");
	public bool MoveCursorToTopRightCornerOfActiveWindow() => _hyprctlClient.Execute("dispatch movecursortocorner 2");
	public bool MoveCursorToTopLeftCornerOfActiveWindow() => _hyprctlClient.Execute("dispatch movecursortocorner 3");
	public bool MoveCursor(int x, int y) => _hyprctlClient.Execute($"dispatch movecursor {x} {y}");

	public bool RenameWorkspace(int workspaceId, string name) => _hyprctlClient.Execute($"dispatch renameworkspace {workspaceId} {name}");

	public bool ExitCompositor() => _hyprctlClient.Execute("dispatch exit");
	public bool ForceRendererReload() => _hyprctlClient.Execute("dispatch forcerendererreload");
	public bool MoveActiveWorkspaceToMonitor(MonitorPointer monitor) => _hyprctlClient.Execute($"dispatch movecurrentworkspacetomonitor {monitor}");
	public bool FocusWorkspaceOnActiveMonitor(WorkspacePointer workspace) => _hyprctlClient.Execute($"dispatch focusworkspaceoncurrentmonitor {workspace}");
	public bool MoveWorkspaceToMonitor(WorkspacePointer workspace, MonitorPointer monitor) => _hyprctlClient.Execute($"dispatch moveworkspacetomonitor {workspace} {monitor}");

	public bool SwapActiveWorkspaces(WorkspacePointer workspace1, WorkspacePointer workspace2) => _hyprctlClient.Execute($"dispatch swapactiveworkspaces {workspace1} {workspace2}");

	public bool ForceActiveWindowToTop() => _hyprctlClient.Execute("dispatch alterzorder top");
	public bool ForceActiveWindowToBottom() => _hyprctlClient.Execute("dispatch alterzorder bottom");
	public bool ForceWindowToTop(WindowQuery window) => _hyprctlClient.Execute($"dispatch alterzorder top,{window}");
	public bool ForceWindowToBottom(WindowQuery window) => _hyprctlClient.Execute($"dispatch alterzorder bottom,{window}");

	public bool ToggleSpecialWorkspace(string specialWorkspaceName) => _hyprctlClient.Execute($"dispatch togglespecialworkspace {specialWorkspaceName}");
	public bool ToggleFirstSpecialWorkspace() => _hyprctlClient.Execute("dispatch togglespecialworkspace");

	public bool FocusUrgentOrLastWindow() => _hyprctlClient.Execute("dispatch focusurgentorlast");

	public bool ToggleActiveWindowIntoAGroup() => _hyprctlClient.Execute("dispatch togglegroup");


	public bool CycleWindowInActiveGroup(bool backwards = false) => _hyprctlClient.Execute($"dispatch changegroupactive {(backwards ? "b" : "f")}");
	public bool SwitchToWindowInActiveGroup(int index) => _hyprctlClient.Execute($"dispatch changegroupactive {index}");

	public bool FocusLastWindow() => _hyprctlClient.Execute("dispatch focuscurrentorlast");

	public bool LockGroups() => _hyprctlClient.Execute("dispatch lockgroups lock");
	public bool UnlockGroups() => _hyprctlClient.Execute("dispatch lockgroups unlock");
	public bool ToggleGroupLocking() => _hyprctlClient.Execute("dispatch lockgroups toggle");

	public bool LockActiveGroup() => _hyprctlClient.Execute("dispatch lockactivegroup lock");
	public bool UnlockActiveGroup() => _hyprctlClient.Execute("dispatch lockactivegroup unlock");
	public bool ToggleActiveGroupLocking() => _hyprctlClient.Execute("dispatch lockactivegroup toggle");

	public bool MoveActiveWindowIntoAGroup(Direction direction) => _hyprctlClient.Execute($"dispatch moveintogroup {direction}");
	public bool MoveActiveWindowOutOfAGroup() => _hyprctlClient.Execute("dispatch moveoutofgroup");
	public bool MoveActiveWindowPreferGrouping(Direction direction) => _hyprctlClient.Execute($"dispatch movewindoworgroup {direction}");

	public bool SwapActiveWindowWithNextGroupedSibling(bool backwards = false) => _hyprctlClient.Execute($"dispatch movegroupwindow {(backwards ? "b" : "f")}");

	public bool DenyActiveWindowFromGrouping() => _hyprctlClient.Execute("dispatch denywindowfromgroup on");
	public bool AllowActiveWindowGrouping() => _hyprctlClient.Execute("dispatch denywindowfromgroup off");
	public bool ToggleActiveWindowGroupingProhibition() => _hyprctlClient.Execute("dispatch denywindowfromgroup toggle");

	public bool IgnoreGroupLocking() => _hyprctlClient.Execute("dispatch setignoregrouplock on");
	public bool RestoreGroupLocking() => _hyprctlClient.Execute("dispatch setignoregrouplock off");
	public bool ToggleGroupLockingIgnore() => _hyprctlClient.Execute("dispatch setignoregrouplock toggle");

	public bool ExecuteGlobalShortcut(string name) => _hyprctlClient.Execute($"dispatch global {name}");
	public bool ChangeSubmap(string? submap) => _hyprctlClient.Execute($"dispatch submap {submap ?? "reset"}");












}

public class ResizeParameter
{
	private string _value;

	public ResizeParameter(int width, int height, bool relative = false) : this(convertValue(width), convertValue(height), relative) { }
	public ResizeParameter(int width, float height, bool relative = false) : this(convertValue(width), convertValue(height), relative) { }
	public ResizeParameter(float width, int height, bool relative = false) : this(convertValue(width), convertValue(height), relative) { }
	public ResizeParameter(float width, float height, bool relative = false) : this(convertValue(width), convertValue(height), relative) { }
	private ResizeParameter(string width, string height, bool relative)
	{
		_value = $"{(relative ? "" : "exact ")}{width} {height}";
	}
	private static string convertValue(int value) => value.ToString();
	private static string convertValue(float value) => $"{(int)Math.Round(value * 100)}%";

	public override string ToString() => _value;
}

public class Direction
{
	public static Direction Left = new("l");
	public static Direction Right = new("r");
	public static Direction Up = new("u");
	public static Direction Down = new("d");

	private string _value;

	private Direction(string value)
	{
		_value = value;
	}

	public override string ToString() => _value;
}

public class WindowQuery
{
	public static WindowQuery ByClass(string classRegex) => new(classRegex);
	public static WindowQuery ByTitle(string titleRegex) => new($"title: {titleRegex}");
	public static WindowQuery ByPid(int pid) => new($"pid: {pid}");
	public static WindowQuery Exact(WindowReference reference) => new($"address: {reference}");
	public static WindowQuery Floating() => new("floating");
	public static WindowQuery Tiled() => new("tiled");

	private string _value;

	private WindowQuery(string value)
	{
		_value = value;
	}

	public override string ToString() => _value;
}

public class WorkspacePointer
{
	public static WorkspacePointer FromId(int id) => new(id.ToString());
	public static WorkspacePointer RelativeToActive(int offset, bool onlyOpened = false)
	{
		string symbol = onlyOpened ? "e" : "";
		return new WorkspacePointer($"{symbol}{offset:+0;-#}");
	}
	public static WorkspacePointer RelativeOnMonitor(int offset, bool includeEmpty = false)
	{
		string symbol = includeEmpty ? "r" : "m";
		return new WorkspacePointer($"{symbol}{offset:+0;-#}");
	}
	public static WorkspacePointer FromName(string name) => new($"name:{name}");
	public static WorkspacePointer FirstEmpty() => new("empty");
	public static WorkspacePointer Special(string name) => new($"special:{name}");

	private string _value;

	private WorkspacePointer(string value)
	{
		_value = value;
	}

	public override string ToString() => _value;
}

public class MonitorPointer
{
	public static MonitorPointer ById(int id) => new(id.ToString());
	public static MonitorPointer ByName(string name) => new(name);
	public static MonitorPointer RelativeToActive(int offset) => new(offset.ToString("+0;-#"));
	public static MonitorPointer InDirectionOfActive(Direction direction) => new(direction.ToString());
	public static MonitorPointer Current() => new("current");

	private string _value;

	private MonitorPointer(string value)
	{
		_value = value;
	}

	public override string ToString() => _value;
}




public enum NotificationIcon
{
	Warning = 0,
	Info = 1,
	Hint = 2,
	Error = 3,
	Confused = 4,
	OK = 5
}


public record MonitorReference(string Value) : TypedId(Value);
public record Monitor(MonitorReference Id, MonitorReference Name, string Description, string Make, string Model, string Serial, int Width, int Height, float RefreshRate, int X, int Y, HyprctlWorkspaceReference ActiveWorkspace, HyprctlWorkspaceReference SpecialWorkspace, int[] Reserved, float Scale, int Transform, bool Focused, bool DpmsStatus, bool Vrr, bool ActivelyTearing);

public record WorkspaceReference(string Value) : TypedId(Value);
public record HyprctlWorkspaceReference(WorkspaceReference Id, WorkspaceReference Name);
public record Workspace(WorkspaceReference Id, WorkspaceReference Name, MonitorReference? Monitor, MonitorReference? MonitorId, int Windows, bool HasFullScreen, WindowReference LastWindow, string LastWindowTitle);

public record Window(WindowReference Address, bool Mapped, bool Hidden, int[] At, int[] Size, HyprctlWorkspaceReference Workspace, bool Floating, WindowReference? Monitor, string Class, string Title, string InitialClass, string InitialTitle, int Pid, bool XWayland, bool Pinned, bool Fullscreen, int FullscreenMode, bool FakeFullscreen, WindowReference[] Grouped, WindowReference Swallowing, int FocusHistoryId);
public record WindowReference(string Value) : TypedId(Value);

public record Devices(Mouse[] Mice, Keyboard[] Keyboards);

public record KeyboardReference(string Value) : TypedId(Value);
public record Keyboard(KeyboardReference Address, string Name, string Rules, string Model, string Layout, string Variant, string Options, string Active_Keymap, bool Main);

public record MouseReference(string Value) : TypedId(Value);
public record Mouse(MouseReference Address, string Name, float DefaultSpeed);

public record KeyboardLayoutIndex
{
	public KeyboardLayoutIndex Next { get; } = new KeyboardLayoutIndex("next");
	public KeyboardLayoutIndex Previous { get; } = new KeyboardLayoutIndex("prev");

	private string _value;

	public KeyboardLayoutIndex(int index)
	{
		_value = index.ToString();
	}

	private KeyboardLayoutIndex(string value)
	{
		_value = value;
	}

	public override string ToString() => _value?.ToString()!;

}
public record SubmapReference(string Value) : TypedId(Value);
public record Bind(bool Locked, bool Mouse, bool Release, bool Repeat, bool Non_Consuming, int ModMask, SubmapReference Submap, string Key, int Keycode, string Dispatcher, string arg);
