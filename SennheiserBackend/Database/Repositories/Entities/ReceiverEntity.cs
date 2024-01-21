using Microsoft.EntityFrameworkCore;

namespace SennheiserBackend.Database.Repositories.Entities
{
    [Index(nameof(Id), IsUnique = true)]
    public class ReceiverEntity : EntityBase
    {
        public string Name { get; set; } = "";
        public string Host { get; set; } = "";
        public int? Port { get; set; }
        public MicrophoneEntity Microphone { get; set; }
    }
}
