using SennheiserBackend.Models;

namespace SennheiserBackend.Services
{
    /// <summary>
    /// Service managing receivers. Holds the internal model and database connections through a repository.
    /// </summary>
    public interface IReceiverService
    {
        /// <summary>
        /// Creates a new receiver.
        /// </summary>
        /// <param name="name">Name of the receiver as configured on the device.</param>
        /// <param name="host">Host under which the receiver can be reached.</param>
        /// <param name="port">Port under which the receiver can be reached.</param>
        /// <returns>The created receiver.</returns>
        public Task<IReceiver> Create(string name, string host, int? port);
        /// <summary>
        /// Get a receiver from the model.
        /// </summary>
        /// <param name="id">Id of the receiver to get from the model.</param>
        /// <returns>Receiver from the model.</returns>
        public IReceiver? Get(string id);
        /// <summary>
        /// Gets all receivers from the model.
        /// </summary>
        /// <returns>Enumerable collection of receivers.</returns>
        public IEnumerable<IReceiver> GetAll();
        /// <summary>
        /// Updates the receiver on the model doing a comparison for changes first.
        /// </summary>
        /// <param name="receiver">Receiver model containing the updates.</param>
        /// <returns>Updated receiver.</returns>
        public Task<IReceiver> Update(Receiver receiver);
        /// <summary>
        /// Deletes a receiver.
        /// </summary>
        /// <param name="id">Id of receiver to delete.</param>
        /// <returns></returns>
        public Task Delete(string id);
        /// <summary>
        /// Connects to a receiver.
        /// </summary>
        /// <param name="receiver">Receiver to connect to.</param>
        /// <returns></returns>
        public Task Connect(IReceiver receiver);
        /// <summary>
        /// Disconnect from a receiver.
        /// </summary>
        /// <param name="receiverId">Id of receiver to disconnect from.</param>
        public void Disconnect(string receiverId);
        /// <summary>
        /// Checks whether a receiver is connected.
        /// </summary>
        /// <param name="receiverId">Receiver to check connection state for.</param>
        /// <returns>True if connected, false otherwise.</returns>
        public bool IsConnected(string receiverId);
    }
}
