
using Igs.Services.Hyprland;

Gtk.Application.Init ();

// Gtk.CssProvider provider = new Gtk.CssProvider();
// provider.LoadFromData("* { all: unset; }");

// Gtk.CssProvider provider2 = new Gtk.CssProvider();
// provider2.LoadFromData("* { background-color: @theme_selected_bg_color;}");


// Gtk.StyleContext.AddProviderForScreen(Gdk.Screen.Default, provider, 0);
// Gtk.StyleContext.AddProviderForScreen(Gdk.Screen.Default, provider2, 0);


// for (int i = 0; i < Gdk.Display.Default.NMonitors; i++)
// {
//     Gdk.Monitor monitor = Gdk.Display.Default.GetMonitor(i);
//     Statusbar sb = new(monitor);
//     sb.ShowAll();
// }

HyprlandService hyprlandService = new();


Gtk.Application.Run();


