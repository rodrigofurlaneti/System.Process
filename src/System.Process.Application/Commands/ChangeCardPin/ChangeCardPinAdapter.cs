using System.Text;
using Jose;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Fis.ChangeCardPin.Messages;
using System.Proxy.Fis.ValueObjects;

namespace System.Process.Application.Commands.ChangeCardPin
{
    public class ChangeCardPinAdapter : IAdapter<ChangeCardPinParams, ChangeCardPinRequestDto>
    {
        public ChangeCardPinParams Adapt(ChangeCardPinRequestDto input)
        {
            return new ChangeCardPinParams
            {
                Pan = new Pan
                {
                    PlainText = string.Empty,
                    CipherText = JWT.Encode(input.Card.Pan, Encoding.ASCII.GetBytes(input.EncryptKey), JweAlgorithm.A256GCMKW, JweEncryption.A256GCM),
                    Alias = string.Empty
                },
                Plastic = string.Empty,
                ExpirationDate = input.Card.ExpirationDate,
                CurrentPin = new Pin
                {
                    CipherText = string.Empty,
                },
                NewPin = new Pin
                {
                    CipherText = JWT.Encode(input.Request.NewPin, Encoding.ASCII.GetBytes(input.EncryptKey), JweAlgorithm.A256GCMKW, JweEncryption.A256GCM),
                }
            };
        }
    }
}