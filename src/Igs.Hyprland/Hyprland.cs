namespace Igs.Hyprland;

public class Hyprland
{
	private readonly IHyprctlClient _hyprctlClient;

	public Hyprland(IHyprctlClient hyprctlClient)
	{
		_hyprctlClient = hyprctlClient;
	}

	/// <summary>
	/// Enters kill mode, where you can kill an app by clicking on it. You can exit it with ESCAPE.
	/// </summary> 
	public void EnterKillMode() => _hyprctlClient.Dispatch("kill");

	/// <summary>
	/// Forces a config reload.
	/// </summary>
	public void Reload() => _hyprctlClient.Dispatch("reload");

	/// <summary>
	/// Sets the cursor theme and reloads the cursor manager. Will set the theme for everything except GTK.
	/// </summary>
	/// <param name="theme">The cursor theme</param>
	/// <param name="size">The cursor size</param>
	public void SetCursor(string theme, int size) => _hyprctlClient.Dispatch($"setcursor {theme} {size}");

	/// <summary>
	/// Sets the xkb layout index for a keyboard.
	/// </summary>
	/// <param name="device">The keyboard</param>
	/// <param name="layoutIndex">The xkb layout index</param>
	public void SwitchKeyboardLayout(DeviceName device, int layoutIndex) => _hyprctlClient.Dispatch($"switchxkblayout {device.Name} {layoutIndex}");

	/// <summary>
	/// Switches to the next xkb layout index for a keyboard.
	/// </summary>
	/// <param name="device">The keyboard</param>
	public void SwitchKeyboardLayoutNext(DeviceName device) => _hyprctlClient.Dispatch($"switchxkblayout {device.Name} next");

	/// <summary>
	/// Switches to the previous xkb layout index for a keyboard.
	/// </summary>
	/// <param name="device">The keyboard</param>
	public void SwitchKeyboardLayoutPrev(DeviceName device) => _hyprctlClient.Dispatch($"switchxkblayout {device.Name} prev");

	/// <summary>
	/// Sends a notification using the built-in Hyprland notification system.
	/// </summary>
	/// <param name="icon">The notification icon</param>
	/// <param name="message">The notification message</param>
	/// <param name="duration">The notification duration</param>
	public void SendNotification(NotificationIcon? icon, string message, TimeSpan duration) =>
		_hyprctlClient.Dispatch($"notify {(icon == null ? -1 : (int)icon)} {duration.TotalMilliseconds} 0 {message}");
}


public record DeviceName(string Name);

public enum NotificationIcon
{
	Warning = 0,
	Info = 1,
	Hint = 2,
	Error = 3,
	Confused = 4,
	OK = 5
}