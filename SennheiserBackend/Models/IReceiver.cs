namespace SennheiserBackend.Models
{
    /// <summary>
    /// Represents a receiver device with getters only.
    /// </summary>
    public interface IReceiver : ICloneable
    {
        /// <summary>
        /// Unique id of the receiver.
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// Name of the receiver as configured on the device.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Host under which the receiver is reachable.
        /// </summary>
        public string Host { get; }
        /// <summary>
        /// Port under which the receiver is reachable.
        /// </summary>
        public int? Port { get; }
        /// <summary>
        /// Host and port divided by :.
        /// </summary>
        public string AddressableHost { get; }
        /// <summary>
        /// The receiver's microphone.
        /// </summary>
        public IMicrophone Microphone { get; }
    }
}
