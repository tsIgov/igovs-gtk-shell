using GLib;

namespace Igs.Services.Hyprland.Events;

public delegate void KeyboardLayoutEventHandler(KeyboardLayoutEventArgs args);
public delegate void KeybindSubmapEventHandler(KeybindSubmapEventArgs args);

public record KeyboardLayoutEventArgs(string KeyboardName, string LayoutName);
public record KeybindSubmapEventArgs(string SubmapName);

public class KeyboardEvents
{
    /// <summary>
    /// Emitted on a layout change of the active keyboard.
    /// </summary>
    public event KeyboardLayoutEventHandler? OnKeyboardLayoutChanged;
    /// <summary>
    /// Emitted when a keybind submap changes. Empty means default.
    /// </summary>
    public event Action<KeybindSubmapEventArgs>? OnKeybindSubmapChanged;

    internal KeyboardEvents() {}
}