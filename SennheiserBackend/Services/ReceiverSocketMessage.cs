namespace SennheiserBackend.Services
{
    /// <summary>
    /// Represents a message from a WebSocket.
    /// </summary>
    public class ReceiverSocketMessage
    {
        /// <summary>
        /// Collection of properties contained in the message.
        /// </summary>
        public Dictionary<string, string> Properties { get; } = [];
    }
}
