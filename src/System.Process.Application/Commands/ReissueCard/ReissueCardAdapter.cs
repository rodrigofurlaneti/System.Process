using System.Text;
using Jose;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Fis.ReissueCard.Messages;
using System.Proxy.Fis.ReissueCard.Messages.ValueObjects;
using System.Proxy.Fis.ValueObjects;

namespace System.Process.Application.Commands.ReissueCard
{
    public class ReissueCardAdapter : IAdapter<ReissueCardParams, ReissueCardRequestDto>
    {
        private ProcessConfig ProcessConfig { get; set; }
        public ReissueCardAdapter(ProcessConfig ProcessConfig)
        {
            ProcessConfig = ProcessConfig;
        }
        public ReissueCardParams Adapt(ReissueCardRequestDto input)
        {
            return new ReissueCardParams
            {
                Pan = new Pan
                {
                    Alias = string.Empty,
                    PlainText = string.Empty,
                    CipherText = JWT.Encode(input.Card.Pan, Encoding.ASCII.GetBytes(input.EncryptKey), JweAlgorithm.A256GCMKW, JweEncryption.A256GCM),
                },
                Plastic = string.Empty,
                DateLastMaintained = string.Empty,
                DateLastUpdated = string.Empty,
                DateLastUsed = string.Empty,
                ReissueDetails = new ReissueCardDetails
                {
                    ActivationFlag = ProcessConfig.ActivationFlag,
                    AddressType = string.Empty,
                    CardOrderFlag = new SimplifiedBaseDescription
                    {
                        HostValue = ProcessConfig.CardOrderFlagHostValue
                    },
                    CardOrderStatusFlag = new SimplifiedBaseDescription
                    {
                        HostValue = ProcessConfig.CardOrderStatusFlagHostValue
                    },
                    CardOrderType = new SimplifiedBaseDescription
                    {
                        HostValue = ProcessConfig.CardOrderTypeHostValue
                    },
                    CardReissueIndicator = ProcessConfig.CardReissueIndicator,
                    CardStockCode = "",
                    CustomImageId = string.Empty,
                    EmbossedName = string.Empty,
                    ExpirationDate = input.Card.ExpirationDate,
                    FlatCardInd = string.Empty,
                    ImageId = string.Empty,
                    IssuerStockCode = string.Empty,
                    MemberNumber = ProcessConfig.MemberNumberReplaceCard,
                    PhotoId = ProcessConfig.PhotoId
                }
            };
        }
    }
}
