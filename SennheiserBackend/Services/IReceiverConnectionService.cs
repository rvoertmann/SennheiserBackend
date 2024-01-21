using SennheiserBackend.Models;

namespace SennheiserBackend.Services
{
    /// <summary>
    /// Service to manage receiver connections.
    /// </summary>
    public interface IReceiverConnectionService
    {
        /// <summary>
        /// Raised when a registered and connected client receives a message.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

        /// <summary>
        /// Open a connection to a receiver.
        /// </summary>
        /// <param name="receiver">Receiver to connect to.</param>
        /// <returns></returns>
        public Task Open(IReceiver receiver);
        /// <summary>
        /// Close an open connection to a receiver.
        /// </summary>
        /// <param name="receiverId">Id of receiver to close the connection for.</param>
        public void Close(string receiverId);
        /// <summary>
        /// Sends the the state of the receiver model to the device.
        /// </summary>
        /// <param name="receiver">Receiver model containing state to send.</param>
        /// <returns></returns>
        public Task UpdateReceiverState(IReceiver receiver);
        /// <summary>
        /// Checks wheher a receiver is connected.
        /// </summary>
        /// <param name="receiverId">Id of the receiver to get the connection status for.</param>
        /// <returns>True if connected, false otherwise.</returns>
        public bool IsConnected(string receiverId);
    }
}
