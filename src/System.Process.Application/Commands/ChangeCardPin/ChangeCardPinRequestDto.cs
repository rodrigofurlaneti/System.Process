using System.Process.Domain.Entities;

namespace System.Process.Application.Commands.ChangeCardPin
{
    public class ChangeCardPinRequestDto
    {
        public Card Card;
        public ChangeCardPinRequest Request;
        public string EncryptKey;
    }
}
