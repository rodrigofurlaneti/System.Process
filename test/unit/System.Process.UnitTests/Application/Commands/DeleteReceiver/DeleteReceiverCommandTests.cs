using Microsoft.Extensions.Logging;
using Moq;
using System.Process.Application.Commands.DeleteReceiver;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Phoenix.Common.Exceptions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.UnitTests.Application.Commands.DeleteReceiver
{
    public class DeleteReceiverCommandTests
    {
        [Fact(DisplayName = "Should Send Handle Delete Receiver")]
        public async Task ShouldSendHandleDeleteReceiverAsync()
        {
            var logger = new Mock<ILogger<DeleteReceiverCommand>>();
            var receiverReadRepository = new Mock<IReceiverReadRepository>();
            var receiverWriteRepository = new Mock<IReceiverWriteRepository>();

            var receivers = new List<Receiver>
            {
                new Receiver
                {
                   AccountNumber = "13141",
                   AccountType = "D"
                }
            };
            receiverReadRepository
                .Setup(r => r.Find(It.IsAny<int>())).Returns(receivers);
            receiverWriteRepository
               .Setup(r => r.Remove(It.IsAny<Receiver>(), It.IsAny<CancellationToken>())).Verifiable();

            var deleteReceiverCommand = new DeleteReceiverCommand(logger.Object, receiverWriteRepository.Object, receiverReadRepository.Object);
            var request = new DeleteReceiverRequest
            {
                ReceiverId = "12334"
            };
            var cancellationToken = new CancellationToken();

            await deleteReceiverCommand.Handle(request, cancellationToken);
        }

        [Fact(DisplayName = "Should Send Handle Receiver Not Found")]
        public async Task ShouldSendHandleNoReceiverFoundAsync()
        {
            var logger = new Mock<ILogger<DeleteReceiverCommand>>();
            var receiverReadRepository = new Mock<IReceiverReadRepository>();
            var receiverWriteRepository = new Mock<IReceiverWriteRepository>();

            var deleteReceiverCommand = new DeleteReceiverCommand(logger.Object, receiverWriteRepository.Object, receiverReadRepository.Object);
            var request = new DeleteReceiverRequest
            {
                ReceiverId = "817871"
            };
            var cancellationToken = new CancellationToken();
            var receivers = new List<Receiver>();

            receiverReadRepository
                 .Setup(r => r.Find(It.IsAny<int>())).Throws(new NotFoundException("not found"));

            receiverWriteRepository
               .Setup(r => r.Remove(It.IsAny<Receiver>(), It.IsAny<CancellationToken>())).Verifiable();

            await Assert.ThrowsAsync<NotFoundException>(() => deleteReceiverCommand.Handle(request, cancellationToken));
        }
    }
}
