using System.Diagnostics;

namespace Igs.Hyprland.Ipc;

public interface ISignatureProvider
{
	string GetSignature();
}

public class SignatureProvider : ISignatureProvider
{
	private string _signature;

	public SignatureProvider()
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

		Process process = new() { StartInfo = psi };
		process.Start();

		string output = process.StandardOutput.ReadToEnd().Trim();
		process.WaitForExit();

		if (string.IsNullOrWhiteSpace(output))
			throw new InvalidOperationException("Hyprland session not found");

		_signature = output;
	}

	public string GetSignature() => _signature;
}