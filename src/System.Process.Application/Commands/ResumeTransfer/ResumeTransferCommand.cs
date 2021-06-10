using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using System.Process.Application.Commands.RemoteDepositCapture.Adapters;
using System.Process.Application.Commands.ResumeTransfer.Adapters;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Rda.AddItem;
using System.Proxy.Rda.AddItem.Messages;
using System.Proxy.Rda.Authenticate;
using System.Proxy.Rda.Authenticate.Messages;
using System.Proxy.Rda.CreateBatch;
using System.Proxy.Rda.CreateBatch.Messages;
using System.Proxy.Rda.Messages;
using System.Proxy.Rda.UpdateBatch;
using System.Proxy.Rda.UpdateBatch.Messages;
using System.Proxy.Silverlake.Base.Exceptions;
using System.Proxy.Silverlake.TranferWire;
using System.Proxy.Silverlake.Transaction;
using System.Proxy.Silverlake.Transaction.Messages.Response;

namespace System.Process.Application.Commands.ResumeTransfer
{
    public class ResumeTransferCommand : IRequestHandler<ResumeTransferRequest, ResumeTransferResponse>
    {
        #region Properties
        private ILogger<ResumeTransferCommand> Logger { get; }
        private ITransferReadRepository TransferReadRepository { get; }
        private ITransferWriteRepository TransferWriteRepository { get; }
        private ITransferItemWriteRepository TransferItemWriteRepository { get; }
        private ITransferItemReadRepository TransferItemReadRepository { get; }
        private ITransactionOperation TransactionOperation { get; }
        private ITransferWireOperation TransferWireOperation { get; }
        private IAuthenticateClient AuthenticateClient { get; }
        private ICreateBatchClient CreateBatchClient { get; }
        private IAddItemClient AddItemClient { get; }
        private IUpdateBatchClient UpdateBatchClient { get; }
        private ProcessConfig ProcessConfig { get; }

        #endregion

        #region Constructor
        public ResumeTransferCommand(
            ILogger<ResumeTransferCommand> logger,
            ITransferReadRepository transferReadRepository,
            ITransferWriteRepository transferWriteRepository,
            ITransferItemReadRepository transferItemReadRepository,
            ITransferItemWriteRepository transferItemWriteRepository,
            ITransactionOperation transactionOperation,
            ITransferWireOperation transferWireOperation,
            IAuthenticateClient authenticateClient,
            ICreateBatchClient createBatchClient,
            IAddItemClient addItemClient,
            IUpdateBatchClient updateBatchClient,
            IOptions<ProcessConfig> ProcessConfig)
        {
            Logger = logger;
            TransferReadRepository = transferReadRepository;
            TransferWriteRepository = transferWriteRepository;
            TransferItemReadRepository = transferItemReadRepository;
            TransferItemWriteRepository = transferItemWriteRepository;
            TransactionOperation = transactionOperation;
            TransferWireOperation = transferWireOperation;
            AuthenticateClient = authenticateClient;
            CreateBatchClient = createBatchClient;
            AddItemClient = addItemClient;
            UpdateBatchClient = updateBatchClient;
            ProcessConfig = ProcessConfig.Value;
        }
        #endregion

        #region INotificationHandler implementation
        public async Task<ResumeTransferResponse> Handle(ResumeTransferRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Resume Transfer Process Started - SystemId: {request.LifeCycleId}");

            try
            {
                var resultTransfer = new TransferResponse();
                var result = TransferReadRepository.FindByLifeCycleId(request.LifeCycleId);

                if (result == null)
                {
                    throw new NotFoundException($"There is no transfer for the Life Cycle {request.LifeCycleId}");
                }

                if (result.TransferDirection.Equals("OUT"))
                {
                    await RemoveLock(result, cancellationToken);
                }

                if (request.Decision.ToLower().Equals("approve"))
                {
                    resultTransfer = await ExecuteTransfer(result, cancellationToken);
                }

                if (result.TransferItems != null)
                {
                    TransferItemWriteRepository.Remove(result.TransferItems.FirstOrDefault(), cancellationToken);
                }

                TransferWriteRepository.Remove(result, cancellationToken);

                return new ResumeTransferResponse
                {
                    Message = $"Transfer {request.Decision} with success",
                    Result = resultTransfer
                };
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error while resuming the transfer operation for Life Cycle {request.LifeCycleId} \n {ex}");
                throw;
            }
        }

        #endregion

        #region Methods

        private async Task<StopCheckCancelResponse> RemoveLock(Transfer input, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Remove Lock Started - SystemId: {input.SystemId}");

            try
            {
                var adapter = new StopCheckCancelAdapter();
                var adapt = adapter.Adapt(input);
                var result = await TransactionOperation.StopCheckCancelAsync(adapt, cancellationToken);

                if (result.ResponseStatus != "Success")
                {
                    Logger.LogInformation($"Error during StopCheckCancel execution");
                    throw new UnprocessableEntityException("Error during StopCheckCancel execution");
                }

                Logger.LogInformation("Remove lock operation completed");

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new Exception(ex.Message, null);
            }
        }

        private async Task<TransferResponse> ExecuteTransfer(Transfer input, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Step Execute Transfer Started - SystemId: {input.SystemId}");

            try
            {
                var response = new TransferResponse();

                switch (input.TransferType)
                {
                    case "INTERNAL":
                        response = await InternalTransfer(input, cancellationToken);
                        break;
                    case "ACH":
                        response = await AchTransfer(input, cancellationToken);
                        break;
                    case "WIRE":
                        response = await WireTransfer(input, cancellationToken);
                        break;
                    case "RDC":
                        response = await RdcTransfer(input, cancellationToken);
                        break;
                    default:
                        break;

                };

                Logger.LogInformation("Execute transfer operation completed");

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new Exception(ex.Message, null);
            }
        }

        private async Task<TransferResponse> InternalTransfer(Transfer input, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Step Internal TransferAdd Started - SystemId: {input.SystemId}");

            try
            {
                var adapter = new InternalTransferAdapter(ProcessConfig);
                var adapt = adapter.Adapt(input);
                var result = await TransactionOperation.TransferAddAsync(adapt, cancellationToken);

                if (result.ResponseStatus != "Success")
                {
                    Logger.LogInformation($"Error during TransferAdd execution - error with {result.TransferKey} transfer key");
                    throw new UnprocessableEntityException("Error during TransferAdd execution", $"Error with {result.TransferKey} transfer key");
                }

                Logger.LogInformation("Internal Transfer operation completed");

                var adapterTransfer = new TransferResponseAdapter();
                var response = adapterTransfer.Adapt(result);

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new Exception(ex.Message, null);
            }
        }

        private async Task<TransferResponse> AchTransfer(Transfer input, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Step Ach TransferAdd Started - SystemId: {input.SystemId}");

            try
            {
                var adapter = new AchTransferAdapter(ProcessConfig);
                var adapt = adapter.Adapt(input);
                var result = await TransactionOperation.TransferAddAsync(adapt, cancellationToken);

                if (result.ResponseStatus != "Success")
                {
                    Logger.LogInformation($"Error during Ach TransferAdd execution - error with {result.TransferKey} transfer key");
                    throw new UnprocessableEntityException("Error during Ach TransferAdd execution", $"Error with {result.TransferKey} transfer key");
                }

                Logger.LogInformation("Ach Transfer operation completed");

                var adapterTransfer = new AchTransferResponseAdapter();
                var response = adapterTransfer.Adapt(result);

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new Exception(ex.Message, null);
            }
        }

        private async Task<TransferResponse> WireTransfer(Transfer input, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Step Wire Transfer Started - SystemId: {input.SystemId}");

            try
            {
                var adapter = new WireTransferAdapter(ProcessConfig);
                var adapt = adapter.Adapt(input);
                var result = await TransferWireOperation.TransferWireAddAsync(adapt, cancellationToken);

                if (result.ResponseStatus != "Success")
                {
                    Logger.LogInformation($"Error during Wire Transfer execution - error with {result.TransactionalReceiptId} transfer id");
                    throw new UnprocessableEntityException("Error during Wire Transfer execution", $"Error with {result.TransactionalReceiptId} transfer id");
                }

                Logger.LogInformation("Wire Transfer operation completed");

                var adapterTransfer = new WireTransferResponseAdapter();
                var response = adapterTransfer.Adapt(result);

                return response;
            }
            catch (SilverlakeException ex)
            {
                Logger.LogError(ex.Message);
                throw new SilverlakeException(ex.Errors.ToJson());
            }
        }

        private async Task<TransferResponse> RdcTransfer(Transfer input, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Step Transfer Rdc Started - SystemId: {input.SystemId}");
            input.TransferItems = TransferItemReadRepository.Find(input.LifeCycleId);

            try
            {
                //Autheticate Customer by HomeBankingId.
                var auth = await Authenticate(input, cancellationToken);

                //Create Batch in Rda by HomebankingId 
                var createBatch = await CreateBatch(auth, cancellationToken);

                //AddItem in Rda by BatchReference 
                var addItem = await AddItem(createBatch, input, auth, cancellationToken);

                //Update Batch in Rda by BatchReference
                var result = await UpdateBatch(createBatch, auth, addItem, cancellationToken);

                var adapterTransfer = new RdcTransferResponseAdapter();
                var response = adapterTransfer.Adapt(result);

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new Exception(ex.Message, null);
            }
        }

        private async Task<BaseResult<AuthenticateResponse>> Authenticate(Transfer input, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Step Authenticate Started - SystemId: {input.SystemId}");

            try
            {
                var adapter = new AuthenticateRdcAdapter();
                var adapt = adapter.Adapt(input);
                var result = await AuthenticateClient.Authenticate(adapt, cancellationToken);

                if (result.Result.ValidationResults.Count() > 0)
                {
                    throw new UnprocessableEntityException("Cannot authenticate customer in rda", result?.Result?.ValidationResults?.First().ToString());
                }

                Logger.LogInformation($"Step Authenticate Customer Success - SystemId: {input.SystemId}");

                return result;

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new Exception(ex.Message, null);
            }
        }

        private async Task<BaseResult<CreateBatchResponse>> CreateBatch(BaseResult<AuthenticateResponse> input, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Step Create Batch Started");

            try
            {
                var adapter = new CreateBatchAdapter();
                var adapt = adapter.Adapt(input.Result);
                var result = await CreateBatchClient.CreateBatch(adapt, cancellationToken);

                if (result.Result.ValidationResults.Count() > 0)
                {
                    throw new UnprocessableEntityException("Cannot Create Batch in rda", result?.Result?.ValidationResults?.First().ToString());
                }

                Logger.LogInformation($"Step Create Batch Success");

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new Exception(ex.Message, null);
            }
        }

        private async Task<List<BaseResult<AddItemResponse>>> AddItem(BaseResult<CreateBatchResponse> batchReponse, Transfer input, BaseResult<AuthenticateResponse> auth, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Step Add Item Started");

            try
            {
                var resultList = new List<BaseResult<AddItemResponse>>();

                foreach (var item in input.TransferItems)
                {
                    var adapter = new AddItemRdcAdapter(auth, item);
                    var adapt = adapter.Adapt(batchReponse);
                    var result = await AddItemClient.AddItem(adapt, cancellationToken);

                    resultList.Add(result);

                    Logger.LogInformation($"Step Add Item Success");
                }

                return resultList;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new Exception(ex.Message, null);
            }
        }

        private async Task<BaseResult<UpdateBatchResponse>> UpdateBatch(BaseResult<CreateBatchResponse> input, BaseResult<AuthenticateResponse> auth, List<Proxy.Rda.Messages.BaseResult<AddItemResponse>> addItem, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Step Update Batch Started");

            try
            {
                var adapter = new UpdateBatchRdcAdapter(auth, addItem);
                var adapt = adapter.Adapt(input.Result);
                var result = await UpdateBatchClient.UpdateBatch(adapt, cancellationToken);

                if (result.Result.ValidationResults.Count() > 0)
                {
                    throw new UnprocessableEntityException("Cannot Update Batch in rda", result?.Result?.ValidationResults?.First().ToString());
                }

                Logger.LogInformation($"Step Update Batch Success");

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new Exception(ex.Message, null);
            }
        }

        #endregion
    }
}
