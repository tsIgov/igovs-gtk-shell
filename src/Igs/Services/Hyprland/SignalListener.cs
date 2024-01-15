using System.Net.Sockets;
using Igs.Services.Hyprland.Events;

namespace Igs.Services.Hyprland;

internal class SignalListener : IDisposable
{
    private HyprlandService _service;
    private Socket _socket;
    private CancellationTokenSource _cancellationTokenSource = new ();


    internal SignalListener(HyprlandService service)
    {
        _service = service;

        string socketPath = $"/tmp/hypr/{service.HyprlandInstanceSignature}/.socket2.sock";
        UnixDomainSocketEndPoint endPoint = new (socketPath);

        _socket = new (AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        _socket.Connect(endPoint);

        NetworkStream stream = new (_socket);
        StreamReader reader = new (stream);

        Task _ = Task.Run(() =>
        {
            while(true)
            {
                if(_cancellationTokenSource.Token.IsCancellationRequested)
                    return;

                string? message = null;
                try 
                {
                    message = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(message))
                        handleSignal(message);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Failed handling Hyprland signal.\nMessage: {message}\nException: {ex.Message}");
                }
            }
        }, _cancellationTokenSource.Token);
    }


    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _socket?.Dispose();
    }

    private void handleSignal(string signal)
    {
        signal = signal.Trim();
        int index = signal.IndexOf(">>");
        if (index < 0)
            Console.Error.WriteLine($"Unable to parse Hyprland signal: \"{signal}\"");

        string eventName = signal[..index];
        string data = signal[(index + ">>".Length)..];

        (Action<string[]> del, int paramCount)? handlerInfo = eventName switch
        {
            "activewindow" => ( handleActiveWindowEvent, 2 ),
            _ => null
        };

        if (handlerInfo == null)
            return;

        string[] parameters = data.Split(',', handlerInfo.Value.paramCount);
        if (parameters.Length != handlerInfo.Value.paramCount)
            Console.Error.WriteLine($"Unable to parse Hyprland signal: \"{signal}\"");

        handlerInfo.Value.del.Invoke(parameters);
    }

    private void handleActiveWindowEvent(string[] parameters)
    {
        string windowClass = parameters[0];
        string windowTitle = parameters[1];

        WindowEventArgs args = new(windowClass, windowTitle);
        _service.WindowEvents.InvokeOnActiveWindowChanged(args);

        Console.WriteLine($"{windowClass} | {windowTitle}");
    }
}