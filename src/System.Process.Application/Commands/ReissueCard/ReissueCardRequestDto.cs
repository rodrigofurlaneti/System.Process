using System.Process.Domain.Entities;

namespace System.Process.Application.Commands.ReissueCard
{
    public class ReissueCardRequestDto
    {
        public Card Card;
        public ReissueCardRequest Request;
        public string EncryptKey;
    }
}
