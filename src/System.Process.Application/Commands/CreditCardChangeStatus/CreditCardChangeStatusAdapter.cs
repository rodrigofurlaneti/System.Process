using System.Text;
using Jose;
using System.Process.Domain.Constants;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Fis.LockUnlock.Messages;
using System.Proxy.Fis.LockUnlock.Messages.ValueObjects;
using System.Proxy.Fis.ValueObjects;

namespace System.Process.Application.Commands.CreditCardChangeStatus
{
    public class CreditCardChangeStatusAdapter : IAdapter<LockUnlockParams, FisChangeStatusCard>
    {
        #region IAdapter

        public LockUnlockParams Adapt(FisChangeStatusCard input)
        {
            return new LockUnlockParams
            {
                Pan = new Pan
                {
                    Alias = string.Empty,
                    PlainText = string.Empty,
                    CipherText = JWT.Encode(input.Card.Pan, Encoding.ASCII.GetBytes(input.EncryptKey), JweAlgorithm.A256GCMKW, JweEncryption.A256GCM),
                },
                Control = new Control
                {
                    AccountEnabled = input.Action.Trim().ToLower().Equals(Constants.CardUnlock) ? "false" : "true"
                }
            };
        }

        #endregion
    }
}
