namespace Server;

public class ConnectionHandler
{
    private readonly ConcurrentDictionary<Guid, WebSocket> _connections = new();

    private void RemoveConnection(Guid connectionId)
    {
        _connections.Remove(connectionId, out var webSocket);
        webSocket?.Dispose();
    }

    private IEnumerable<WebSocket> AllConnectionsExcept(Guid connectionId) =>
        _connections.Where(x => x.Key != connectionId).Select(x => x.Value);

    public async Task Handle(WebSocket webSocket)
    {
        var connectionId = Guid.NewGuid();
        _connections.TryAdd(connectionId, webSocket);

        while (webSocket.State.Equals(WebSocketState.Open))
        {
            //TODO: Tornar a criação do buffer de maneira dinâmica criando um contrato que pede especificação do tamanho do envio em bytes;
            var buffer = new byte[1024 * 1024];

            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            var input = (char)buffer[0] == '{'
                ? buffer.AsMemory()[..receiveResult.Count]
                : Encoding.ASCII.GetBytes(Convert.ToBase64String(buffer[..receiveResult.Count]));

            var connections = AllConnectionsExcept(connectionId);

            await Parallel.ForEachAsync(connections, async (socket, token) =>
                await socket.SendAsync(input, WebSocketMessageType.Text,
                    WebSocketMessageFlags.EndOfMessage, token)
            );
        }

        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "EOC", CancellationToken.None);
        RemoveConnection(connectionId);
    }
}