namespace Igs.Services.Hyprland.Events;

public delegate void LayerEventHandler(LayerEventArgs args);

public record LayerEventArgs(string Namespace);

public class LayerEvents
{
    /// <summary>
    /// Emitted when a layerSurface is mapped.
    /// </summary>
    public event LayerEventHandler? OnLayerOpened;
    /// <summary>
    /// Emitted when a layerSurface is unmapped.
    /// </summary>
    public event LayerEventHandler? OnLayerClosed;

    internal LayerEvents() {}
}