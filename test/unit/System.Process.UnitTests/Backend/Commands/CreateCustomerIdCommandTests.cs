using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.CustomerIds.Worker.Commands;
using System.Phoenix.Event;
using System.Phoenix.Event.Kafka.Config;
using Xunit;

namespace System.CustomerIds.UnitTests.Backend.Commands
{
    public class CreateCustomerIdCommandTests
    {
        [Fact(DisplayName = "Should send ExecuteAsync CreateCustomerIdCommand Successfully")]
        public async Task ShouldSendExecuteAsyncSuccessfullyAsync()
        {
            var config = new Mock<IOptions<ConsumerConfig>>();
            var consumer = new Mock<IConsumer>();
            var mediator = new Mock<IMediator>();
            var logger = new Mock<ILogger<CreateCustomerIdCommand>>();
            var cancellationToken = new CancellationToken(true);
            var command = new CreateCustomerIdCommand(config.Object, consumer.Object, mediator.Object, logger.Object);

            await command.ExecuteAsync(cancellationToken);
        }

        [Fact(DisplayName = "Should send ExecuteAsync UpdateProcessCommand Error", Skip = "true")]
        public async Task ShouldSendExecuteAsyncErrorAsync()
        {
            var config = new Mock<IOptions<ConsumerConfig>>();
            var consumer = new Mock<IConsumer>();
            var mediator = new Mock<IMediator>();
            var logger = new Mock<ILogger<CreateCustomerIdCommand>>();
            var cancellationToken = new CancellationToken();
            var command = new CreateCustomerIdCommand(config.Object, consumer.Object, mediator.Object, logger.Object);

            await Assert.ThrowsAnyAsync<Exception>(() => command.ExecuteAsync(cancellationToken));
        }
    }
}
