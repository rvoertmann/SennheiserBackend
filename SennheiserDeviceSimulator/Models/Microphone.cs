namespace SennheiserDeviceSimulator.Model
{
    public class Microphone(ILogger<Microphone> logger)
    {
        private int micGain = 60;

        public event EventHandler? MicGainChanged;

        public int MicGain
        {
            get
            {
                return micGain;
            }
            set
            {
                if (value != micGain)
                {

                    if (value < 0)
                    {
                        micGain = 0;
                    }
                    else if (value > 100)
                    {
                        micGain = 100;
                    }
                    else
                    {
                        micGain = value;
                    }

                    logger.LogInformation("MicGain changed to {micGain}", micGain);
                    MicGainChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
