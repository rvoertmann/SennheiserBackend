using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SennheiserBackend.Models;
using SennheiserBackend.Services;
using SennheiserBackend.Tests.TestData;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace SennheiserBackend.Tests.UnitTests.ServiceTests
{
    [TestClass]
    public class ReceiverClientTest
    {
        private void ConfigureVerificationMessage(Mock<IReceiverClientWebSocket> webSocketMock, IReceiver testReceiver, int iterationBeforeClose,  WebSocketReceiveResult receiveResult, Action<ArraySegment<byte>, CancellationToken> nextCallback)
        {
            var iterations = 0;

            var message = new ReceiverSocketMessage
            {
                Properties =
                {
                    { "micGain", "53" },
                    { "name", testReceiver.Name }
                }
            };
            var messageJson = JsonConvert.SerializeObject(message);
            var messageBytes = new ArraySegment<byte>(Encoding.ASCII.GetBytes(messageJson));

            var verificationResult = new WebSocketReceiveResult(messageBytes.Count, WebSocketMessageType.Text, true);

            webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.None);
            webSocketMock.Setup(w => w.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).Callback(() => webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.Open));
            webSocketMock.Setup(w => w.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>())).ReturnsAsync(verificationResult).Callback((ArraySegment<byte> s, CancellationToken t) =>
            {
                messageBytes.CopyTo(s);
                webSocketMock.Setup(w => w.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>())).Callback<ArraySegment<byte>, CancellationToken>(nextCallback).ReturnsAsync(receiveResult);
                if (iterations++ == iterationBeforeClose)
                {
                    webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.Closed);
                }
            });
        }

        [TestMethod]
        public async Task Connect_NotOpen_ShouldConnectWebSocketAndBeginListen()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var testReceiver = testData.Receivers[0];

            var message = new ReceiverSocketMessage
            {
                Properties =
                {
                    { "micGain", "53" },
                    { "name", testReceiver.Name }
                }
            };
            var messageJson = JsonConvert.SerializeObject(message);
            var messageBytes = new ArraySegment<byte>(Encoding.ASCII.GetBytes(messageJson));

            var receiveResult = new WebSocketReceiveResult(messageBytes.Count, WebSocketMessageType.Text, true);

            var webSocketMock = new Mock<IReceiverClientWebSocket>();
            ConfigureVerificationMessage(webSocketMock, testReceiver, 2, receiveResult, (s, t) => 
            {
                webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.Closed);
            });

            var loggerMock = new Mock<ILogger<ReceiverClient>>();

            var webSocketFactoryMock = new Mock<IWebSocketFactory>();
            webSocketFactoryMock.Setup(w => w.Create()).Returns(webSocketMock.Object);

            var receiverClient = new ReceiverClient(testReceiver, webSocketFactoryMock.Object, loggerMock.Object);

            //ACT
            await receiverClient.Connect();

            //ASSERT
            webSocketMock.Verify(w => w.ConnectAsync(new Uri($"ws://{testReceiver.AddressableHost}"), It.IsAny<CancellationToken>()), Times.Once);
            webSocketMock.Verify(w => w.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task Connect_ReceiveValidMessage_ShouldInvokeEvent()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var testReceiver = testData.Receivers[0];

            var message = new ReceiverSocketMessage
            {
                Properties =
                {
                    { "micGain", "53" }
                }
            };
            var messageJson = JsonConvert.SerializeObject(message);
            var messageBytes = new ArraySegment<byte>(Encoding.ASCII.GetBytes(messageJson));

            var receiveResult = new WebSocketReceiveResult(messageBytes.Count, WebSocketMessageType.Text, true);

            var webSocketMock = new Mock<IReceiverClientWebSocket>();
            webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.None);
            webSocketMock.Setup(w => w.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).Callback(() => webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.Open));
            webSocketMock.Setup(w => w.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
                .Callback((ArraySegment<byte> s, CancellationToken t) =>
                {
                    messageBytes.CopyTo(s);
                    webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.Closed);
                })
                .ReturnsAsync(receiveResult);

            var loggerMock = new Mock<ILogger<ReceiverClient>>();

            var webSocketFactoryMock = new Mock<IWebSocketFactory>();
            webSocketFactoryMock.Setup(w => w.Create()).Returns(webSocketMock.Object);

            var receiverClient = new ReceiverClient(testReceiver, webSocketFactoryMock.Object, loggerMock.Object);
            object? sender = null;
            MessageReceivedEventArgs? eventArgs = null;
            receiverClient.MessageReceived += (s, e) =>
            {
                sender = s;
                eventArgs = e;
            };

            //ACT
            await receiverClient.Connect();

            //ASSERT
            sender.Should().Be(receiverClient);
            eventArgs?.Message.Should().BeEquivalentTo(message);
        }

        [TestMethod]
        public async Task Connect_ReceiveUnfinishedMessage_ShouldNotInvokeEvent()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var testReceiver = testData.Receivers[0];

            var message = new ReceiverSocketMessage
            {
                Properties =
                {
                    { "micGain", "53" }
                }
            };
            var messageJson = JsonConvert.SerializeObject(message);
            var messageBytes = new ArraySegment<byte>(Encoding.ASCII.GetBytes(messageJson));

            //Mark message as not-last
            var receiveResult = new WebSocketReceiveResult(messageBytes.Count, WebSocketMessageType.Text, false);

            var webSocketMock = new Mock<IReceiverClientWebSocket>();
            ConfigureVerificationMessage(webSocketMock, testReceiver, 1, receiveResult, (s,t) =>
            {
                messageBytes.CopyTo(s);
                webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.Closed);
            });

            var loggerMock = new Mock<ILogger<ReceiverClient>>();

            var webSocketFactoryMock = new Mock<IWebSocketFactory>();
            webSocketFactoryMock.Setup(w => w.Create()).Returns(webSocketMock.Object);

            var receiverClient = new ReceiverClient(testReceiver, webSocketFactoryMock.Object, loggerMock.Object);
            var invocations = 0;
            receiverClient.MessageReceived += (s, e) => invocations++;

            //ACT
            await receiverClient.Connect();

            //ASSERT
            invocations.Should().Be(1);
        }

        [TestMethod]
        public async Task Connect_ReceiveMalformatedMessage_ShouldNotInvokeEvent()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var testReceiver = testData.Receivers[0];

            var messageBytes = new ArraySegment<byte>(Encoding.ASCII.GetBytes("any malformated message"));

            //Mark message as not-last
            var webSocketMock = new Mock<IReceiverClientWebSocket>();
            var receiveResult = new WebSocketReceiveResult(messageBytes.Count, WebSocketMessageType.Text, true);
            ConfigureVerificationMessage(webSocketMock, testReceiver, 1, receiveResult, (s, t) =>
            {
                messageBytes.CopyTo(s);
                webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.Closed);
            });

            var loggerMock = new Mock<ILogger<ReceiverClient>>();

            var webSocketFactoryMock = new Mock<IWebSocketFactory>();
            webSocketFactoryMock.Setup(w => w.Create()).Returns(webSocketMock.Object);

            var receiverClient = new ReceiverClient(testReceiver, webSocketFactoryMock.Object, loggerMock.Object);
            var invokes = 0;
            receiverClient.MessageReceived += (s, e) => invokes++;

            //ACT
            await receiverClient.Connect();

            //ASSERT
            invokes.Should().Be(1);
        }

        [TestMethod]
        public async Task Connect_ServerRequestedClose_ShouldConfirmAndInvoke()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var testReceiver = testData.Receivers[0];

            var receiveResult = new WebSocketReceiveResult(1, WebSocketMessageType.Close, true, WebSocketCloseStatus.NormalClosure, null);

            var webSocketMock = new Mock<IReceiverClientWebSocket>();
            ConfigureVerificationMessage(webSocketMock, testReceiver, 1, receiveResult, (s, t) => {});

            var loggerMock = new Mock<ILogger<ReceiverClient>>();

            var webSocketFactoryMock = new Mock<IWebSocketFactory>();
            webSocketFactoryMock.Setup(w => w.Create()).Returns(webSocketMock.Object);

            var receiverClient = new ReceiverClient(testReceiver, webSocketFactoryMock.Object, loggerMock.Object);
            var invocationsMessageReceived = 0;
            object? connectionClosedSender = null;
            receiverClient.MessageReceived += (s, e) => invocationsMessageReceived++;
            receiverClient.ConnectionClosed += (s, e) => connectionClosedSender = s;

            //ACT
            await receiverClient.Connect();

            //ASSERT
            invocationsMessageReceived.Should().Be(1);
            connectionClosedSender.Should().Be(receiverClient);
            webSocketMock.Verify(w => w.CloseOutputAsync(receiveResult.CloseStatus.GetValueOrDefault(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [TestMethod]
        public async Task Connect_ConnectionLost_ShouldInvokeDirectly()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var testReceiver = testData.Receivers[0];

            var message = new ReceiverSocketMessage
            {
                Properties =
                {
                    { "micGain", "53" },
                    { "name", testReceiver.Name }
                }
            };
            var messageJson = JsonConvert.SerializeObject(message);
            var messageBytes = new ArraySegment<byte>(Encoding.ASCII.GetBytes(messageJson));

            var receiveResult = new WebSocketReceiveResult(messageBytes.Count, WebSocketMessageType.Text, true);

            var webSocketMock = new Mock<IReceiverClientWebSocket>();
            webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.None);
            webSocketMock.Setup(w => w.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).Callback(() => 
            {
                webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.Open).Callback(() =>
                {
                    webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.Closed);
                });
            });
            webSocketMock.Setup(w => w.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
               .Callback((ArraySegment<byte> s, CancellationToken t) => messageBytes.CopyTo(s))
               .ReturnsAsync(receiveResult);

            var loggerMock = new Mock<ILogger<ReceiverClient>>();

            var webSocketFactoryMock = new Mock<IWebSocketFactory>();
            webSocketFactoryMock.Setup(w => w.Create()).Returns(webSocketMock.Object);

            var receiverClient = new ReceiverClient(testReceiver, webSocketFactoryMock.Object, loggerMock.Object);
            object? connectionClosedSender = null;
            receiverClient.ConnectionClosed += (s, e) => connectionClosedSender = s;

            //ACT
            await receiverClient.Connect();

            //ASSERT
            connectionClosedSender.Should().Be(receiverClient);
            webSocketMock.Verify(w => w.CloseOutputAsync(receiveResult.CloseStatus.GetValueOrDefault(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [TestMethod]
        public async Task Connect_WrongName_ShouldCloseConnection()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var testReceiver = testData.Receivers[0];

            //Mark message as not-last
            var message = new ReceiverSocketMessage
            {
                Properties =
                {
                    { "micGain", "53" },
                    { "name", testReceiver.Name + "2" },
                }
            };
            var messageJson = JsonConvert.SerializeObject(message);
            var messageBytes = new ArraySegment<byte>(Encoding.ASCII.GetBytes(messageJson));

            var receiveResult = new WebSocketReceiveResult(messageBytes.Count, WebSocketMessageType.Text, true);

            var webSocketMock = new Mock<IReceiverClientWebSocket>();
            webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.None);
            webSocketMock.Setup(w => w.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).Callback(() => webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.Open));
            webSocketMock.Setup(w => w.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
                .Callback((ArraySegment<byte> s, CancellationToken t) => messageBytes.CopyTo(s))
                .ReturnsAsync(receiveResult);
            webSocketMock.Setup(w => w.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, It.IsAny<string>(), It.IsAny<CancellationToken>())).Callback(() =>
            {
                webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.Closed);
            });

            var loggerMock = new Mock<ILogger<ReceiverClient>>();

            var webSocketFactoryMock = new Mock<IWebSocketFactory>();
            webSocketFactoryMock.Setup(w => w.Create()).Returns(webSocketMock.Object);

            var receiverClient = new ReceiverClient(testReceiver, webSocketFactoryMock.Object, loggerMock.Object);
            var invocations = 0;
            receiverClient.MessageReceived += (s, e) => invocations++;

            Func<Task> action = async () => await receiverClient.Connect();

            //ACT / ASSERT
            await action.Should().ThrowAsync<WebSocketException>();
            
            invocations.Should().Be(0);
            webSocketMock.Verify(w => w.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [TestMethod]
        public async Task Connect_CorrectName_ShouldInvoke()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var testReceiver = testData.Receivers[0];

            //Mark message as not-last
            var message = new ReceiverSocketMessage
            {
                Properties =
                {
                    { "micGain", "53" },
                    { "name", testReceiver.Name },
                }
            };
            var messageJson = JsonConvert.SerializeObject(message);
            var messageBytes = new ArraySegment<byte>(Encoding.ASCII.GetBytes(messageJson));

            var receiveResult = new WebSocketReceiveResult(messageBytes.Count, WebSocketMessageType.Text, true);

            var webSocketMock = new Mock<IReceiverClientWebSocket>();
            webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.None);
            webSocketMock.Setup(w => w.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>())).Callback(() => webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.Open));
            webSocketMock.Setup(w => w.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
                .Callback((ArraySegment<byte> s, CancellationToken t) =>
                {
                    messageBytes.CopyTo(s);
                    webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.Closed);
                })
                .ReturnsAsync(receiveResult);

            var loggerMock = new Mock<ILogger<ReceiverClient>>();

            var webSocketFactoryMock = new Mock<IWebSocketFactory>();
            webSocketFactoryMock.Setup(w => w.Create()).Returns(webSocketMock.Object);

            var receiverClient = new ReceiverClient(testReceiver, webSocketFactoryMock.Object, loggerMock.Object);
            var invokedMessageReceived = false;
            receiverClient.MessageReceived += (s, e) => invokedMessageReceived = true;

            //ACT
            await receiverClient.Connect();

            //ASSERT
            invokedMessageReceived.Should().BeTrue();
            webSocketMock.Verify(w => w.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        }

        [TestMethod]
        public async Task Connect_AlreadyOpen_ShouldThrowException()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var testReceiver = testData.Receivers[0];

            var webSocketMock = new Mock<IReceiverClientWebSocket>();
            webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.Open);

            var loggerMock = new Mock<ILogger<ReceiverClient>>();

            var webSocketFactoryMock = new Mock<IWebSocketFactory>();
            webSocketFactoryMock.Setup(w => w.Create()).Returns(webSocketMock.Object);

            var receiverClient = new ReceiverClient(testReceiver, webSocketFactoryMock.Object, loggerMock.Object);

            Func<Task> action = async () => await receiverClient.Connect();

            //ACT / ASSERT
            await action.Should().ThrowAsync<WebSocketException>();
            webSocketMock.Verify(w => w.ConnectAsync(new Uri($"ws://{testReceiver.AddressableHost}"), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task Disconnect_ConnectionOpen_ShouldDisconnectWebSocket()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var testReceiver = testData.Receivers[0];

            var webSocketMock = new Mock<IReceiverClientWebSocket>();
            webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.Open);

            var loggerMock = new Mock<ILogger<ReceiverClient>>();

            var webSocketFactoryMock = new Mock<IWebSocketFactory>();
            webSocketFactoryMock.Setup(w => w.Create()).Returns(webSocketMock.Object);

            var receiverClient = new ReceiverClient(testReceiver, webSocketFactoryMock.Object, loggerMock.Object);

            //ACT
            await receiverClient.Disconnect();

            //ASSERT
            webSocketMock.Verify(w => w.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, It.IsAny<string>(), CancellationToken.None), Times.Once);
        }

        [TestMethod]
        public async Task Disconnect_ConnectionClosed_ShouldSkip()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var testReceiver = testData.Receivers[0];

            var webSocketMock = new Mock<IReceiverClientWebSocket>();
            webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.None);

            var loggerMock = new Mock<ILogger<ReceiverClient>>();

            var webSocketFactoryMock = new Mock<IWebSocketFactory>();
            webSocketFactoryMock.Setup(w => w.Create()).Returns(webSocketMock.Object);

            var receiverClient = new ReceiverClient(testReceiver, webSocketFactoryMock.Object, loggerMock.Object);

            //ACT
            await receiverClient.Disconnect();

            //ASSERT
            webSocketMock.Verify(w => w.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, It.IsAny<string>(), CancellationToken.None), Times.Never);
        }

        [TestMethod]
        public async Task Update_Always_ShouldSendToWebSocket()
        {
            //ARRANGE
            var testData = new ReceiverTestData();

            var testReceiver = testData.Receivers[0];

            var webSocketMock = new Mock<IReceiverClientWebSocket>();
            webSocketMock.SetupGet(w => w.State).Returns(WebSocketState.None);

            var loggerMock = new Mock<ILogger<ReceiverClient>>();

            var webSocketFactoryMock = new Mock<IWebSocketFactory>();
            webSocketFactoryMock.Setup(w => w.Create()).Returns(webSocketMock.Object);

            var receiverClient = new ReceiverClient(testReceiver, webSocketFactoryMock.Object, loggerMock.Object);

            //ACT
            await receiverClient.Update(testReceiver);

            //ASSERT
            webSocketMock.Verify(w => w.SendAsync(It.IsAny<ArraySegment<byte>>(), WebSocketMessageType.Text, true, CancellationToken.None), Times.Once);
        }
    }
}
