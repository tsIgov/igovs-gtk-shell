
using Igs.Hyprland;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

Gtk.Application.Init ();

// Gtk.CssProvider provider = new Gtk.CssProvider();
// provider.LoadFromData("* { all: unset; }");

// Gtk.CssProvider provider2 = new Gtk.CssProvider();
// provider2.LoadFromData("* { background-color: @theme_selected_bg_color;}");


// Gtk.StyleContext.AddProviderForScreen(Gdk.Screen.Default, provider, 0);
// Gtk.StyleContext.AddProviderForScreen(Gdk.Screen.Default, provider2, 0);

ServiceCollection services = new();
services
	.AddHyprland()
	.AddLogging(logging => logging.AddConsole());

ServiceProvider serviceProvider = services.BuildServiceProvider();

using IServiceScope scope = serviceProvider.CreateScope();

IHyprland hyprland = scope.ServiceProvider.GetRequiredService<IHyprland>();
hyprland.Initialize();

for (int i = 0; i < Gdk.Display.Default.NMonitors; i++)
{
	Gdk.Monitor monitor = Gdk.Display.Default.GetMonitor(i);
	showAllPerMonitor(monitor);
}

Gtk.Application.Run();


void showAllPerMonitor(Gdk.Monitor monitor)
{
	foreach (Gtk.Widget widget in registerPerMonitor(monitor))
		widget.ShowAll();
}




IEnumerable<Gtk.Widget> registerPerMonitor(Gdk.Monitor monitor)
{
	yield return new Statusbar(monitor);
}