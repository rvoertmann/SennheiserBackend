using SennheiserBackend.Database.Repositories.Entities;
using SennheiserBackend.Models;

namespace SennheiserBackend.Tests.TestData
{
    public class ReceiverTestData
    {
        public List<Receiver> Receivers { get; set; } =
        [
                new() {
                    Id = "testid-1",
                    Name = "TestReceiver1",
                    Host = "127.0.0.1",
                    Port = 8080,
                    Microphone = new Microphone {MicGain = 20}
                },
                new() {
                    Id = "testid-2",
                    Name = "TestReceiver2",
                    Host = "127.0.0.2",
                    Port = 8081,
                    Microphone = new Microphone {MicGain = 30}
                },
                new() {
                    Id = "testid-3",
                    Name = "TestReceiver3",
                    Host = "127.0.0.3",
                    Microphone = new Microphone {MicGain = 40}
                },
            ];

        public List<ReceiverEntity> ReceiverEntities { get; set; } =
        [
                new() {
                    Id = "testid-1",
                    Name = "TestReceiver1",
                    Host = "127.0.0.1",
                    Port = 8080,
                    Microphone = new MicrophoneEntity
                    {
                        Id = "testmicid-1",
                        ReceiverId = "testid-1",
                        MicGain = 20
                    }
                },
                new() {
                    Id = "testid-2",
                    Name = "TestReceiver2",
                    Host = "127.0.0.2",
                    Port = 8081,
                    Microphone = new MicrophoneEntity
                    {
                        Id = "testmicid-2",
                        ReceiverId = "testid-2",
                        MicGain = 30
                    }
                },
                new() {
                    Id = "testid-3",
                    Name = "TestReceiver3",
                    Host = "127.0.0.3",
                    Microphone = new MicrophoneEntity
                    {
                        Id = "testmicid-3",
                        ReceiverId = "testid-3",
                        MicGain = 40
                    }
                },
            ];
    }
}
