using Microsoft.EntityFrameworkCore;

namespace SennheiserBackend.Database.Repositories.Entities
{
    [Index(nameof(Id), IsUnique = true)]
    public class MicrophoneEntity : EntityBase
    {
        public int MicGain { get; set; }
        public string ReceiverId { get; set; } = "";
        public ReceiverEntity? Receiver { get; set; }
    }
}
