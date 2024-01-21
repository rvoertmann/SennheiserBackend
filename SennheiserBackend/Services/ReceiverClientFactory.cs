using SennheiserBackend.Models;

namespace SennheiserBackend.Services
{
    ///<inheritdoc cref="IReceiverClientFactory"/>
    public class ReceiverClientFactory(ILogger<ReceiverClient> logger, IWebSocketFactory webSocketFactory) : IReceiverClientFactory
    {
        ///<inheritdoc/>
        public IReceiverClient Create(IReceiver receiver)
        {
            return new ReceiverClient(receiver, webSocketFactory, logger);
        }
    }
}
