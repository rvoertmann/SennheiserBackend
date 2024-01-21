using SennheiserBackend.Models;

namespace SennheiserBackend.Services
{
    /// <summary>
    /// Flexibly creates a new receiver client instance.
    /// </summary>
    public interface IReceiverClientFactory
    {
        /// <summary>
        /// Creates a new receiver client instance.
        /// </summary>
        /// <param name="receiver">Receiver to create client for.</param>
        /// <returns>Receiver client instance.</returns>
        public IReceiverClient Create(IReceiver receiver);
    }
}
