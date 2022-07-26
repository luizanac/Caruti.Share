using System.Net.WebSockets;

const string fileName = "output.jpeg";
var service = new ScreenshotService();

var host = new Uri("ws://localhost:5118/ws");
var client = new ClientWebSocket();
await client.ConnectAsync(host, CancellationToken.None);

var timer = new Stopwatch();
while (true)
{
    try
    {
        timer.Restart();

        var buffer = service.GetImageBytes(fileName);
        Console.WriteLine(buffer.Length / 1024);
        if (client.State != WebSocketState.Open)
            continue;

        await client.SendAsync(buffer, WebSocketMessageType.Binary,
            WebSocketMessageFlags.EndOfMessage,
            CancellationToken.None);
        Console.WriteLine(timer.ElapsedMilliseconds + "ms");
    }
    catch (Exception)
    {
        await client.CloseAsync(WebSocketCloseStatus.InternalServerError,
            nameof(WebSocketCloseStatus.InternalServerError),
            CancellationToken.None);
    }
}