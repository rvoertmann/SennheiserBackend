namespace SennheiserBackend.Services
{
    ///<inheritdoc cref="IWebSocketFactory"/>
    public class WebSocketFactory : IWebSocketFactory
    {
        ///<inheritdoc/>
        public IReceiverClientWebSocket Create()
        {
            return new ReceiverClientWebSocket();
        }
    }
}
