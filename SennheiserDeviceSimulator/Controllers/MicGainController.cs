using Microsoft.AspNetCore.Mvc;
using SennheiserDeviceSimulator.Model;

namespace SennheiserDeviceSimulator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MicGainController(Microphone microphone) : ControllerBase
    {

        /// <summary>
        /// Get current MicGain value.
        /// </summary>
        /// <param name="micGain">Value to set MicGain to.</param>
        [HttpPost]
        [Route("{micGain?}")]
        public IActionResult Set(int micGain)
        {
            if (micGain < 0)
            {
                microphone.MicGain = 0;
            }
            else if (micGain > 100)
            {
                microphone.MicGain = 100;
            }
            else
            {
                microphone.MicGain = micGain;
            }

            return new AcceptedResult();
        }

        /// <summary>
        /// Get current MicGain value.
        /// </summary>
        /// <returns>Current MicGain value.</returns>
        [HttpGet]
        public IActionResult Get()
        {
            return new OkObjectResult(microphone.MicGain);
        }
    }
}
