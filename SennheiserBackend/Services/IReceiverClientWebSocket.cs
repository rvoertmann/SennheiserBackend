using System.Net.WebSockets;

namespace SennheiserBackend.Services
{
    /// <summary>
    /// Facade for a receiver client's WebSocket.
    /// </summary>
    public interface IReceiverClientWebSocket
    {
        /// <summary>
        /// Current state of the WebSocket.
        /// </summary>
        public WebSocketState State { get; }
        /// <summary>
        /// Connect to the WebSocket.
        /// </summary>
        /// <param name="uri">Uri to connect to.</param>
        /// <param name="cancellationToken">Cancellation token to cancel operation.</param>
        /// <returns></returns>
        public Task ConnectAsync(Uri uri, CancellationToken cancellationToken);
        /// <summary>
        /// Starts receiving / listening for a WebSocket message.
        /// </summary>
        /// <param name="buffer">Buffer containing the received message.</param>
        /// <param name="cancellationToken">Cancellation token to cancel operation.</param>
        /// <returns>Result of the receive operation.</returns>
        public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken);
        /// <summary>
        /// Closes the WebSocket connection.
        /// </summary>
        /// <param name="closeStatus">Status of the closure.</param>
        /// <param name="statusDescription">Description of closure.</param>
        /// <param name="cancellationToken">Cancellation token to cancel operation.</param>
        /// <returns></returns>
        public Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken);
        /// <summary>
        /// Sends a message to the WebSocket.
        /// </summary>
        /// <param name="buffer">Buffer to write message to.</param>
        /// <param name="messageType">Type of message to send.</param>
        /// <param name="endOfMessage">Marks whether this is the end of a message.</param>
        /// <param name="cancellationToken">Cancellation token to cancel operation.</param>
        /// <returns></returns>
        public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);
    }
}
