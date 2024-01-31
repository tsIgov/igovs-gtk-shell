using Igs.Hyprland.Ipc;

namespace Igs.Hyprland.Input;

public delegate void LayoutChangeEventHandler(string keyboardName, string keyboardLayout);
public delegate void SubmapChangeEventHandler(string submap);

public class Keyboard
{
	/// <summary>
	/// Emitted on a layout change of the active keyboard.
	/// </summary>
	public event LayoutChangeEventHandler? OnLayoutChanged;
	/// <summary>
	/// Emitted when a keybind submap changes. Empty means default.
	/// </summary>
	public event SubmapChangeEventHandler? OnSubmapChanged;

	internal Keyboard(ISignalReceiver signalReciever)
	{
		signalReciever.OnSignalReceived += handleEvents;
	}

	private void handleEvents(string eventName, string[] args)
	{
		switch (eventName)
		{
			case "submap": OnSubmapChanged?.Invoke(args[0]); break;
			case "activelayout": OnLayoutChanged?.Invoke(args[0], args[1]); break;
		}
	}

}