namespace SennheiserBackend.Models
{
    /// <summary>
    /// Represents a request for receiver creation.
    /// </summary>
    public class ReceiverCreateRequest
    {
        /// <summary>
        /// Name of the receiver as configured on the device.
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// Name of the host under which the receiver is rechable.
        /// </summary>
        public string Host { get; set; } = "";
        /// <summary>
        /// Number of the port under which the receiver is rechable.
        /// </summary>
        public int? Port { get; set; }
    }
}
