using FluentAssertions.Common;
using Microsoft.Extensions.Logging;
using Moq;
using System.Process.Application.Clients.Cards;
using System.Process.Application.DataTransferObjects;
using System.Process.Application.Queries.ConsultCardsByCustomerId;
using System.Process.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.UnitTests.Application.Queries.ConsultCardsByCustomerIdQueryTests
{
    public class ConsultCardsByCustomerIdQueryTests
    {
        [Fact(DisplayName = "Handle should update 'card.Validated' to 1 if request validation is OK")]
        public async Task ShouldUpdateCardValidatedTo1IfRequestValidationIdOk()
        {
            var logger = new Mock<ILogger<ConsultCardsByCustomerIdQuery>>();
            var cardService = new Mock<ICardService>();

            ConsultCardsByCustomerIdResponse response = new ConsultCardsByCustomerIdResponse
            {
                CardRecords = new List<CustomerCardDto>
                {
                    new CustomerCardDto
                    {
                        AccountBalance = "0",
                        AssetId = "00aa0",
                        Bin = "00000",
                        BusinessName = "CORPORATE INC.",
                        CardHolder = "CORPORATE CARD HOLDER",
                        CardStatus = "Pending Activation",
                        CardId = 1,
                        ExpirationDate = "2312",
                        Pan = "3133",
                        CardType = "DR",
                        Locked = true
                    }
                }
            };

            IList<Card> cards = new List<Card>
            {
                new Card
                {
                    CardId = 1,
                    AssetId = "00aa0",
                    CustomerId = "SP-340",
                    ExpirationDate = "2312",
                    LastFour = "3133",
                    CardType = "DR",
                    AccountBalance = "0",
                    Bin = "00000",
                    BusinessName = "COMPORATE INC.",
                    CardHolder = "CORPORATE CARD HOLDER",
                    Locked = 1,
                    CardStatus = "Pending Activation"
                }
            };

            var cancellationToken = new CancellationToken();
            cardService
                .Setup(r => r.FindByCardType(It.IsAny<string>(), It.IsAny<string>(), cancellationToken)).Returns(Task.FromResult(cards));

            var consultCardsByCustomerIdQuery = new ConsultCardsByCustomerIdQuery(logger.Object, cardService.Object);

            var request = new ConsultCardsByCustomerIdRequest("SP-340", "DR");

            var handleResponse = await consultCardsByCustomerIdQuery.Handle(request, cancellationToken);

            handleResponse.IsSameOrEqualTo(response);
        }

        [Fact(DisplayName = "Handle should throw message 'No card was found for this client' if no card is found")]
        public async Task ShouldThowMessageIfNoCardIsFound()
        {
            var logger = new Mock<ILogger<ConsultCardsByCustomerIdQuery>>();
            var cardService = new Mock<ICardService>();

            IList<Card> cards = new List<Card>();

            var cancellationToken = new CancellationToken();
            cardService
                .Setup(r => r.FindByCardType(It.IsAny<string>(), It.IsAny<string>(), cancellationToken)).Returns(Task.FromResult(cards));

            var consultCardsByCustomerIdQuery = new ConsultCardsByCustomerIdQuery(logger.Object, cardService.Object);

            var request = new ConsultCardsByCustomerIdRequest("SP-340", "DR");

            try
            {
                var handleResponse = await consultCardsByCustomerIdQuery.Handle(request, cancellationToken);
            }
            catch (Exception ex)
            {
                Assert.Equal(ex.Message, "No card was found for this client");
            }
        }
    }
}
