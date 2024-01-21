using Igs.Hyprland.Ipc;
using Microsoft.Extensions.DependencyInjection;

namespace Igs.Hyprland;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddHyprland(this IServiceCollection services)
	{
		services.AddSingleton<ISignatureProvider, SignatureProvider>();
		services.AddSingleton<IHyprctlClient, HyprctlClient>();
		services.AddSingleton<ISignalReciever, SignalReciever>();
		services.AddSingleton<IHyprland, Hyprland>();
		return services;
	}
}