using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SennheiserBackend.Controllers;
using SennheiserBackend.Models;
using SennheiserBackend.Services;
using SennheiserBackend.Tests.TestData;
using System.Data;

namespace SennheiserBackend.Tests.UnitTests.ControllerTests
{
    [TestClass]
    public class ReceiverControllerTest
    {
        private readonly List<Receiver> testReceivers;
        private readonly Mock<ILogger<ReceiverController>> loggerMock = new();

        public ReceiverControllerTest()
        {
            testReceivers = new ReceiverTestData().Receivers;
        }


        [TestMethod]
        public void Get_Single_ReturnsReceiver()
        {
            //ARRANGE
            var testReceiver = testReceivers[1];

            var receiverServiceMock = new Mock<IReceiverService>();
            receiverServiceMock.Setup(s => s.Get(testReceiver.Id)).Returns(() => testReceiver);

            var receiverController = new ReceiverController(receiverServiceMock.Object, loggerMock.Object);

            //ACT
            var result = receiverController.Get(testReceiver.Id);

            //ASSERT
            receiverServiceMock.Verify(r => r.Get(testReceiver.Id), Times.Once);

            result.Should().BeOfType<ObjectResult>();
            ((result as ObjectResult)?.Value as Receiver)?.Id.Should().Be(testReceiver.Id);
        }

        [TestMethod]
        public void Get_NonExistent_ReturnsNotFoundResult()
        {
            //ARRANGE
            var testReceiver = testReceivers[1];

            var receiverServiceMock = new Mock<IReceiverService>();
            receiverServiceMock.Setup(s => s.Get(It.IsAny<string>())).Returns(() => null);

            var receiverController = new ReceiverController(receiverServiceMock.Object, loggerMock.Object);

            //ACT
            var result = receiverController.Get(testReceiver.Id);

            //ASSERT
            receiverServiceMock.Verify(r => r.Get(testReceiver.Id), Times.Once);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [TestMethod]
        public async Task Create_ValidRequest_ShouldReturnNewReceiver()
        {
            //ARRANGE
            var testReceiver = testReceivers[1];

            var receiverServiceMock = new Mock<IReceiverService>();
            receiverServiceMock.Setup(s => s.Create(testReceiver.Name, testReceiver.Host, testReceiver.Port)).ReturnsAsync(() => testReceiver);

            var httpRequest = new Mock<HttpRequest>();
            var httpContext = new Mock<HttpContext>();
            var controllerContext = new ControllerContext();
            httpContext.SetupGet(c => c.Request).Returns(httpRequest.Object);
            controllerContext.HttpContext = httpContext.Object;

            var receiverController = new ReceiverController(receiverServiceMock.Object, loggerMock.Object)
            {
                ControllerContext = controllerContext
            };


            var createRequest = new ReceiverCreateRequest
            {
                Name = testReceiver.Name,
                Host = testReceiver.Host,
                Port = testReceiver.Port
            };


            //ACT
            var result = await receiverController.Create(createRequest);

            //ASSERT
            receiverServiceMock.Verify(r => r.Create(createRequest.Name, createRequest.Host, createRequest.Port), Times.Once);

            result.Should().BeOfType<CreatedResult>();
            (result as CreatedResult)?.Value.Should().Be(testReceiver);
        }

        [TestMethod]
        public async Task Create_BadRequest_ShouldReturnBadRequestResult()
        {
            //ARRANGE
            var testReceiver = testReceivers[1];

            var receiverServiceMock = new Mock<IReceiverService>();
            receiverServiceMock.Setup(s => s.Create(testReceiver.Name, testReceiver.Host, testReceiver.Port)).ReturnsAsync(() => testReceiver);

            var httpRequest = new Mock<HttpRequest>();
            var httpContext = new Mock<HttpContext>();
            var controllerContext = new ControllerContext();
            httpContext.SetupGet(c => c.Request).Returns(httpRequest.Object);
            controllerContext.HttpContext = httpContext.Object;

            var receiverController = new ReceiverController(receiverServiceMock.Object, loggerMock.Object)
            {
                ControllerContext = controllerContext
            };


            var createRequestNoName = new ReceiverCreateRequest
            {
                Name = "",
                Host = "192.168.0.1",
                Port = 12345,
            };

            var createRequestNoHost = new ReceiverCreateRequest
            {
                Name = "TestName",
                Host = ""
            };

            //ACT
            var resultNoName = await receiverController.Create(createRequestNoName);
            var resultNoHost = await receiverController.Create(createRequestNoHost);


            //ASSERT
            receiverServiceMock.Verify(r => r.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Never);

            resultNoName.Should().BeOfType<BadRequestObjectResult>();
            resultNoHost.Should().BeOfType<BadRequestObjectResult>();
        }

        [TestMethod]
        public async Task Delete_ReceiverNotExistent_ShouldNotCallDelete()
        {
            //ARRANGE
            var testReceiver = testReceivers[1];

            var receiverServiceMock = new Mock<IReceiverService>();
            receiverServiceMock.Setup(r => r.Delete(It.IsAny<string>())).Throws(() => new NullReferenceException());

            var receiverController = new ReceiverController(receiverServiceMock.Object, loggerMock.Object);

            Func<Task> action = async () => await receiverController.Delete(testReceiver.Id);

            //ACT / ASSERT
            await action.Should().ThrowAsync<NullReferenceException>();
        }

        [TestMethod]
        public async Task Delete_ValidRequest_ShouldDeleteAndReturnNoContentResult()
        {
            //ARRANGE
            var testReceiver = testReceivers[1];

            var receiverServiceMock = new Mock<IReceiverService>();

            var receiverController = new ReceiverController(receiverServiceMock.Object, loggerMock.Object);

            //ACT
            var result = await receiverController.Delete(testReceiver.Id);

            //ASSERT
            receiverServiceMock.Verify(r => r.Delete(testReceiver.Id), Times.Once);

            result.Should().BeOfType<NoContentResult>();
        }

        [TestMethod]
        public void GetAll_Request_ShouldReturnList()
        {
            //ARRANG

            var receiverServiceMock = new Mock<IReceiverService>();
            receiverServiceMock.Setup(s => s.GetAll()).Returns(() => testReceivers);

            var receiverController = new ReceiverController(receiverServiceMock.Object, loggerMock.Object);

            //ACT
            var result = receiverController.GetAll();

            //ASSERT
            result.Should().BeOfType<ObjectResult>();
            (result as ObjectResult)?.Value.Should().Be(testReceivers);
        }

        [TestMethod]
        public void GetAll_NoEntries_ShouldReturnEmptyList()
        {
            //ARRANG

            var receiverServiceMock = new Mock<IReceiverService>();
            receiverServiceMock.Setup(s => s.GetAll()).Returns(() => new List<Receiver>());

            var receiverController = new ReceiverController(receiverServiceMock.Object, loggerMock.Object);

            //ACT
            var result = receiverController.GetAll();

            //ASSERT
            receiverServiceMock.Verify(r => r.GetAll(), Times.Once);

            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult?.Value.Should().BeOfType<List<Receiver>>();
            (objectResult?.Value as List<Receiver>).Should().BeEmpty();
        }

        [TestMethod]
        public async Task Patch_ReceiverNotExistent_ShouldReturnNotFoundResult()
        {
            //ARRANGE
            var testReceiver = testReceivers[1];

            var receiverServiceMock = new Mock<IReceiverService>();
            receiverServiceMock.Setup(r => r.Get(It.IsAny<string>())).Returns(() => null);

            var receiverController = new ReceiverController(receiverServiceMock.Object, loggerMock.Object);

            var patchDocument = new JsonPatchDocument<Receiver>
            {
                Operations = { new Operation<Receiver>("add", "/MicGain", "") }
            };

            //ACT
            var result = await receiverController.Patch(patchDocument, testReceiver.Id);

            //ASSERT
            receiverServiceMock.Verify(r => r.Get(testReceiver.Id), Times.Once);
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [TestMethod]
        public async Task Patch_ValidRequest_ShouldApplyChanges()
        {
            //ARRANGE
            var testReceiver = testReceivers[1];
            int newMicGain = testReceiver.Microphone.MicGain + 3;

            var receiverServiceMock = new Mock<IReceiverService>();
            receiverServiceMock.Setup(r => r.Get(testReceiver.Id)).Returns(() => testReceiver);
            receiverServiceMock.Setup(r => r.Update(testReceiver)).ReturnsAsync(() => testReceiver);

            var receiverController = new ReceiverController(receiverServiceMock.Object, loggerMock.Object);

            var patchDocument = new JsonPatchDocument<Receiver>
            {
                Operations = { new Operation<Receiver>("add", "/Microphone/MicGain", "", newMicGain) }
            };

            //ACT
            var result = await receiverController.Patch(patchDocument, testReceiver.Id);

            //ASSERT
            receiverServiceMock.Verify(r => r.Update(testReceiver), Times.Once);

            result.Should().BeOfType<OkObjectResult>();
            var okObjectResult = (OkObjectResult)result;
            okObjectResult?.Value.Should().BeAssignableTo<IReceiver>();
            var receiver = okObjectResult?.Value as IReceiver;
            receiver?.Microphone.MicGain.Should().Be(newMicGain);
        }

        [TestMethod]
        public async Task Patch_BadArgument_ShouldThrowException()
        {
            //ARRANGE
            var microphoneMock = new Mock<Microphone>();
            microphoneMock.SetupSet(r => r.MicGain = It.IsAny<int>()).Throws<ArgumentException>();

            var testReceiver = new Receiver
            {
                Microphone = microphoneMock.Object
            };

            var receiverServiceMock = new Mock<IReceiverService>();
            receiverServiceMock.Setup(r => r.Get(testReceiver.Id)).Returns(() => testReceiver);
            receiverServiceMock.Setup(r => r.Update(testReceiver)).ReturnsAsync(() => testReceiver);

            var receiverController = new ReceiverController(receiverServiceMock.Object, loggerMock.Object);

            var patchDocumentMin = new JsonPatchDocument<Receiver>
            {
                Operations = { new Operation<Receiver>("add", "/Microphone/MicGain", "", 100) }
            };

            Func<Task<IActionResult>> action = async () => await receiverController.Patch(patchDocumentMin, testReceiver.Id);

            //ACT / ASSERT
            await action.Should().ThrowAsync<JsonSerializationException>();
            receiverServiceMock.Verify(r => r.Update(It.IsAny<Receiver>()), Times.Never);
        }

        [TestMethod]
        public async Task Patch_DuplicateKey_ShouldThrowException()
        {
            //ARRANGE
            var microphoneMock = new Mock<Microphone>();

            var testReceiver = new Receiver
            {
                Microphone = microphoneMock.Object
            };

            var receiverServiceMock = new Mock<IReceiverService>();
            receiverServiceMock.Setup(r => r.Get(testReceiver.Id)).Returns(() => testReceiver);
            receiverServiceMock.Setup(r => r.Update(testReceiver)).Throws<DuplicateNameException>(() => new DuplicateNameException());

            var receiverController = new ReceiverController(receiverServiceMock.Object, loggerMock.Object);

            var patchDocumentMin = new JsonPatchDocument<Receiver>
            {
                Operations = { new Operation<Receiver>("add", "/Microphone/MicGain", "", 100) }
            };

            Func<Task<IActionResult>> action = async () => await receiverController.Patch(patchDocumentMin, testReceiver.Id);

            //ACT / ASSERT
            await action.Should().ThrowAsync<DuplicateNameException>();
        }

        [TestMethod]
        public async Task Patch_OtherException_ShouldRethrow()
        {
            //ARRANGE
            var microphoneMock = new Mock<Microphone>();
            var exception = new Exception();
            microphoneMock.SetupSet(m => m.MicGain = It.IsAny<int>()).Throws(() => exception);

            var testReceiver = new Receiver
            {
                Microphone = microphoneMock.Object
            };

            var receiverServiceMock = new Mock<IReceiverService>();
            receiverServiceMock.Setup(r => r.Get(testReceiver.Id)).Returns(() => testReceiver);
            receiverServiceMock.Setup(r => r.Update(testReceiver)).ReturnsAsync(() => testReceiver);

            var receiverController = new ReceiverController(receiverServiceMock.Object, loggerMock.Object);

            var patchDocumentMin = new JsonPatchDocument<Receiver>
            {
                Operations = { new Operation<Receiver>("add", "/Microphone/MicGain", "", 50) }
            };

            Func<Task<IActionResult>> action = async () => await receiverController.Patch(patchDocumentMin, testReceiver.Id);

            //ACT / ASSERT
            var exceptionAssertion = await action.Should().ThrowAsync<Exception>();
            exceptionAssertion.Which.InnerException.Should().Be(exception);
        }

        [TestMethod]
        public async Task OpenConnection_ValidRequest_ShouldReturnCreatedResult()
        {
            //ARRANGE
            var testReceiver = new Receiver();

            var receiverServiceMock = new Mock<IReceiverService>();
            receiverServiceMock.Setup(r => r.Get(testReceiver.Id)).Returns(() => testReceiver);

            var receiverController = new ReceiverController(receiverServiceMock.Object, loggerMock.Object);

            //ACT
            var result = await receiverController.OpenConnection(testReceiver.Id);

            //ASSERT
            result.Should().BeOfType<CreatedResult>();
        }

        [TestMethod]
        public async Task OpenConnection_NonExistentReceiver_ShouldReturnNotFoundResult()
        {
            //ARRANGE
            var testReceiver = new Receiver();

            var receiverServiceMock = new Mock<IReceiverService>();
            receiverServiceMock.Setup(r => r.Get(testReceiver.Id)).Returns(() => null);

            var receiverController = new ReceiverController(receiverServiceMock.Object, loggerMock.Object);

            //ACT
            var result = await receiverController.OpenConnection(testReceiver.Id);

            //ASSERT
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [TestMethod]
        public async Task OpenConnection_ConnectionFailed_ShouldThrowException()
        {
            //ARRANGE
            var testReceiver = new Receiver();

            var receiverServiceMock = new Mock<IReceiverService>();
            receiverServiceMock.Setup(r => r.Get(testReceiver.Id)).Returns(() => testReceiver);
            receiverServiceMock.Setup(r => r.Connect(testReceiver)).Throws(() => new Exception());

            var receiverController = new ReceiverController(receiverServiceMock.Object, loggerMock.Object);

            Func<Task> action = async () => await receiverController.OpenConnection(testReceiver.Id);

            //ACT / ASSERT
            await action.Should().ThrowAsync<Exception>();
        }

        [TestMethod]
        public void CloseConnection_ValidRequest_ShouldReturnNoContentResult()
        {
            //ARRANGE
            var testReceiver = new Receiver();

            var receiverServiceMock = new Mock<IReceiverService>();

            var receiverController = new ReceiverController(receiverServiceMock.Object, loggerMock.Object);

            //ACT
            var result = receiverController.CloseConnection(testReceiver.Id);

            //ASSERT
            result.Should().BeOfType<NoContentResult>();
        }

        [TestMethod]
        public void CloseConnection_DisconnectFailed_ShouldRethrow()
        {
            //ARRANGE
            var testReceiver = new Receiver();

            var receiverServiceMock = new Mock<IReceiverService>();
            receiverServiceMock.Setup(r => r.Disconnect(testReceiver.Id)).Throws(() => new Exception());

            var receiverController = new ReceiverController(receiverServiceMock.Object, loggerMock.Object);

            var action = new Action(() => receiverController.CloseConnection(testReceiver.Id));

            //ACT / ASSERT
            action.Should().Throw<Exception>();
        }

        [TestMethod]
        public void GetConnectionInfo_Always_ShouldReturnConnectionInfo()
        {
            //ARRANGE
            var testReceiver = new Receiver();

            var receiverServiceMock = new Mock<IReceiverService>();
            receiverServiceMock.Setup(r => r.IsConnected(testReceiver.Id)).Returns(true);

            var receiverController = new ReceiverController(receiverServiceMock.Object, loggerMock.Object);

            //ACT
            var result = receiverController.GetConnectionInfo(testReceiver.Id);

            //ASSERT
            result.Should().BeOfType<OkObjectResult>();

            var objectResult = result as OkObjectResult;
            objectResult?.Value.Should().BeOfType<ReceiverConnectionInfo>();

            var connectionInfo = objectResult?.Value as ReceiverConnectionInfo;
            connectionInfo?.IsConnected.Should().BeTrue();
        }
    }
}