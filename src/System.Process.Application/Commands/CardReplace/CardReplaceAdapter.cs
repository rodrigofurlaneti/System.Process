using System.Text;
using Jose;
using System.Process.Domain.Constants;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Fis.CardReplace.Messages;
using System.Proxy.Fis.CardReplace.Messages.ValueObjects;
using System.Proxy.Fis.ValueObjects;

namespace System.Process.Application.Commands.CardReplace
{
    public class CardReplaceAdapter : IAdapter<CardReplaceParams, CardReplaceRequestDto>
    {

        private ProcessConfig ProcessConfig { get; set; }

        public CardReplaceAdapter(ProcessConfig ProcessConfig)
        {
            ProcessConfig = ProcessConfig;
        }


        public CardReplaceParams Adapt(CardReplaceRequestDto input)
        {
            return new CardReplaceParams
            {
                Pan = new Pan
                {
                    Alias = "",
                    PlainText = "",
                    CipherText = JWT.Encode(input.Card.Pan, Encoding.ASCII.GetBytes(input.EncryptKey), JweAlgorithm.A256GCMKW, JweEncryption.A256GCM),
                },
                Bin = input.Card.Bin,
                Plastic = "",
                ReplacementCardDetails = new ReplacementCardDetails
                {
                    ExpirationDate = input.Card.ExpirationDate,
                    CardOrderFlag = new BaseDescription
                    {
                        Description = Constants.CardAndPin,
                        HostDescription = Constants.CardAndPin,
                        HostValue = ProcessConfig.CardOrderFlagHostValue
                    },
                    CardOrderStatusFlag = new BaseDescription
                    {
                        Description = Constants.NewOrder,
                        HostDescription = Constants.NewOrder,
                        HostValue = ProcessConfig.CardOrderStatusFlagHostValue
                    },
                    CardOrderType = new BaseDescription
                    {
                        Description = Constants.IssueNewImmediately,
                        HostDescription = Constants.IssueNewImmediately,
                        HostValue = ProcessConfig.CardOrderTypeHostValue
                    },
                    CardReissueIndicator = ProcessConfig.CardReissueIndicator,
                    CardStockCode = "",
                    FlatCardInd = ProcessConfig.FlatCardInd,
                    IssuerStockCode = "",
                    CustomImageId = input.Card.CardHolder,
                    PhotoId = ProcessConfig.PhotoId,
                    ImageId = "",
                    ActivationFlag = ProcessConfig.ActivationFlag,
                    AddressType = ProcessConfig.AddressType,
                    EmbossedName = "",
                    MemberNumber = ProcessConfig.MemberNumberReplaceCard
                }
            };
        }
    }
}
