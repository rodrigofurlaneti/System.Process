using MediatR;
using Microsoft.Extensions.Logging;
using System.Process.Domain.Repositories;
using System.Phoenix.Common.Exceptions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Application.Commands.DeleteReceiver
{
    public class DeleteReceiverCommand : IRequestHandler<DeleteReceiverRequest, DeleteReceiverResponse>
    {
        #region Properties

        private ILogger<DeleteReceiverCommand> Logger { get; }
        private IReceiverWriteRepository ReceiverWriteRepository { get; }
        private IReceiverReadRepository ReceiverReadRepository { get; }

        #endregion

        #region Constructor

        public DeleteReceiverCommand(
            ILogger<DeleteReceiverCommand> logger,
            IReceiverWriteRepository receiverWriteRepository,
            IReceiverReadRepository receiverReadRepository)
        {
            Logger = logger;
            ReceiverWriteRepository = receiverWriteRepository;
            ReceiverReadRepository = receiverReadRepository;
        }

        #endregion

        #region INotificationHandler implementation

        public async Task<DeleteReceiverResponse> Handle(DeleteReceiverRequest request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("Starting process to remove receiver");

                return await Task.FromResult(RemoveReceiver(request, cancellationToken));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw;
            }

        }
        #endregion

        #region Methods
        private DeleteReceiverResponse RemoveReceiver(DeleteReceiverRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var adapter = new DeleteReceiverAdapter();
                var result = ReceiverReadRepository.Find(adapter.Adapt(request));

                if (result.Count == 0)
                {
                    throw new NotFoundException($"Receiver Id {request.ReceiverId} not found");
                }

                ReceiverWriteRepository.Remove(result.FirstOrDefault(), cancellationToken);

                return new DeleteReceiverResponse
                {
                    Code = "00",
                    Message = "Receiver successfully removed."
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
