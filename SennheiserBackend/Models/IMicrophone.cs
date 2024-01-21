namespace SennheiserBackend.Models
{
    /// <summary>
    /// Represents a hardware microphone with getters only.
    /// </summary>
    public interface IMicrophone : ICloneable
    {
        /// <summary>
        /// Unique id of the microphpne.
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// Gain of the microphone.
        /// </summary>
        public int MicGain { get; }
    }
}
