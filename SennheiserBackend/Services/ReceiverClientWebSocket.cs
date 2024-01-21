using System.Net.WebSockets;

namespace SennheiserBackend.Services
{
    ///<inheritdoc cref="IReceiverClientWebSocket"/>
    public class ReceiverClientWebSocket : IReceiverClientWebSocket
    {
        private readonly ClientWebSocket webSocket;

        ///<inheritdoc/>
        public ReceiverClientWebSocket()
        {
            webSocket = new ClientWebSocket();
        }

        ///<inheritdoc/>
        public WebSocketState State => webSocket.State;

        ///<inheritdoc/>
        public async Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
        {
            await webSocket.CloseOutputAsync(closeStatus, statusDescription, cancellationToken);
        }

        ///<inheritdoc/>
        public async Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            await webSocket.ConnectAsync(uri, cancellationToken);
        }

        ///<inheritdoc/>
        public async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            return await webSocket.ReceiveAsync(buffer, cancellationToken);
        }

        ///<inheritdoc/>
        public async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            await webSocket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);
        }
    }
}
