namespace SennheiserBackend.Models
{
    /// <summary>
    /// Represents a hardware microphone with private getters.
    /// </summary>
    public class Microphone : IMicrophone
    {
        private int micGain = 50;
        
        ///<inheritdoc/>
        public string Id { get; set; } = "";
        ///<inheritdoc/>
        public virtual int MicGain
        {
            get => micGain;
            set
            {
                if (value < 0 || value > 100)
                {
                    throw new ArgumentException("Value needs to be between 0 and 100.", "MicGain");
                }
                micGain = value;
            }
        }

        /// <summary>
        /// Creates an instance clone.
        /// </summary>
        /// <returns>Cloned instance as object.</returns>
        public object Clone()
        {
            return (IMicrophone)MemberwiseClone();
        }
    }
}
