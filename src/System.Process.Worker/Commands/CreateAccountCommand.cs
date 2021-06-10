using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Application.Commands.CreateAccount;
using System.Process.Infrastructure.Messages;
using System.Phoenix.Event;
using System.Phoenix.Event.Kafka.Config;
using System.Phoenix.Event.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Worker.Commands
{
    public class CreateAccountCommand : ICommand
    {
        #region Properties

        private ConsumerConfig Config { get; }
        private IConsumer Consumer { get; }
        private IMediator Mediator { get; }
        private ILogger<CreateAccountCommand> Logger { get; }

        #endregion

        #region Constructor

        public CreateAccountCommand(
            IOptions<ConsumerConfig> config,
            IConsumer consumer,
            IMediator mediator,
            ILogger<CreateAccountCommand> logger)
        {
            Config = config.Value;
            Consumer = consumer;
            Mediator = mediator;
            Logger = logger;
        }

        #endregion

        #region ICommand implementation

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation($"CreateAccount Worker started");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Logger.LogInformation($"Start consuming from {string.Join(" - ", Config.Topics)}");
                    Consumer.Consume<AccountMessage>(Config.Topics, cancellationToken, ProcessMessage);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Execution error : {ex.Message}");
                }
            }

            await Task.CompletedTask;
        }

        #endregion

        #region Methods

        private ExecutionResult ProcessMessage(MessageContent<AccountMessage> message)
        {
            try
            {
                var notification = new CreateAccountNotification
                {
                    MessageContent = message
                };

                Mediator.Publish(notification).Wait();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Sending message to dead letter - ApplicationId: {message.Payload.ApplicationId} - Reason: {ex.InnerException.Message}");

                return new ExecutionResult()
                {
                    Exception = ex
                };
            }

            return ExecutionResult.Success();
        }

        #endregion
    }
}