namespace SennheiserBackend.Services
{
    /// <summary>
    /// Flexibly creates a WebSocket instance.
    /// </summary>
    public interface IWebSocketFactory
    {
        /// <summary>
        /// Creates a new WebSocket instance.
        /// </summary>
        /// <returns>Instance of WebSocket.</returns>
        public IReceiverClientWebSocket Create();
    }
}
