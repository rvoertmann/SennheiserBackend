using FluentAssertions;
using SennheiserBackend.Models;

namespace SennheiserBackend.Tests.UnitTests.ModelTests
{
    [TestClass]
    public class ReceiverModelTest
    {
        [TestMethod]
        public void GetAddressableHost_HostAndPort_ShouldReturnHostAndPort()
        {
            //ARRANGE
            var receiver = new Receiver
            {
                Host = "localhost",
                Port = 8080
            };

            //ACT
            var addressableHost = receiver.AddressableHost;

            //ASSERT
            addressableHost.Should().Be("localhost:8080");
        }

        [TestMethod]
        public void GetAddressableHost_HostOnly_ShouldReturnHostAndPort()
        {
            //ARRANGE
            var receiver = new Receiver
            {
                Host = "localhost",
                Port = null
            };

            //ACT
            var addressableHost = receiver.AddressableHost;

            //ASSERT
            addressableHost.Should().Be("localhost");
        }
    }
}
