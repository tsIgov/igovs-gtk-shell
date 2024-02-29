using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Igs.Hyprland;

public delegate void SignalEventHandler(string eventName, string[] arguments);

public interface ISignalReceiver : IDisposable
{
	event SignalEventHandler? OnSignalReceived;

	void StartListening();
}

public class SignalReceiver : ISignalReceiver
{
	private readonly Socket _socket;
	private readonly UnixDomainSocketEndPoint _endpoint;
	private readonly CancellationTokenSource _cancellationTokenSource;
	private readonly ILogger<SignalReceiver>? _logger;

	public event SignalEventHandler? OnSignalReceived;

	public SignalReceiver(ISignatureProvider signatureProvider, ILoggerFactory? loggerFactory = null)
	{
		_logger = loggerFactory?.CreateLogger<SignalReceiver>();
		_socket = new(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
		_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
		string socketPath = $"/tmp/hypr/{signatureProvider.GetSignature()}/.socket2.sock";
		_endpoint = new(socketPath);
		_cancellationTokenSource = new CancellationTokenSource();
	}

	public void StartListening()
	{
		_socket.Connect(_endpoint);

		Task _ = Task.Run(() =>
		{
			using NetworkStream stream = new(_socket);
			using StreamReader reader = new(stream);

			while (true)
			{
				if (_cancellationTokenSource.Token.IsCancellationRequested)
					return;

				string? message = null;
				try
				{
					message = reader.ReadLine();
					if (!string.IsNullOrWhiteSpace(message))
						handleSignal(message);
				}
				catch (Exception ex)
				{
					_logger?.LogError(ex, "Failed handling the following Hyprland signal: \"{signal}\"", message);
				}
			}
		}, _cancellationTokenSource.Token);
	}

	private void handleSignal(string signal)
	{
		signal = signal.Trim();
		int index = signal.IndexOf(">>");
		if (index < 0)
			throw new ArgumentException("Unable to parse event type.");

		string eventName = signal[..index];
		string[] parameters = signal[(index + ">>".Length)..].Split(',');

		OnSignalReceived?.Invoke(eventName, parameters);
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_cancellationTokenSource.Cancel();
			_socket?.Dispose();
		}
	}
}