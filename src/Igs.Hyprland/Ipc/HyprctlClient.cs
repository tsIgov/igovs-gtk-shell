using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Igs.Hyprland.Ipc;

public interface IHyprctlClient
{
	T Query<T>(string query);
	bool Dispatch(string command);
}

public class HyprctlClient : IHyprctlClient
{
	private readonly string _hyprlandInstanceSignature;
	private readonly ILogger<HyprctlClient>? _logger;

	public HyprctlClient(ISignatureProvider signatureProvider, ILoggerFactory? loggerFactory = null)
	{
		_hyprlandInstanceSignature = signatureProvider.GetSignature();
		_logger = loggerFactory?.CreateLogger<HyprctlClient>();
	}

	public T Query<T>(string query)
	{
		string rawResponse = sendMessage(query);

		T response = JsonSerializer.Deserialize<T>(rawResponse, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
		return response;
	}

	public bool Dispatch(string command)
	{
		string response = sendMessage($"dispatch {command}");
		if (response != "ok")
		{
			_logger?.LogError("Dispatching a Hyprland command \"{command}\" failed with the following error: \"{error}\"", command, response);
			return false;
		}

		return true;
	}

	private string sendMessage(string message)
	{
		string socketPath = $"/tmp/hypr/{_hyprlandInstanceSignature}/.socket.sock";
		UnixDomainSocketEndPoint endpoint = new(socketPath);
		using Socket socket = new(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
		socket.Connect(endpoint);

		using NetworkStream stream = new(socket);
		using StreamWriter writer = new(stream);
		using StreamReader reader = new(stream);

		writer.Write($"j/{message}");
		writer.Flush();
		string response = reader.ReadToEnd();
		socket.Close();

		return response;
	}
}

