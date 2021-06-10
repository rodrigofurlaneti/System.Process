using System.Process.Domain.Entities;

namespace System.Process.Application.Commands.CardReplace
{
    public class CardReplaceRequestDto
    {
        public Card Card;
        public CardReplaceRequest Request;
        public string EncryptKey;
    }
}
