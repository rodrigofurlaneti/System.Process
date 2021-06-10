using System.Process.Domain.Entities;

namespace System.Process.Domain.ValueObjects
{
    public class FisChangeStatusCard
    {
        public Card Card { get; set; }
        public string Action { get; set; }
        public string EncryptKey { get; set; }
    }
}
