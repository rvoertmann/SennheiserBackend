using SennheiserBackend.Models;
using System.Net.WebSockets;

namespace SennheiserBackend.Services
{
    ///<inheritdoc cref="IReceiverConnectionService"/>
    public class ReceiverConnectionService(IReceiverClientFactory clientFactory) : IReceiverConnectionService
    {
        private readonly List<IReceiverClient> receiverClients = [];

        ///<inheritdoc/>
        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

        ///<inheritdoc/>
        public async Task Open(IReceiver receiver)
        {
            if (receiverClients.Exists(r => r.Receiver.Id == receiver.Id))
            {
                throw new InvalidOperationException($"Receiver with id '{receiver.Id}' already connected.");
            }

            var receiverClient = clientFactory.Create(receiver);
            receiverClient.MessageReceived += OnClientMessageReceived;
            receiverClient.ConnectionClosed += OnConnectionClosed;

            await receiverClient.Connect();

            receiverClients.Add(receiverClient);
        }

        private void OnConnectionClosed(object? sender, EventArgs e)
        {
            if (sender is IReceiverClient client && receiverClients.Exists(c => c == client))
            {
                client.ConnectionClosed -= OnConnectionClosed;
                receiverClients.Remove(client);
            }
        }

        private void OnClientMessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            if (sender is IReceiverClient receiverClient)
            {
                MessageReceived?.Invoke(receiverClient.Receiver, e);
            }
        }

        ///<inheritdoc/>
        public void Close(string receiverId)
        {
            var receiverClient = GetClient(receiverId);

            receiverClient.MessageReceived -= OnClientMessageReceived;
            receiverClient.Disconnect();
        }

        ///<inheritdoc/>
        public bool IsConnected(string receiverId)
        {
            return receiverClients.Exists(c => c.Receiver.Id == receiverId && c.IsConnected);
        }

        ///<inheritdoc/>
        public async Task UpdateReceiverState(IReceiver receiver)
        {
            var receiverClient = GetClient(receiver.Id);

            if (!receiverClient.IsConnected)
            {
                throw new WebSocketException($"Connection to receiver with id '{receiver.Id}' is not open.");
            }

            await receiverClient.Update(receiver);
        }

        private IReceiverClient GetClient(string receiverId)
        {
            var receiverClient = receiverClients.Find(c => c.Receiver.Id == receiverId);

            if (receiverClient == null)
            {
                throw new ArgumentNullException($"No open connection for receiver with id '{receiverId}'.");
            }

            return receiverClient;
        }
    }
}
