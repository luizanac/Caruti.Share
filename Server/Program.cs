var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ConnectionHandler>();

var app = builder.Build();
app.UseWebSockets();

app.Use(async (context, next) =>
{
    var connectionHandler = app.Services.GetRequiredService<ConnectionHandler>();
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await connectionHandler.Handle(webSocket);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    else
    {
        await next(context);
    }
});

app.Run();