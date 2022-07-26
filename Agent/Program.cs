using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using SharpHook;
using SharpHook.Native;

var service = new ScreenshotService();

var host = new Uri("ws://10.0.0.147:5118/ws");
var socket = new ClientWebSocket();
await socket.ConnectAsync(host, CancellationToken.None);
var tokenSource = new CancellationTokenSource();

var captureTask = Task.Run(async () =>
{
    while (!tokenSource.Token.IsCancellationRequested)
    {
        try
        {
            var buffer = service.GetImageBytes();
            if (socket.State != WebSocketState.Open)
                continue;

            await socket.SendAsync(buffer, WebSocketMessageType.Binary, WebSocketMessageFlags.EndOfMessage,
                tokenSource.Token);
        }
        catch (Exception)
        {
            await socket.CloseAsync(WebSocketCloseStatus.InternalServerError,
                nameof(WebSocketCloseStatus.InternalServerError), tokenSource.Token);
        }
    }
});

var inputTask = Task.Run(async () =>
{
    while (!tokenSource.Token.IsCancellationRequested)
    {
        var buffer = new byte[1024 * 1024];

        var receiveResult = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), tokenSource.Token);
        var json = Encoding.ASCII.GetString(buffer[..receiveResult.Count]);
        var contract = JsonSerializer.Deserialize<Contract>(json,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        if (contract is null)
            continue;

        var simulator = new EventSimulator();
        switch (contract.Event)
        {
            case "MouseClick":
            {
                simulator.SimulateMousePress(MouseButton.Button1);
                await Task.Delay(50);
                simulator.SimulateMouseRelease(MouseButton.Button1);
                break;
            }
            case "MouseMove":
            {
                simulator.SimulateMouseMovement((short)contract.Data.X, (short)contract.Data.Y);
                break;
            }
        }


        Console.WriteLine(contract?.Event);
    }
});

Task.WaitAll(captureTask, inputTask);