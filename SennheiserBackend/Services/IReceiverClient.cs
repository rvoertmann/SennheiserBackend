using SennheiserBackend.Models;

namespace SennheiserBackend.Services
{
    /// <summary>
    /// Represents a client to connect to a receiver device.
    /// </summary>
    public interface IReceiverClient
    {
        /// <summary>
        /// The receiver related to this client.
        /// </summary>
        public IReceiver Receiver { get; }
        /// <summary>
        /// Connected state of the client.
        /// </summary>
        public bool IsConnected { get; }

        /// <summary>
        /// Raised if a message has been received from the receiver device.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
        /// <summary>
        /// Raised if the connection has been closed.
        /// </summary>
        public event EventHandler? ConnectionClosed;

        /// <summary>
        /// Initiate connection to receiver and start listening.
        /// </summary>
        /// <returns></returns>
        public Task Connect();
        /// <summary>
        /// Close the connection to the receiver.
        /// </summary>
        /// <returns></returns>
        public Task Disconnect();
        /// <summary>
        /// Send an updated state to the receiver.
        /// </summary>
        /// <param name="receiver">The updated receiver model.</param>
        /// <returns></returns>
        public Task Update(IReceiver receiver);
    }
}
