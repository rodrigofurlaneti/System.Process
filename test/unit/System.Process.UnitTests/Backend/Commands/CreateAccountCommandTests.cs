using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Worker.Commands;
using System.Phoenix.Event;
using System.Phoenix.Event.Kafka.Config;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.UnitTests.Backend.Commands
{
    public class CreateAccountCommandTests
    {
        [Fact(DisplayName = "Should send ExecuteAsync CreateAccountCommand Successfully")]
        public async Task ShouldSendExecuteAsyncSuccessfullyAsync()
        {
            var config = new Mock<IOptions<ConsumerConfig>>();
            var consumer = new Mock<IConsumer>();
            var mediator = new Mock<IMediator>();
            var logger = new Mock<ILogger<CreateAccountCommand>>();
            var cancellationToken = new CancellationToken(true);
            var updateProcessCommand = new CreateAccountCommand(config.Object, consumer.Object, mediator.Object, logger.Object);

            await updateProcessCommand.ExecuteAsync(cancellationToken);
        }

        [Fact(DisplayName = "Should send ExecuteAsync UpdateProcessCommand Error", Skip = "true")]
        public async Task ShouldSendExecuteAsyncErrorAsync()
        {
            var config = new Mock<IOptions<ConsumerConfig>>();
            var consumer = new Mock<IConsumer>();
            var mediator = new Mock<IMediator>();
            var logger = new Mock<ILogger<CreateAccountCommand>>();
            var cancellationToken = new CancellationToken();
            var updateProcessCommand = new CreateAccountCommand(config.Object, consumer.Object, mediator.Object, logger.Object);

            await Assert.ThrowsAnyAsync<Exception>(() => updateProcessCommand.ExecuteAsync(cancellationToken));
        }
    }
}
