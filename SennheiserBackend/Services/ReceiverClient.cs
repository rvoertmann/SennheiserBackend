using Newtonsoft.Json;
using SennheiserBackend.Models;
using System.Net.WebSockets;
using System.Text;

namespace SennheiserBackend.Services
{
    ///<inheritdoc cref="IReceiverClient"/>
    public class ReceiverClient(IReceiver receiver, IWebSocketFactory webSocketFactory, ILogger<ReceiverClient> logger) : IReceiverClient
    {
        private const int BufferSize = 1024;
        private const int verificationTimeoutMs = 3000;

        private readonly IReceiverClientWebSocket webSocket = webSocketFactory.Create();
        private readonly IReceiver receiver = receiver;
        private readonly ILogger<ReceiverClient> logger = logger;

        ///<inheritdoc/>
        public IReceiver Receiver => receiver;
        ///<inheritdoc/>
        public bool IsConnected => webSocket.State == WebSocketState.Open;

        ///<inheritdoc/>
        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
        ///<inheritdoc/>
        public event EventHandler? ConnectionClosed;

        ///<inheritdoc/>
        public async Task Connect()
        {
            if (webSocket.State != WebSocketState.None)
            {
                throw new WebSocketException($"Connection already in use for receiver with id '{receiver.Id}'");
            }

            logger.LogInformation("Attempt to connect to receiver with id {receiver.Id}, {receiver.AddressableHost}", receiver.Id, receiver.AddressableHost);

            var uri = new Uri($"ws://{receiver.AddressableHost}");
            await webSocket.ConnectAsync(uri, CancellationToken.None);

            //Receive first message
            using var cancellationTokenSource = new CancellationTokenSource(verificationTimeoutMs);
            var bytesReceived = WebSocket.CreateClientBuffer(BufferSize, BufferSize);
            var result = await webSocket.ReceiveAsync(bytesReceived, cancellationTokenSource.Token);

            var message = await ParseMessage(bytesReceived, result);

            if(message == null)
            {
                throw new WebSocketException($"Connection to receiver with id '{receiver.Id}' could not be verified and will be closed: Name mismatch or bad format.");
            }

            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
#pragma warning disable CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
            BeginListen();
#pragma warning restore CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
        }

        private async Task BeginListen()
        {
            try
            {
                logger.LogInformation("Start listening to receiver with id {receiver.Id}, {receiver.AddressableHost}", receiver.Id, receiver.AddressableHost);

                while (webSocket.State == WebSocketState.Open)
                {
                    var bytesReceived = WebSocket.CreateClientBuffer(BufferSize, BufferSize);
                    var result = await webSocket.ReceiveAsync(bytesReceived, CancellationToken.None);

                    if (result.CloseStatus.HasValue)
                    {
                        logger.LogInformation("Rreceiver with id {receiver.Id}, {receiver.AddressableHost} sent close message.", receiver.Id, receiver.AddressableHost);
                        if (webSocket.State != WebSocketState.Closed)
                        {
                            await webSocket.CloseOutputAsync(result.CloseStatus.Value, "ServerClose", CancellationToken.None);
                        }

                        break;
                    }

                    var message = await ParseMessage(bytesReceived, result);

                    if(message != null)
                    {
                        MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
                    }
                }

                logger.LogInformation("Stop listening to receiver with id {receiver.Id}, {receiver.AddressableHost}", receiver.Id, receiver.AddressableHost);
            }
            catch (Exception ex)
            {
                logger.LogWarning("Receiver with id {receiver.Id}, {receiver.AddressableHost} suddenly lost connection. {ex.Message}", receiver.Id, receiver.AddressableHost, ex.Message);
            }

            ConnectionClosed?.Invoke(this, EventArgs.Empty);
        }

        private async Task<ReceiverSocketMessage?> ParseMessage(ArraySegment<byte> bytesReceived, WebSocketReceiveResult result)
        {
            if (!result.EndOfMessage)
            {
                logger.LogError("Received websocket message was too long to be processed (> {BufferSize} bytes).", BufferSize);
                return null;
            }

            var message = Encoding.ASCII.GetString(bytesReceived.ToArray());
            var receiverMessage = JsonConvert.DeserializeObject<ReceiverSocketMessage>(message);
            if (receiverMessage == null)
            {
                logger.LogError("The received message does not contain the right format (expected {ExpectedType}).", typeof(ReceiverSocketMessage));

                return null;
            }
            else
            {
                if (receiverMessage.Properties.TryGetValue("name", out string? value) && value != receiver.Name)
                {
                    logger.LogError("The answering receiver's name differs from the configured name. Configured: {receiver.Name} | Answered: {value}. The connection will be closed.", receiver.Name, value);
                    await Disconnect();

                    return null;
                }
            }

            return receiverMessage;
        }

        ///<inheritdoc/>
        public async Task Disconnect()
        {
            if (webSocket.State != WebSocketState.None)
            {
                logger.LogInformation("Initiate close handshake with receiver with id {receiver.Id}, {receiver.AddressableHost}", receiver.Id, receiver.AddressableHost);

                await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "ClientClose", CancellationToken.None);
            }
        }

        ///<inheritdoc/>
        public async Task Update(IReceiver receiver)
        {
            var message = new ReceiverSocketMessage
            {
                Properties =
                {
                    {"micGain", receiver.Microphone.MicGain.ToString() }
                }
            };

            var bytesToSend = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(message));
            await webSocket.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
