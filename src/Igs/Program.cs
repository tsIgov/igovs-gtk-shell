
using System.Text.Json;
using Igs.Hyprland;
using Igs.TypedIds;
using Microsoft.Extensions.DependencyInjection;

Gtk.Application.Init ();

// Gtk.CssProvider provider = new Gtk.CssProvider();
// provider.LoadFromData("* { all: unset; }");

// Gtk.CssProvider provider2 = new Gtk.CssProvider();
// provider2.LoadFromData("* { background-color: @theme_selected_bg_color;}");


// Gtk.StyleContext.AddProviderForScreen(Gdk.Screen.Default, provider, 0);
// Gtk.StyleContext.AddProviderForScreen(Gdk.Screen.Default, provider2, 0);

ServiceCollection services = new();
services
	.AddSingleton<ISignatureProvider, SignatureProvider>()
	.AddSingleton<IHyprctlClient, HyprctlClient>()
	.AddSingleton<ISignalReceiver, SignalReceiver>()
	.AddSingleton<Hyprland>()
	.AddSingleton<JsonSerializerOptions>(services =>
	{
		JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
		options.Converters.Add(new TypedIdJsonConverterFactory());
		return options;
	});








ServiceProvider serviceProvider = services.BuildServiceProvider();

using IServiceScope scope = serviceProvider.CreateScope();

Hyprland hyprland = scope.ServiceProvider.GetRequiredService<Hyprland>();


Gtk.Application.Run();




// for (int i = 0; i < Gdk.Display.Default.NMonitors; i++)
// {
// 	Gdk.Monitor monitor = Gdk.Display.Default.GetMonitor(i);
// 	showAllPerMonitor(monitor);
// }


// void showAllPerMonitor(Gdk.Monitor monitor)
// {
// 	foreach (Gtk.Widget widget in registerPerMonitor(monitor))
// 		widget.ShowAll();
// }




// IEnumerable<Gtk.Widget> registerPerMonitor(Gdk.Monitor monitor)
// {
// 	yield return new Statusbar(monitor);
// }