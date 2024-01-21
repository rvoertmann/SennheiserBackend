namespace SennheiserBackend.Models
{
    /// <summary>
    /// Represents a receiver device.
    /// </summary>
    public class Receiver : IReceiver
    {
        ///<inheritdoc/>
        public string Id { get; set; } = string.Empty;
        ///<inheritdoc/>
        public string Name { get; set; } = string.Empty;
        ///<inheritdoc/>
        public string Host { get; set; } = string.Empty;
        ///<inheritdoc/>
        public string AddressableHost
        {
            get
            {
                return GetAddressableHost(Host, Port);
            }
        }
        ///<inheritdoc/>
        public int? Port { get; set; }
        ///<inheritdoc/>
        public IMicrophone Microphone { get; set; } = new Microphone();

        private static string GetAddressableHost(string host, int? port)
        {
            return port == null ? host : $"{host}:{port}";
        }

        ///<inheritdoc/>
        public object Clone()
        {
            var clonedReceiver = (Receiver)MemberwiseClone();
            clonedReceiver.Microphone = (IMicrophone)Microphone.Clone();

            return clonedReceiver;
        }
    }
}
