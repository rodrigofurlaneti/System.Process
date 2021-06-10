using FluentAssertions;
using System.Process.Application.Commands.CardReplace;
using System.Process.Domain.Entities;
using System.Process.Domain.ValueObjects;
using Xunit;

namespace System.Process.UnitTests.Application.Commands.CardReplace
{
    public class CardReplaceAdapterTests
    {
        private ProcessConfig ProcessConfig { get; set; }
        public CardReplaceAdapterTests()
        {
            ProcessConfig = new ProcessConfig();
        }
        #region Tests

        [Fact(DisplayName = "Should Return Card Replace Params Adapter Successfully")]
        public void ShouldReturnCardReplaceParams()
        {
            var createInquiryRequest = GetCardReplaceRequestDto();
            var createInquiryAdapter = new CardReplaceAdapter(ProcessConfig);
            var result = createInquiryAdapter.Adapt(createInquiryRequest);

            result.Should().NotBeNull();
        }

        #endregion

        #region Methods

        private CardReplaceRequestDto GetCardReplaceRequestDto()
        {
            return new CardReplaceRequestDto
            {
                Card = new Card
                {
                    CardId = 1,
                    CustomerId = "SP-001",
                    LastFour = "2831",
                    AccountBalance = "0",
                    CardType = "",
                    Bin = "",
                    Pan = "938498434322831",
                    ExpirationDate = "2310",
                    CardHolder = "Test",
                    BusinessName = "Test",
                    Locked = 1,
                    CardStatus = "",
                    Validated = 1
                },
                Request = new CardReplaceRequest
                {
                    CardId = 1,
                    Pan = "2831",
                    Address = new Address
                    {
                        Type = "Primary",
                        Line1 = "Street Line 213",
                        Line2 = "",
                        Line3 = "",
                        City = "SHANANNTOWN",
                        State = "FL",
                        Country = "United States",
                        ZipCode = "11111"
                    },
                    ReplaceReason = "Damaged"
                },
                EncryptKey = "32398a19a66b498e94c16a6e9b257a7e"
            };
        }

        #endregion
    }
}
