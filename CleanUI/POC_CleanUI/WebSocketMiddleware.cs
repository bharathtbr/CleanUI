using Microsoft.AspNetCore.Http;
using Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class WebSocketMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SoapServiceClient _soapServiceClient;
    private readonly HttpClient _httpClient;
    private readonly TimeSpan _fetchInterval = TimeSpan.FromSeconds(2); // Fetch interval in seconds
    private CancellationTokenSource _cancellationTokenSource;
    private int counter = 0;
    public static List<string> messages = new List<string>();
    public WebSocketMiddleware(RequestDelegate next, SoapServiceClient soapServiceClient, IHttpClientFactory httpClientFactory)
    {
        _next = next;
        _soapServiceClient = soapServiceClient;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path == "/ws")
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
                {
                    await HandleWebSocketConnection(webSocket);
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
        else if (context.Request.Path == "/api/ws")
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
                {
                    await HandleApiWebSocket(webSocket);
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
        else
        {
            await _next(context);
        }
    }

    private async Task HandleApiWebSocket(WebSocket webSocket)
    {
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            while (!webSocket.CloseStatus.HasValue)
            {
                // Fetch data from API endpoint (/api/messages)
                // Fetch data from API endpoint (/api/messages)
                List<string> apiResponses = await FetchApiData();

                // Send each string in the list to the WebSocket client
                foreach (var response in apiResponses)
                {
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }

                // Delay before fetching data again
                await Task.Delay(_fetchInterval, _cancellationTokenSource.Token);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket error: {ex.Message}");
            await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None);
        }
    }

    private async Task<List<string>> FetchApiData()
    {
        try
        {
            var response = await _httpClient.GetAsync("https://localhost:7049/api/messages");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<string>>(responseString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to fetch API data: {ex.Message}");
            return new List<string> { $"Failed to fetch API data: {ex.Message}" };
        }
    }
    private async Task HandleWebSocketConnection(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!result.CloseStatus.HasValue)
        {
            var clientMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var serverMessage = $"SOAP: Server received: {clientMessage}";
            if (clientMessage.StartsWith("PingComplexModel:"))
            {
                var parameter = clientMessage.Substring("PingComplexModel:".Length).Trim();
                var soapResult1 = _soapServiceClient.PingComplexModel(new ComplexModelInput
                {
                    StringProperty = parameter,
                    IntProperty = 123,
                    ListProperty = new List<string> { "test", "list", "of", "strings" },
                    DateTimeOffsetProperty = DateTimeOffset.UtcNow
                });

                // Send SOAP service response back to client
                var responseBytes = Encoding.UTF8.GetBytes($"SOAP service result: {soapResult1.StringProperty}");
                serverMessage += $"\nSOAP PingComplexModel service result:";
            }
            else
            {
                // Example of calling a SOAP service method
                var complexModel = new ComplexModelInput
                {
                    StringProperty = Guid.NewGuid().ToString(),
                    IntProperty = int.MaxValue / 2,
                    ListProperty = new List<string> { "test", "list", "of", "strings" },
                    DateTimeOffsetProperty = new DateTimeOffset(2018, 12, 31, 13, 59, 59, TimeSpan.FromHours(1))
                };

                var complexResult = _soapServiceClient.PingComplexModel(complexModel);
                var soapResult = _soapServiceClient.Ping("Recieved");
                serverMessage += $"\nSOAP service result: {soapResult}";
            }           

            var responseMessage = Encoding.UTF8.GetBytes(serverMessage);
            await webSocket.SendAsync(new ArraySegment<byte>(responseMessage, 0, responseMessage.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }
}
