using System;
using System.Runtime.InteropServices;

namespace Gtk 
{
	public static class LayerShell 
	{
		[DllImport("gtk-layer-shell")] private static extern uint gtk_layer_get_major_version();
		public static uint MajorVersion { get => gtk_layer_get_major_version(); }

		[DllImport("gtk-layer-shell")] private static extern uint gtk_layer_get_minor_version();
		public static uint MinorVersion { get => gtk_layer_get_minor_version(); }

		[DllImport("gtk-layer-shell")] private static extern uint gtk_layer_get_micro_version();
		public static uint MicroVersion { get => gtk_layer_get_micro_version(); }

		[DllImport("gtk-layer-shell")] private static extern bool gtk_layer_is_supported();
		public static bool IsSupported { get => gtk_layer_is_supported(); }

		[DllImport("gtk-layer-shell")] private static extern uint gtk_layer_get_protocol_version();
		public static uint ProtocolVersion { get => gtk_layer_get_protocol_version(); }

		[DllImport("gtk-layer-shell")] private static extern void gtk_layer_init_for_window(IntPtr window);
		public static void InitWindow(Window window) => gtk_layer_init_for_window(window.Handle);

		[DllImport("gtk-layer-shell")] private static extern bool gtk_layer_is_layer_window(IntPtr window);
		public static bool IsLayerWindow(Window window) => gtk_layer_is_layer_window(window.Handle);

		// [DllImport("gtk-layer-shell")] private static extern struct zwlr_layer_surface_v1* gtk_layer_get_zwlr_layer_surface_v1 (IntPtr window);


		[DllImport("gtk-layer-shell")] private static extern void gtk_layer_set_namespace(IntPtr window, IntPtr name_space);
		public static void SetNamespace(Window window, string @namespace) => gtk_layer_set_namespace(window.Handle, Marshal.StringToCoTaskMemUTF8(@namespace));

		[DllImport("gtk-layer-shell")] private static extern IntPtr gtk_layer_get_namespace (IntPtr window);
		public static string? GetNamespace(Window window) => Marshal.PtrToStringUTF8(gtk_layer_get_namespace(window.Handle));

		[DllImport("gtk-layer-shell")] private static extern void gtk_layer_set_layer(IntPtr window, LayerShell.Layer layer);
		public static void SetLayer(Window window, LayerShell.Layer layer) => gtk_layer_set_layer(window.Handle, layer);

		[DllImport("gtk-layer-shell")] private static extern LayerShell.Layer gtk_layer_get_layer(IntPtr window);
		public static LayerShell.Layer GetLayer(Window window) => gtk_layer_get_layer(window.Handle);

		[DllImport("gtk-layer-shell")] private static extern void gtk_layer_set_monitor(IntPtr window, IntPtr monitor);
		public static void SetMonitor(Window window, Gdk.Monitor monitor) => gtk_layer_set_monitor(window.Handle, monitor.Handle);

		[DllImport("gtk-layer-shell")] private static extern IntPtr gtk_layer_get_monitor(IntPtr window);
		public static Gdk.Monitor GetMonitor(Window window) => new Gdk.Monitor(gtk_layer_get_monitor(window.Handle));

		[DllImport("gtk-layer-shell")] private static extern void gtk_layer_set_anchor(IntPtr window, LayerShell.Edge edge, bool anchor_to_edge);
		public static void SetAnchor(Window window, LayerShell.Edge edge, bool anchorToEdge) => gtk_layer_set_anchor(window.Handle, edge, anchorToEdge);

		[DllImport("gtk-layer-shell")] private static extern bool gtk_layer_get_anchor(IntPtr window, LayerShell.Edge edge);
		public static bool GetAnchor(Window window, LayerShell.Edge edge) => gtk_layer_get_anchor(window.Handle, edge);

		[DllImport("gtk-layer-shell")] private static extern void gtk_layer_set_margin(IntPtr window, LayerShell.Edge edge, int margin_size);
		public static void SetMargin(Window window, LayerShell.Edge edge, int marginSize) => gtk_layer_set_margin(window.Handle, edge, marginSize);

		[DllImport("gtk-layer-shell")] private static extern int gtk_layer_get_margin(IntPtr window, LayerShell.Edge edge);
		public static int GetMargin(Window window, LayerShell.Edge edge) => gtk_layer_get_margin(window.Handle, edge);

		[DllImport("gtk-layer-shell")] private static extern void gtk_layer_set_exclusive_zone(IntPtr window, int exclusive_zone);
		public static void SetExclusiveZone(Window window, int zone) => gtk_layer_set_exclusive_zone(window.Handle, zone);

		[DllImport("gtk-layer-shell")] private static extern int gtk_layer_get_exclusive_zone(IntPtr window);
		public static int GetExclusiveZone(Window window) => gtk_layer_get_exclusive_zone(window.Handle);

		[DllImport("gtk-layer-shell")] private static extern void gtk_layer_auto_exclusive_zone_enable(IntPtr window);
		public static void AutoExclusiveZoneEnable(Window window) => gtk_layer_auto_exclusive_zone_enable(window.Handle);

		[DllImport("gtk-layer-shell")] private static extern bool gtk_layer_auto_exclusive_zone_is_enabled(IntPtr window);
		public static bool AutoExclusiveZoneIsEnabled(Window window) => gtk_layer_auto_exclusive_zone_is_enabled(window.Handle);

		[DllImport("gtk-layer-shell")] private static extern void gtk_layer_set_keyboard_mode(IntPtr window, LayerShell.KeyboardMode mode);
		public static void SetKeyboardMode(Window window, LayerShell.KeyboardMode mode) => gtk_layer_set_keyboard_mode(window.Handle, mode);

		[DllImport("gtk-layer-shell")] private static extern LayerShell.KeyboardMode gtk_layer_get_keyboard_mode(IntPtr window);
		public static LayerShell.KeyboardMode GetKeyboardMode(Window window) => gtk_layer_get_keyboard_mode(window.Handle);

		[DllImport("gtk-layer-shell")] private static extern void gtk_layer_set_keyboard_interactivity(IntPtr window, bool interactivity);
		public static void SetKeyboardInteractivity(Window window, bool interactivity) => gtk_layer_set_keyboard_interactivity(window.Handle, interactivity);

		[DllImport("gtk-layer-shell")] private static extern bool gtk_layer_get_keyboard_interactivity(IntPtr window);
		public static bool GetKeyboardInteractivity(Window window) => gtk_layer_get_keyboard_interactivity(window.Handle);

		public enum Layer 
		{
			Background,
			Bottom,
			Top,
			Overlay,
			EntryNumber,
		}

		public enum Edge 
		{
			Left = 0,
			Right,
			Top,
			Bottom,
			EntryNumber,
		}

		public enum KeyboardMode 
		{
			None = 0,
			Exclusive = 1,
			OnDemand = 2,
			EntryNumber = 3,
		}
	}
}