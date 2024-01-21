using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SennheiserBackend.Services;
using SennheiserDeviceSimulator.Model;
using System.Net.WebSockets;
using System.Text;

namespace SennheiserDeviceSimulator.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ApiController : Controller
    {
        private readonly Microphone microphone;
        private WebSocket? webSocket;

        public ApiController(Microphone microphone)
        {
            this.microphone = microphone;
            microphone.MicGainChanged += OnMicGainChanged;
        }

        private void OnMicGainChanged(object? mic, EventArgs e)
        {
            if (webSocket != null && webSocket.State == WebSocketState.Open)
            {
                var message = new SimReceiverSocketMessage
                {
                    Properties =
                    {
                        { "micGain", microphone.MicGain.ToString() }
                    }
                };
                var json = JsonConvert.SerializeObject(message);
                webSocket.SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes(json)), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        [Route("/")]
        public async Task Connect()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var message = new SimReceiverSocketMessage
                {
                    Properties =
                    {
                        { "micGain", microphone.MicGain.ToString() },
                        { "name", "SimReceiver" }
                    }
                };
                var json = JsonConvert.SerializeObject(message);
                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes(json)), WebSocketMessageType.Text, true, CancellationToken.None);
                await Receive(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private async Task Receive(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult? receiveResult = null;

            while (webSocket.State == WebSocketState.Open)
            {
                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);

                if (receiveResult.MessageType == WebSocketMessageType.Text)
                {
                    var content = Encoding.ASCII.GetString(buffer);
                    var message = JsonConvert.DeserializeObject<SimReceiverSocketMessage>(content);
                    if(message != null)
                    {
                        microphone.MicGain = int.Parse(message.Properties["micGain"]);
                    }
                }

                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
            }

            if (receiveResult != null)
            {
                await webSocket.CloseAsync(receiveResult.CloseStatus.GetValueOrDefault(), receiveResult.CloseStatusDescription, CancellationToken.None);
            }
        }
    }
}
