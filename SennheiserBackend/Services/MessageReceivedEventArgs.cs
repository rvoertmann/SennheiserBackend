namespace SennheiserBackend.Services
{
    /// <summary>
    /// Specifies event arguments for a socket message event.
    /// </summary>
    /// <param name="message">Message related to the event.</param>
    public class MessageReceivedEventArgs(ReceiverSocketMessage message) : EventArgs
    {
        /// <summary>
        /// Message related to the event.
        /// </summary>
        public ReceiverSocketMessage Message => message;
    }
}
