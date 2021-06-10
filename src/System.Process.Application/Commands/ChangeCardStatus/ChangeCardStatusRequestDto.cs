using System.Process.Domain.Entities;

namespace System.Process.Application.Commands.ChangeCardStatus
{
    public class ChangeCardStatusRequestDto
    {
        public Card Card;
        public ChangeCardStatusRequest Request;
        public string EncryptKey;
    }
}
