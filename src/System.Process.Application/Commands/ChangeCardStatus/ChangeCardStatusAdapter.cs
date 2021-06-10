using System.Text;
using Jose;
using System.Process.Domain.Constants;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Fis.LockUnlock.Messages;
using System.Proxy.Fis.LockUnlock.Messages.ValueObjects;
using System.Proxy.Fis.ValueObjects;

namespace System.Process.Application.Commands.ChangeCardStatus
{
    public class ChangeCardStatusAdapter : IAdapter<LockUnlockParams, FisChangeStatusCard>
    {
        public LockUnlockParams Adapt(FisChangeStatusCard request)
        {

            return new LockUnlockParams
            {
                Pan = new Pan
                {
                    Alias = string.Empty,
                    PlainText = string.Empty,
                    CipherText = JWT.Encode(request.Card.Pan, Encoding.ASCII.GetBytes(request.EncryptKey), JweAlgorithm.A256GCMKW, JweEncryption.A256GCM),
                },
                Control = new Control
                {
                    AccountEnabled = request.Action.Trim().ToLower().Equals(Constants.CardUnlock) ? "false" : "true"
                }
            };
        }
    }
}
