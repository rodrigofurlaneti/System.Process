using System.Process.Domain.Entities;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Fis.ActivateCard.Messages;
using System.Proxy.Fis.ValueObjects;

namespace System.Process.Application.Commands.ActivateCard
{
    public class ActivateCardAdapter : IAdapter<ActivateCardParams, Card>
    {
        #region Properties

        private ProcessConfig ProcessConfig { get; set; }

        #endregion

        public ActivateCardAdapter(ProcessConfig ProcessConfig)
        {
            ProcessConfig = ProcessConfig;
        }



        public ActivateCardParams Adapt(Card input)
        {
            return new ActivateCardParams
            {
                Pan = new Pan
                {
                    Alias = "",
                    PlainText = input.Pan,
                    CipherText = ""
                },
                CardActivationStatus = ProcessConfig.CardActivationStatus
            };
        }
    }
}