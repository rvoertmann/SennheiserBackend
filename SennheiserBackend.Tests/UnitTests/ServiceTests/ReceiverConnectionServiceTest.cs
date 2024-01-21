using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SennheiserBackend.Services;
using SennheiserBackend.Tests.TestData;

namespace SennheiserBackend.Tests.UnitTests.ServiceTests
{
    [TestClass]
    public class ReceiverConnectionServiceTest
    {
        [TestMethod]
        public async Task Open_Always_ShouldInitiateConnection()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var testReceiver = testData.Receivers[0];

            var clientMock = new Mock<IReceiverClient>();

            var clientFactoryMock = new Mock<IReceiverClientFactory>();
            clientFactoryMock.Setup(c => c.Create(testReceiver)).Returns(clientMock.Object);

            var receiverConnectionService = new ReceiverConnectionService(clientFactoryMock.Object);

            //ACT
            await receiverConnectionService.Open(testReceiver);

            //ASSERT
            clientMock.Verify(c => c.Connect(), Times.Once);
            clientMock.VerifyAdd(c => c.MessageReceived += It.IsAny<EventHandler<MessageReceivedEventArgs>>());
        }

        [TestMethod]
        public async Task OnClientMessageReceived_ValidReceiver_ShouldRaiseMessageReceived()
        {
            //ARRANGE
            var eventFired = false;

            var testData = new ReceiverTestData();

            var testReceiver = testData.Receivers[0];

            var clientMock = new Mock<IReceiverClient>();

            var clientFactoryMock = new Mock<IReceiverClientFactory>();
            clientFactoryMock.Setup(c => c.Create(testReceiver)).Returns(clientMock.Object);

            var receiverConnectionService = new ReceiverConnectionService(clientFactoryMock.Object);
            receiverConnectionService.MessageReceived += (sender, args) => eventFired = true;

            //ACT
            await receiverConnectionService.Open(testReceiver);
            clientMock.Raise(c => c.MessageReceived += null, clientMock.Object, new MessageReceivedEventArgs(new ReceiverSocketMessage()));

            //ASSERT
            eventFired.Should().BeTrue();
        }

        [TestMethod]
        public async Task Close_ReceiverConnected_ShouldDisonnectAndRemoveHandler()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var testReceiver = testData.Receivers[0];

            var clientMock = new Mock<IReceiverClient>();
            clientMock.SetupGet(c => c.Receiver).Returns(testReceiver);

            var clientFactoryMock = new Mock<IReceiverClientFactory>();
            clientFactoryMock.Setup(c => c.Create(testReceiver)).Returns(clientMock.Object);

            var receiverConnectionService = new ReceiverConnectionService(clientFactoryMock.Object);

            //ACT
            await receiverConnectionService.Open(testReceiver);
            receiverConnectionService.Close(testReceiver.Id);

            //ASSERT
            clientMock.Verify(c => c.Disconnect(), Times.Once());
            clientMock.VerifyRemove(c => c.MessageReceived -= It.IsAny<EventHandler<MessageReceivedEventArgs>>());
        }

        [TestMethod]
        public void Close_ReceiverNotConnected_ShouldThrowException()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var testReceiver = testData.Receivers[0];

            var clientMock = new Mock<IReceiverClient>();
            clientMock.SetupGet(c => c.Receiver).Returns(testReceiver);

            var clientFactoryMock = new Mock<IReceiverClientFactory>();
            clientFactoryMock.Setup(c => c.Create(testReceiver)).Returns(clientMock.Object);

            var receiverConnectionService = new ReceiverConnectionService(clientFactoryMock.Object);

            var action = () => receiverConnectionService.Close(testReceiver.Id);

            //ACT / ASSERT
            action.Should().Throw<Exception>();
        }

        [TestMethod]
        public async Task IsConnected_Always_ShouldReturnTrueIfConnected()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var testReceiver = testData.Receivers[0];

            var clientMock = new Mock<IReceiverClient>();
            clientMock.SetupGet(c => c.Receiver).Returns(testReceiver);
            clientMock.SetupGet(c => c.IsConnected).Returns(true);

            var clientFactoryMock = new Mock<IReceiverClientFactory>();
            clientFactoryMock.Setup(c => c.Create(testReceiver)).Returns(clientMock.Object);

            var receiverConnectionService = new ReceiverConnectionService(clientFactoryMock.Object);

            //ACT
            var isConnectedBefore = receiverConnectionService.IsConnected(testReceiver.Id);
            await receiverConnectionService.Open(testReceiver);
            var isConnectedAfter = receiverConnectionService.IsConnected(testReceiver.Id);

            receiverConnectionService.Close(testReceiver.Id);

            //ASSERT
            isConnectedBefore.Should().BeFalse();
            isConnectedAfter.Should().BeTrue();
        }

        [TestMethod]
        public async Task UpdateReceiverState_ReceiverConnected_ShouldSendUpdate()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var testReceiver = testData.Receivers[0];

            var clientMock = new Mock<IReceiverClient>();
            clientMock.SetupGet(c => c.Receiver).Returns(testReceiver);
            clientMock.SetupGet(c => c.IsConnected).Returns(true);

            var clientFactoryMock = new Mock<IReceiverClientFactory>();
            clientFactoryMock.Setup(c => c.Create(testReceiver)).Returns(clientMock.Object);

            var receiverConnectionService = new ReceiverConnectionService(clientFactoryMock.Object);

            //ACT
            await receiverConnectionService.Open(testReceiver);
            await receiverConnectionService.UpdateReceiverState(testReceiver);

            //ASSERT
            clientMock.Verify(c => c.Update(testReceiver), Times.Once);
        }

        [TestMethod]
        public async Task UpdateReceiverState_ReceiverNotConnected_ShouldThrowException()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var testReceiver = testData.Receivers[0];

            var clientMock = new Mock<IReceiverClient>();
            clientMock.SetupGet(c => c.Receiver).Returns(testReceiver);
            clientMock.SetupGet(c => c.IsConnected).Returns(false);

            var clientFactoryMock = new Mock<IReceiverClientFactory>();
            clientFactoryMock.Setup(c => c.Create(testReceiver)).Returns(clientMock.Object);

            var receiverConnectionService = new ReceiverConnectionService(clientFactoryMock.Object);

            Func<Task> action = async () => await receiverConnectionService.UpdateReceiverState(testReceiver);

            //ACT / ASSERT
            await action.Should().ThrowAsync<Exception>();
        }
    }
}
