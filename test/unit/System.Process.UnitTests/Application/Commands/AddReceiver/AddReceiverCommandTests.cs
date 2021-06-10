using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Commands.AddReceiver;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Phoenix.Common.Exceptions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.UnitTests.Application.Commands.AddReceiver
{
    public class AddReceiverCommandTests
    {
        [Fact(DisplayName = "Should Send Handle Add Receiver")]
        public async Task ShouldSendHandleAddReceiverAsync()
        {
            var logger = new Mock<ILogger<AddReceiverCommand>>();
            var receiverReadRepository = new Mock<IReceiverReadRepository>();
            var receiverWriteRepository = new Mock<IReceiverWriteRepository>();
            var ProcessConfig = new Mock<IOptions<ProcessConfig>>();

            var receivers = new List<Receiver>();
            receiverReadRepository
                .Setup(r => r.FindExistent(It.IsAny<Receiver>())).Returns(receivers);

            var deleteReceiverCommand = new AddReceiverCommand(logger.Object, receiverWriteRepository.Object, receiverReadRepository.Object);
            var cancellationToken = new CancellationToken();
            var request = new AddReceiverRequest
            {
                AccountNumber = "16516",
                AccountType = "D",
                BankType = "S",
                CompanyName = "",
                CustomerId = "123",
                EmailAdress = "email@mail.com",
                FirstName = "Raul",
                LastName = "Segal",
                NickName = "NickName",
                PhoneNumber = "+5512727222",
                ReceiverType = "D",
                RoutingNumber = "1",
                Ownership = "S"
            };

            await deleteReceiverCommand.Handle(request, cancellationToken);
        }

        [Fact(DisplayName = "Should Send Handle Add Receiver Already Exists")]
        public async Task ShouldSendHandleAddReceiverAlreadyExistsAsync()
        {
            var logger = new Mock<ILogger<AddReceiverCommand>>();
            var receiverReadRepository = new Mock<IReceiverReadRepository>();
            var receiverWriteRepository = new Mock<IReceiverWriteRepository>();
            var accountConfig = new Mock<IOptions<ProcessConfig>>();

            var receivers = new List<Receiver>
            {
                new Receiver
                {
                    AccountNumber = "65151",
                    AccountType = "D",
                }
            };
            receiverReadRepository
                .Setup(r => r.FindExistent(It.IsAny<Receiver>())).Returns(receivers);

            var deleteReceiverCommand = new AddReceiverCommand(logger.Object, receiverWriteRepository.Object, receiverReadRepository.Object);
            var cancellationToken = new CancellationToken();
            var request = new AddReceiverRequest
            {
                AccountNumber = "16516",
                AccountType = "D",
                BankType = "S",
                CompanyName = "",
                CustomerId = "123",
                EmailAdress = "email@mail.com",
                FirstName = "Raul",
                LastName = "Segal",
                NickName = "NickName",
                PhoneNumber = "+5512727222",
                ReceiverType = "D",
                RoutingNumber = "1",
                Ownership = "S"
            };

            await Assert.ThrowsAsync<ConflictException>(() => deleteReceiverCommand.Handle(request, cancellationToken));
        }
    }
}