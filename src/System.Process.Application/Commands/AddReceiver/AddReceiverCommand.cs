using MediatR;
using Microsoft.Extensions.Logging;
using System.Process.Domain.Repositories;
using System.Phoenix.Common.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Application.Commands.AddReceiver
{
    public class AddReceiverCommand : IRequestHandler<AddReceiverRequest, AddReceiverResponse>
    {
        #region Properties

        private ILogger<AddReceiverCommand> Logger { get; }
        private IReceiverWriteRepository ReceiverWriteRepository { get; }
        private IReceiverReadRepository ReceiverReadRepository { get; }

        #endregion

        #region Constructor

        public AddReceiverCommand(
            ILogger<AddReceiverCommand> logger,
            IReceiverWriteRepository receiverWriteRepository,
            IReceiverReadRepository receiverReadRepository
            )
        {
            Logger = logger;
            ReceiverWriteRepository = receiverWriteRepository;
            ReceiverReadRepository = receiverReadRepository;

        }

        #endregion

        #region INotificationHandler implementation

        public async Task<AddReceiverResponse> Handle(AddReceiverRequest request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("Starting process to add new receiver");

                return await AddReceiver(request, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw;
            }

        }
        #endregion

        #region Methods
        private async Task<AddReceiverResponse> AddReceiver(AddReceiverRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var adapter = new AddReceiverAdapter();
                var adaptedRequest = adapter.Adapt(request);

                var result = ReceiverReadRepository.FindExistent(adaptedRequest);

                if (result.Count != 0)
                {
                    Logger.LogInformation($"Receiver with Account Number: {request.AccountNumber}  already exists. Receiver not added.");
                    throw new ConflictException($"Receiver with Account Number: {request.AccountNumber}  already exists. Receiver not added.");
                }

                await ReceiverWriteRepository.Add(adaptedRequest, cancellationToken);

                Logger.LogInformation("Receiver successfully added.");

                return new AddReceiverResponse
                {
                    Code = "00",
                    Message = "Receiver successfully added."
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw;
            }

        }

        #endregion
    }
}