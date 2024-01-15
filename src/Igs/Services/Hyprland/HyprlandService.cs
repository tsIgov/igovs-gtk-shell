using System.Diagnostics;
using Igs.Services.Hyprland.Events;

namespace Igs.Services.Hyprland;

public class HyprlandService : IDisposable
{
    private SignalListener _signalListener;
    private Hyprctl _hyprctl;

    public string HyprlandInstanceSignature { get; }

    public KeyboardEvents KeyboardEvents { get; } = new();
    public LayerEvents LayerEvents { get; } = new();
    public MonitorEvents MonitorEvents { get; } = new();
    public WindowEvents WindowEvents { get; } = new();
    public WorkspaceEvents WorkspaceEvents { get; } = new();

    public HyprlandService()
    {
        HyprlandInstanceSignature = getHyprlandInstanceSignature();

        if (string.IsNullOrWhiteSpace(HyprlandInstanceSignature))
            throw new InvalidOperationException("Hyprland session not found");

        _signalListener = new SignalListener(this);
        _hyprctl = new Hyprctl(HyprlandInstanceSignature);
    }

    private string getHyprlandInstanceSignature()
    {
        ProcessStartInfo psi = new("bash")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = "-c \"echo $HYPRLAND_INSTANCE_SIGNATURE\""
        };

        Process process = new () { StartInfo = psi };
        process.Start();

        string output = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();

        return output;
    }

    public State GetState() => _hyprctl.GetState();

    public void Dispose()
    {
        _signalListener?.Dispose();
    }

}