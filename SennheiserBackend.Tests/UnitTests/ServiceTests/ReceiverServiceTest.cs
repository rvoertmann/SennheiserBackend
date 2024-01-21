using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SennheiserBackend.Database.Repositories;
using SennheiserBackend.Database.Repositories.Entities;
using SennheiserBackend.Models;
using SennheiserBackend.Services;
using SennheiserBackend.Tests.TestData;
using System.Data;

namespace SennheiserBackend.Tests.UnitTests.ServiceTests
{
    [TestClass]
    public class ReceiverServiceTest
    {
        private readonly Mock<ILogger<ReceiverService>> logger = new();

        [TestMethod]
        public void Constructor_Instantiation_ShouldLoadReceivers()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var repositoryMock = new Mock<IReceiverRepository>();
            repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(testData.ReceiverEntities);

            var connectionServiceMock = new Mock<IReceiverConnectionService>();

            //ACT
            var receiverService = new ReceiverService(repositoryMock.Object, connectionServiceMock.Object, logger.Object);
            var loadedReceivers = receiverService.GetAll();

            //ASSERT
            repositoryMock.Verify(r => r.GetAll(), Times.Once);
            loadedReceivers.Should().HaveCount(testData.ReceiverEntities.Count);
            var ids = testData.ReceiverEntities.Select(r => r.Id);
            loadedReceivers.Should().AllSatisfy(r => ids.Contains(r.Id));
        }

        [TestMethod]
        public void OnReceiverDataReceived_MicGainNotPresent_ShouldNotProcess()
        {
            //ARRANGE
            var repositoryMock = new Mock<IReceiverRepository>();

            var connectionServiceMock = new Mock<IReceiverConnectionService>();

#pragma warning disable S1481 // Unused local variables should be removed (will react to event)
            var receiverService = new ReceiverService(repositoryMock.Object, connectionServiceMock.Object, logger.Object);
#pragma warning restore S1481 // Unused local variables should be removed

            var action = new Action(() => connectionServiceMock.Raise(c => c.MessageReceived += null, new Receiver(), new MessageReceivedEventArgs(new ReceiverSocketMessage())));

            //ACT / ASSERT
            action.Should().NotThrow();
            repositoryMock.Verify(r => r.Update(It.IsAny<ReceiverEntity>()), Times.Never);
        }

        [TestMethod]
        public void OnReceiverDataReceived_MicGainNotChanged_ShouldNotUpdateDb()
        {
            //ARRANGE
            var repositoryMock = new Mock<IReceiverRepository>();

            var connectionServiceMock = new Mock<IReceiverConnectionService>();

            var testReceiver = new Receiver();

            var message = new ReceiverSocketMessage
            {
                Properties =
                {
                    { "micGain", testReceiver.Microphone.MicGain.ToString() }
                }
            };

            var action = new Action(() => connectionServiceMock.Raise(c => c.MessageReceived += null, testReceiver, new MessageReceivedEventArgs(message)));

            //ACT / ASSERT
            action.Should().NotThrow();
            repositoryMock.Verify(r => r.Update(It.IsAny<ReceiverEntity>()), Times.Never);
        }

        [TestMethod]
        public void OnReceiverDataReceived_MicGainChanged_ShouldUpdateDb()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            ReceiverEntity receiverEntity = testData.ReceiverEntities[0];

            var repositoryMock = new Mock<IReceiverRepository>();
            repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(testData.ReceiverEntities);
            repositoryMock.Setup(r => r.Update(It.IsAny<ReceiverEntity>())).ReturnsAsync(receiverEntity);

            var connectionServiceMock = new Mock<IReceiverConnectionService>();

            var testReceiver = testData.Receivers[0];

            var message = new ReceiverSocketMessage
            {
                Properties =
                {
                    { "micGain", "51" }
                }
            };

#pragma warning disable S1481 // Unused local variables should be removed (will react to event)
            var receiverService = new ReceiverService(repositoryMock.Object, connectionServiceMock.Object, logger.Object);
#pragma warning restore S1481 // Unused local variables should be removed

            var action = new Action(() => connectionServiceMock.Raise(c => c.MessageReceived += null, testReceiver, new MessageReceivedEventArgs(message)));

            //ACT / ASSERT
            action.Should().NotThrow();
            repositoryMock.Verify(r => r.Update(It.IsAny<ReceiverEntity>()), Times.Once);
            testReceiver.Microphone.MicGain.Should().Be(51);
        }

        [TestMethod]
        public void OnReceiverDataReceived_DbBlocked_ShouldRetryAndThrow()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var repositoryMock = new Mock<IReceiverRepository>();
            repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(testData.ReceiverEntities);
            repositoryMock.Setup(r => r.Update(It.IsAny<ReceiverEntity>())).Throws(new Exception());

            var connectionServiceMock = new Mock<IReceiverConnectionService>();

            var testReceiver = testData.Receivers[0];

            var message = new ReceiverSocketMessage
            {
                Properties =
                {
                    { "micGain", "51" }
                }
            };

#pragma warning disable S1481 // Unused local variables should be removed (will react to event)
            var receiverService = new ReceiverService(repositoryMock.Object, connectionServiceMock.Object, logger.Object);
#pragma warning restore S1481 // Unused local variables should be removed

            var action = new Action(() => connectionServiceMock.Raise(c => c.MessageReceived += null, testReceiver, new MessageReceivedEventArgs(message)));

            //ACT / ASSERT
            action.Should().Throw<DbUpdateConcurrencyException>();
            repositoryMock.Verify(r => r.Update(It.IsAny<ReceiverEntity>()), Times.Exactly(3));
        }

        [TestMethod]
        public async Task Connect_Always_ShouldCallConnectOnConnectionService()
        {
            //ARRANGE
            var testData = new ReceiverTestData();
            var testReceiver = testData.Receivers[0];

            var repositoryMock = new Mock<IReceiverRepository>();

            var connectionServiceMock = new Mock<IReceiverConnectionService>();

            var receiverServiceMock = new ReceiverService(repositoryMock.Object, connectionServiceMock.Object, logger.Object);

            //ACT
            await receiverServiceMock.Connect(testReceiver);

            //ASSERT
            connectionServiceMock.Verify(c => c.Open(testReceiver), Times.Once);
        }

        [TestMethod]
        public void Disconnect_Always_ShouldCallDisconnectOnConnectionService()
        {
            //ARRANGE
            var testData = new ReceiverTestData();
            var testReceiver = testData.Receivers[0];

            var repositoryMock = new Mock<IReceiverRepository>();

            var connectionServiceMock = new Mock<IReceiverConnectionService>();

            var receiverServiceMock = new ReceiverService(repositoryMock.Object, connectionServiceMock.Object, logger.Object);

            //ACT
            receiverServiceMock.Disconnect(testReceiver.Id);

            //ASSERT
            connectionServiceMock.Verify(c => c.Close(testReceiver.Id), Times.Once);
        }

        [TestMethod]
        public async Task Create_DuplicateName_ShouldThrowException()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var repositoryMock = new Mock<IReceiverRepository>();
            repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(testData.ReceiverEntities);

            var connectionServiceMock = new Mock<IReceiverConnectionService>();

            var receiverService = new ReceiverService(repositoryMock.Object, connectionServiceMock.Object, logger.Object);

            Func<Task<IReceiver>> action = () => receiverService.Create(testData.ReceiverEntities[0].Name, "123.123.123.123", null);

            //ACT / ASSERT
            repositoryMock.Verify(r => r.Add(It.IsAny<ReceiverEntity>()), Times.Never);
            receiverService.GetAll().Should().HaveCount(testData.ReceiverEntities.Count);
            await action.Should().ThrowAsync<DuplicateNameException>();
        }

        [TestMethod]
        public async Task Create_DuplicateAddressableHost_ShouldThrowException()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var repositoryMock = new Mock<IReceiverRepository>();
            repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(testData.ReceiverEntities);

            var connectionServiceMock = new Mock<IReceiverConnectionService>();

            var receiverService = new ReceiverService(repositoryMock.Object, connectionServiceMock.Object, logger.Object);

            Func<Task<IReceiver>> action = () => receiverService.Create(Guid.NewGuid().ToString(), testData.ReceiverEntities[0].Host, testData.ReceiverEntities[0].Port);

            //ACT / ASSERT
            repositoryMock.Verify(r => r.Add(It.IsAny<ReceiverEntity>()), Times.Never);
            receiverService.GetAll().Should().HaveCount(testData.ReceiverEntities.Count);
            await action.Should().ThrowAsync<DuplicateNameException>();
        }

        [TestMethod]
        public async Task Create_ValidArguments_ShouldCreateReceiver()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var name = Guid.NewGuid().ToString();
            var host = "localhost";
            var port = 12345;

            var repositoryMock = new Mock<IReceiverRepository>();
            repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(testData.ReceiverEntities);

            var connectionServiceMock = new Mock<IReceiverConnectionService>();

            var receiverService = new ReceiverService(repositoryMock.Object, connectionServiceMock.Object, logger.Object);

            //ACT
            var receiver = await receiverService.Create(name, host, port);

            //ASSERT
            receiver.Name.Should().Be(name);
            receiver.Host.Should().Be(host);
            receiver.Port.Should().Be(port);
            receiver.Id.Should().NotBeNullOrEmpty();
            receiver.Microphone.Id.Should().NotBeNullOrEmpty();

            repositoryMock.Verify(r => r.Add(It.IsAny<ReceiverEntity>()), Times.Once);
            receiverService.GetAll().Should().HaveCount(testData.ReceiverEntities.Count + 1);
        }

        [TestMethod]
        public async Task Delete_Always_ShouldDeleteReceiver()
        {
            //ARRANGE
            var testData = new ReceiverTestData();
            var testReceiver = testData.Receivers[0];

            var repositoryMock = new Mock<IReceiverRepository>();
            repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(testData.ReceiverEntities);

            var connectionServiceMock = new Mock<IReceiverConnectionService>();

            var receiverService = new ReceiverService(repositoryMock.Object, connectionServiceMock.Object, logger.Object);

            //ACT
            await receiverService.Delete(testReceiver.Id);

            //ASSERT
            repositoryMock.Verify(r => r.Delete(testReceiver.Id), Times.Once);
            receiverService.GetAll().Should().NotContain(r => r.Id == testReceiver.Id);
        }

        [TestMethod]
        public async Task Delete_ReceiverConnected_ShouldCloseConnection()
        {
            //ARRANGE
            var testData = new ReceiverTestData();
            var testReceiver = testData.Receivers[0];

            var repositoryMock = new Mock<IReceiverRepository>();
            repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(testData.ReceiverEntities);

            var connectionServiceMock = new Mock<IReceiverConnectionService>();
            connectionServiceMock.Setup(c => c.IsConnected(testReceiver.Id)).Returns(true);

            var receiverService = new ReceiverService(repositoryMock.Object, connectionServiceMock.Object, logger.Object);

            //ACT
            await receiverService.Delete(testReceiver.Id);

            //ASSERT
            connectionServiceMock.Verify(c => c.Close(testReceiver.Id), Times.Once);
        }

        [TestMethod]
        public void Get_Existing_ShouldReturnReceiver()
        {
            //ARRANGE
            var testData = new ReceiverTestData();
            var testReceiver = testData.Receivers[0];

            var repositoryMock = new Mock<IReceiverRepository>();
            repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(testData.ReceiverEntities);

            var connectionServiceMock = new Mock<IReceiverConnectionService>();

            var receiverService = new ReceiverService(repositoryMock.Object, connectionServiceMock.Object, logger.Object);

            //ACT
            var receiver = receiverService.Get(testReceiver.Id);

            //ASSERT
            receiver?.Id.Should().Be(testReceiver.Id);
        }

        [TestMethod]
        public void Get_Existing_ShouldReturnNull()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var repositoryMock = new Mock<IReceiverRepository>();
            repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(testData.ReceiverEntities);

            var connectionServiceMock = new Mock<IReceiverConnectionService>();

            var receiverService = new ReceiverService(repositoryMock.Object, connectionServiceMock.Object, logger.Object);

            //ACT
            var receiver = receiverService.Get(Guid.NewGuid().ToString());

            //ASSERT
            receiver?.Should().BeNull();
        }

        [TestMethod]
        public void GetAll_Always_ShouldReturnReceivers()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var repositoryMock = new Mock<IReceiverRepository>();
            repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(testData.ReceiverEntities);

            var connectionServiceMock = new Mock<IReceiverConnectionService>();

            var receiverService = new ReceiverService(repositoryMock.Object, connectionServiceMock.Object, logger.Object);

            //ACT
            var receivers = receiverService.GetAll();

            //ASSERT
            receivers.Should().HaveCount(testData.ReceiverEntities.Count);
        }

        [TestMethod]
        public async Task Update_Always_ShouldUpdateRepoAndModel()
        {
            //ARRANGE
            ReceiverEntity? repoReceiverEntity = null;

            var testData = new ReceiverTestData();

            var targetReceiver = testData.Receivers[0];

            var updatedReceiver = new Receiver
            {
                Id = targetReceiver.Id,
                Name = targetReceiver.Name + "2",
                Host = targetReceiver.Host,
                Port = targetReceiver.Port
            };

            var updatedEntity = new ReceiverEntity
            {
                Id = updatedReceiver.Id,
                Name = updatedReceiver.Name,
                Host = updatedReceiver.Host,
                Port = updatedReceiver.Port,
                Microphone = new MicrophoneEntity()
            };

            var repositoryMock = new Mock<IReceiverRepository>();
            repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(testData.ReceiverEntities);
            repositoryMock.Setup(r => r.Update(It.IsAny<ReceiverEntity>())).Callback<ReceiverEntity>(r => repoReceiverEntity = r).ReturnsAsync(updatedEntity);

            var connectionServiceMock = new Mock<IReceiverConnectionService>();

            var receiverService = new ReceiverService(repositoryMock.Object, connectionServiceMock.Object, logger.Object);

            //ACT
            var returnedReceiver = await receiverService.Update(updatedReceiver);
            var modelReceiver = receiverService.Get(updatedReceiver.Id);

            //ASSERT
            returnedReceiver.Id.Should().Be(targetReceiver.Id);
            returnedReceiver.Name.Should().Be(updatedReceiver.Name);

            modelReceiver?.Name.Should().Be(updatedReceiver.Name);

            repositoryMock.Verify(r => r.Update(It.IsAny<ReceiverEntity>()), Times.Once);
            repoReceiverEntity?.Id.Should().Be(targetReceiver.Id);
            repoReceiverEntity?.Name.Should().Be(updatedReceiver.Name);
        }

        [TestMethod]
        public async Task Update_ConnectedReceiverValue_ShouldSendState()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var targetReceiver = testData.Receivers[0];

            var updatedReceiver = new Receiver
            {
                Id = targetReceiver.Id,
                Name = targetReceiver.Name + "2",
                Host = targetReceiver.Host,
                Port = targetReceiver.Port
            };

            var updatedEntity = new ReceiverEntity
            {
                Id = updatedReceiver.Id,
                Name = updatedReceiver.Name,
                Host = updatedReceiver.Host,
                Port = updatedReceiver.Port,
                Microphone = new MicrophoneEntity()
            };

            var repositoryMock = new Mock<IReceiverRepository>();
            repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(testData.ReceiverEntities);
            repositoryMock.Setup(r => r.Update(It.IsAny<ReceiverEntity>())).ReturnsAsync(updatedEntity);

            var connectionServiceMock = new Mock<IReceiverConnectionService>();
            connectionServiceMock.Setup(c => c.IsConnected(updatedReceiver.Id)).Returns(true);

            var receiverService = new ReceiverService(repositoryMock.Object, connectionServiceMock.Object, logger.Object);

            //ACT
            await receiverService.Update(updatedReceiver);
            receiverService.Get(updatedReceiver.Id);

            //ASSERT
            connectionServiceMock.Verify(c => c.UpdateReceiverState(updatedReceiver), Times.Once);
        }

        [TestMethod]
        public async Task Update_DuplicateNameOrAddressableHost_ShouldThrowException()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var targetReceiver = testData.Receivers[0];
            testData.ReceiverEntities[1].Name = targetReceiver.Name + "2";
            testData.ReceiverEntities[2].Host = targetReceiver.Host;
            testData.ReceiverEntities[2].Port = targetReceiver.Port + 1;

            var updatedReceiverName = new Receiver
            {
                Id = targetReceiver.Id,
                Name = targetReceiver.Name,
                Host = targetReceiver.Host,
                Port = targetReceiver.Port + 1
            };

            var updatedReceiverAddressableHost = new Receiver
            {
                Id = targetReceiver.Id,
                Name = targetReceiver.Name + "2",
                Host = targetReceiver.Host,
                Port = targetReceiver.Port
            };

            var updatedEntity = new ReceiverEntity
            {
                Id = updatedReceiverName.Id,
                Name = updatedReceiverName.Name,
                Host = updatedReceiverName.Host,
                Port = updatedReceiverName.Port,
                Microphone = new MicrophoneEntity()
            };

            var repositoryMock = new Mock<IReceiverRepository>();
            repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(testData.ReceiverEntities);
            repositoryMock.Setup(r => r.Update(It.IsAny<ReceiverEntity>())).ReturnsAsync(updatedEntity);

            var connectionServiceMock = new Mock<IReceiverConnectionService>();

            var receiverService = new ReceiverService(repositoryMock.Object, connectionServiceMock.Object, logger.Object);

            Func<Task<IReceiver>> actionName = async () => await receiverService.Update(updatedReceiverName);
            Func<Task<IReceiver>> actionAddressableHost = async () => await receiverService.Update(updatedReceiverAddressableHost);

            //ACT / ASSERT
            await actionName.Should().ThrowAsync<DuplicateNameException>();
            await actionAddressableHost.Should().ThrowAsync<DuplicateNameException>();
        }

        [TestMethod]
        public async Task Update_AddressableHostUpdated_ShouldDisconnect()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var targetReceiver = testData.Receivers[0];

            var updatedReceiver = new Receiver
            {
                Id = targetReceiver.Id,
                Name = targetReceiver.Name,
                Host = targetReceiver.Host,
                Port = targetReceiver.Port + 1
            };

            var updatedEntity = new ReceiverEntity
            {
                Id = updatedReceiver.Id,
                Name = updatedReceiver.Name,
                Host = updatedReceiver.Host,
                Port = updatedReceiver.Port,
                Microphone = new MicrophoneEntity()
            };

            var repositoryMock = new Mock<IReceiverRepository>();
            repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(testData.ReceiverEntities);
            repositoryMock.Setup(r => r.Update(It.IsAny<ReceiverEntity>())).ReturnsAsync(updatedEntity);

            var connectionServiceMock = new Mock<IReceiverConnectionService>();
            connectionServiceMock.Setup(c => c.IsConnected(updatedReceiver.Id)).Returns(true);

            var receiverService = new ReceiverService(repositoryMock.Object, connectionServiceMock.Object, logger.Object);

            //ACT
            await receiverService.Update(updatedReceiver);

            //ASSERT
            connectionServiceMock.Verify(c => c.Close(updatedReceiver.Id), Times.Once);
        }

        [TestMethod]
        public void Update_WhenLocked_ShouldThrowException()
        {
            //ARRANGE / ASSERT
            var testData = new ReceiverTestData();

            var targetReceiver = testData.Receivers[0];

            var updatedReceiver = new Receiver
            {
                Id = targetReceiver.Id,
                Name = targetReceiver.Name,
                Host = targetReceiver.Host,
                Port = targetReceiver.Port + 1
            };

            var updatedEntity = new ReceiverEntity
            {
                Id = updatedReceiver.Id,
                Name = updatedReceiver.Name,
                Host = updatedReceiver.Host,
                Port = updatedReceiver.Port,
                Microphone = new MicrophoneEntity()
            };

            var message = new ReceiverSocketMessage
            {
                Properties =
                {
                    { "micGain", "10" }
                }
            };

            var messageEventArgs = new MessageReceivedEventArgs(message);

            var repositoryMock = new Mock<IReceiverRepository>();
            repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(testData.ReceiverEntities);

            var connectionServiceMock = new Mock<IReceiverConnectionService>();
            connectionServiceMock.Setup(c => c.IsConnected(updatedReceiver.Id)).Returns(true);

            var receiverService = new ReceiverService(repositoryMock.Object, connectionServiceMock.Object, logger.Object);

            Func<Task<IReceiver>> action = async () => await receiverService.Update(updatedReceiver);

            repositoryMock.Setup(r => r.Update(It.IsAny<ReceiverEntity>())).Callback(async () =>
            {
                await action.Should().ThrowAsync<DbUpdateConcurrencyException>();
            }).ReturnsAsync(updatedEntity);

            //ACT
            connectionServiceMock.Raise(c => c.MessageReceived += null, targetReceiver, messageEventArgs);
        }

        [TestMethod]
        public void IsConnected_Always_ShouldCallConnectionService()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var testReceiver = testData.Receivers[0];

            var repositoryMock = new Mock<IReceiverRepository>();
            repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(testData.ReceiverEntities);

            var connectionServiceMock = new Mock<IReceiverConnectionService>();
            connectionServiceMock.Setup(c => c.IsConnected(testReceiver.Id)).Returns(true);

            var receiverService = new ReceiverService(repositoryMock.Object, connectionServiceMock.Object, logger.Object);

            //ACT
            var result = receiverService.IsConnected(testReceiver.Id);

            //ASSERT
            connectionServiceMock.Verify(c => c.IsConnected(testReceiver.Id), Times.Once);
            result.Should().BeTrue();
        }
    }
}
