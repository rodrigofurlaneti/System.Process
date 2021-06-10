using Microsoft.Extensions.Logging;
using Moq;
using System.Process.Application.Queries.FindReceivers;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Phoenix.Common.Exceptions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.UnitTests.Application.Queries.FindReceivers
{
    public class FindReceiversQueryTests
    {
        [Fact(DisplayName = "Should Send Handle Find Receivers")]
        public async Task ShouldSendHandleFindReceiversAsync()
        {
            var logger = new Mock<ILogger<FindReceiversQuery>>();
            var receiverReadRepository = new Mock<IReceiverReadRepository>();

            var findReceiversQuery = new FindReceiversQuery(logger.Object, receiverReadRepository.Object);
            var request = new FindReceiversRequest
            {
                CustomerId = "13141",
                InquiryType = "A",
                Ownership = "A"
            };
            var cancellationToken = new CancellationToken();
            var receivers = new List<Receiver>
            {
                new Receiver
                {
                   AccountNumber = "13141",
                   AccountType = "D"
                }
            };
            receiverReadRepository
                .Setup(t => t.FindByCustomerId(It.IsAny<string>())).Returns(receivers);
            receiverReadRepository
                .Setup(t => t.FindByBankType(It.IsAny<string>(), It.IsAny<string>())).Returns(receivers);
            receiverReadRepository
               .Setup(t => t.FindByOwnerShip(It.IsAny<string>(), It.IsAny<string>())).Returns(receivers);
            receiverReadRepository
              .Setup(t => t.FindByBankTypeAndOwnership(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(receivers);

            await findReceiversQuery.Handle(request, cancellationToken);
        }

        [Fact(DisplayName = "Should Send Handle Find Receivers By Account and Ownership")]
        public async Task ShouldSendHandleFindReceiversByProcesspecificAndOwnerShipAsync()
        {
            var logger = new Mock<ILogger<FindReceiversQuery>>();
            var receiverReadRepository = new Mock<IReceiverReadRepository>();

            var findReceiversQuery = new FindReceiversQuery(logger.Object, receiverReadRepository.Object);
            var request = new FindReceiversRequest
            {
                CustomerId = "13141",
                InquiryType = "A",
                Ownership = "S"
            };
            var cancellationToken = new CancellationToken();
            var receivers = new List<Receiver>
            {
                new Receiver
                {
                   AccountNumber = "13141",
                   AccountType = "D",
                }
            };
            receiverReadRepository
                .Setup(t => t.FindByCustomerId(It.IsAny<string>())).Returns(receivers);
            receiverReadRepository
                .Setup(t => t.FindByBankType(It.IsAny<string>(), It.IsAny<string>())).Returns(receivers);
            receiverReadRepository
               .Setup(t => t.FindByOwnerShip(It.IsAny<string>(), It.IsAny<string>())).Returns(receivers);
            receiverReadRepository
              .Setup(t => t.FindByBankTypeAndOwnership(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(receivers);

            await findReceiversQuery.Handle(request, cancellationToken);
        }

        [Fact(DisplayName = "Should Send Handle Find Receivers By Account and Ownership Specific")]
        public async Task ShouldSendHandleFindReceiversByAccountAndOwnerShipSpecificAsync()
        {
            var logger = new Mock<ILogger<FindReceiversQuery>>();
            var receiverReadRepository = new Mock<IReceiverReadRepository>();

            var findReceiversQuery = new FindReceiversQuery(logger.Object, receiverReadRepository.Object);
            var request = new FindReceiversRequest
            {
                CustomerId = "13141",
                InquiryType = "S",
                Ownership = "A"
            };
            var cancellationToken = new CancellationToken();
            var receivers = new List<Receiver>
            {
                new Receiver
                {
                   AccountNumber = "13141",
                   AccountType = "D"
                }
            };
            receiverReadRepository
                .Setup(t => t.FindByCustomerId(It.IsAny<string>())).Returns(receivers);
            receiverReadRepository
                .Setup(t => t.FindByBankType(It.IsAny<string>(), It.IsAny<string>())).Returns(receivers);
            receiverReadRepository
               .Setup(t => t.FindByOwnerShip(It.IsAny<string>(), It.IsAny<string>())).Returns(receivers);
            receiverReadRepository
              .Setup(t => t.FindByBankTypeAndOwnership(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(receivers);
            await findReceiversQuery.Handle(request, cancellationToken);
        }

        [Fact(DisplayName = "Should Send Handle No receiver found")]
        public async Task ShouldSendHandleNoReceiverFoundAsync()
        {
            var logger = new Mock<ILogger<FindReceiversQuery>>();
            var receiverReadRepository = new Mock<IReceiverReadRepository>();

            var findReceiversQuery = new FindReceiversQuery(logger.Object, receiverReadRepository.Object);
            var request = new FindReceiversRequest
            {
                CustomerId = "13141",
                InquiryType = "E",
                Ownership = "S"
            };
            var cancellationToken = new CancellationToken();
            var receivers = new List<Receiver>();
            receiverReadRepository
                .Setup(t => t.FindByCustomerId(It.IsAny<string>())).Returns(receivers);
            receiverReadRepository
                .Setup(t => t.FindByBankType(It.IsAny<string>(), It.IsAny<string>())).Returns(receivers);
            receiverReadRepository
               .Setup(t => t.FindByOwnerShip(It.IsAny<string>(), It.IsAny<string>())).Returns(receivers);
            receiverReadRepository
              .Setup(t => t.FindByBankTypeAndOwnership(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(receivers);
            await Assert.ThrowsAsync<NotFoundException>(() => findReceiversQuery.Handle(request, cancellationToken));
        }

        [Fact(DisplayName = "Should Send Handle No Receiver Found Under 'System' Filter")]
        public async Task ShouldSendHandleNoReceiverFoundSAsync()
        {
            var logger = new Mock<ILogger<FindReceiversQuery>>();
            var receiverReadRepository = new Mock<IReceiverReadRepository>();

            var findReceiversQuery = new FindReceiversQuery(logger.Object, receiverReadRepository.Object);
            var request = new FindReceiversRequest
            {
                CustomerId = "13141",
                InquiryType = "S",
                Ownership = "A"
            };
            var cancellationToken = new CancellationToken();
            var receivers = new List<Receiver>();
            receiverReadRepository
                .Setup(t => t.FindByCustomerId(It.IsAny<string>())).Returns(receivers);
            receiverReadRepository
                .Setup(t => t.FindByBankType(It.IsAny<string>(), It.IsAny<string>())).Returns(receivers);
            receiverReadRepository
               .Setup(t => t.FindByOwnerShip(It.IsAny<string>(), It.IsAny<string>())).Returns(receivers);
            receiverReadRepository
              .Setup(t => t.FindByBankTypeAndOwnership(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(receivers);
            await Assert.ThrowsAsync<NotFoundException>(() => findReceiversQuery.Handle(request, cancellationToken));
        }
    }
}
