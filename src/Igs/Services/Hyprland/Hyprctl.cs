using System.Net.Sockets;
using System.Text.Json;

namespace Igs.Services.Hyprland;

internal static class Hyprctl
{
	internal static T Query<T>(string command)
	{
		string rawResponse = sendMessage(command);

		T response = JsonSerializer.Deserialize<T>(rawResponse, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
		return response;
	}

	internal static void Dispatch(string command)
	{
		string response = sendMessage($"dispatch {command}");
		if (response != "ok")
			Console.Error.WriteLine(response);
	}

	private static string sendMessage(string message)
	{
		string socketPath = $"/tmp/hypr/{Hyprland.Instance.Signature}/.socket.sock";
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