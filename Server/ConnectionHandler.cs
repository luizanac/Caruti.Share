using System.Diagnostics;
using System.Text;

namespace Server;

public class ConnectionHandler
{
    private readonly ConcurrentDictionary<Guid, WebSocket> _connections = new();

    public void RemoveConnection(Guid connectionId)
    {
        _connections.Remove(connectionId, out var webSocket);
        webSocket?.Dispose();
    }

    public IEnumerable<WebSocket> AllConnectionsExcept(Guid connectionId) =>
        _connections.Where(x => x.Key != connectionId).Select(x => x.Value);

    public async Task Handle(WebSocket webSocket)
    {
        var connectionId = Guid.NewGuid();
        var addSuccess = _connections.TryAdd(connectionId, webSocket);

        Console.WriteLine(addSuccess ? "Added" : "NoAdded");
        var timer = new Stopwatch();

        while (webSocket.State.Equals(WebSocketState.Open))
        {
            var buffer = new byte[1024 * 1024];

            timer.Restart();
            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            Console.WriteLine(receiveResult.Count / 1024 + "kb");

            var base64 = Convert.ToBase64String(buffer[..receiveResult.Count]);
            var connections = AllConnectionsExcept(connectionId);

            await Parallel.ForEachAsync(connections, async (socket, token) =>
                await socket.SendAsync(Encoding.ASCII.GetBytes(base64), WebSocketMessageType.Text,
                    WebSocketMessageFlags.EndOfMessage, token)
            );

            Console.WriteLine(timer.ElapsedMilliseconds + "ms");
            // await webSocket.SendAsync(
            //     new ArraySegment<byte>(buffer, 0, receiveResult.Count),
            //     receiveResult.MessageType,
            //     receiveResult.EndOfMessage,
            //     CancellationToken.None);
            //
            // receiveResult = await webSocket.ReceiveAsync(
            //     new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "EOC", CancellationToken.None);
        RemoveConnection(connectionId);
    }
}