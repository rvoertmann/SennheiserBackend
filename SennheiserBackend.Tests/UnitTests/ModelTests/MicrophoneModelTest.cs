using FluentAssertions;
using SennheiserBackend.Models;

namespace SennheiserBackend.Tests.UnitTests.ModelTests
{
    [TestClass]
    public class MicrophoneModelTest
    {
        [TestMethod]
        public void SetMicGain_ValueOutOfRange_ShouldThrowException()
        {
            //ARRANGE
            var microphone = new Microphone();
            Exception? exceptionNegative = null;
            Exception? exceptionMax = null;

            //ACT
            try
            {
                microphone.MicGain = -1;
            }
            catch (Exception ex)
            {
                exceptionNegative = ex;
            }

            try
            {
                microphone.MicGain = 101;
            }
            catch (Exception ex)
            {
                exceptionMax = ex;
            }

            //ASSERT
            exceptionNegative.Should().NotBeNull();
            exceptionNegative.Should().BeOfType<ArgumentException>();

            exceptionMax.Should().NotBeNull();
            exceptionMax.Should().BeOfType<ArgumentException>();
        }
    }
}
