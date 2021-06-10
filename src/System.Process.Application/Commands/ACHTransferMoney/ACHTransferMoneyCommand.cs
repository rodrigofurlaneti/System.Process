using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Application.Clients.Jarvis;
using System.Process.Application.Commands.ACHTransferMoney.Adapters;
using System.Process.Domain.Constants;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Feedzai.Base.Config;
using System.Proxy.Feedzai.Base.Messages;
using System.Proxy.Feedzai.TransferInitiation;
using System.Proxy.Feedzai.TransferInitiation.Messages;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.SearchAddress;
using System.Proxy.Salesforce.SearchAddress.Messages;
using System.Proxy.Silverlake.Inquiry;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Proxy.Silverlake.Inquiry.Messages.Response;
using System.Proxy.Silverlake.Transaction;
using ServiceException = System.Proxy.Silverlake.Base.Exceptions.SilverlakeException;

namespace System.Process.Application.Commands.AchTransferMoney
{
    public class AchTransferMoneyCommand : IRequestHandler<AchTransferMoneyRequest, AchTransferMoneyResponse>
    {

        #region Properties

        private ILogger<AchTransferMoneyCommand> Logger { get; }
        private ITransactionOperation TransactionOperation { get; }
        private ProcessConfig ProcessConfig { get; }
        private ITransferInitiationClient TransferInitiationClient { get; }
        private FeedzaiConfig FeedzaiConfig { get; }
        private IInquiryOperation InquiryOperation { get; }
        private IReceiverReadRepository ReceiverReadRepository { get; }
        private IGetTokenClient TokenClient { get; }
        private ISearchAddressClient SearchAddressClient { get; }
        private GetTokenParams SalesforceTokenParams { get; }
        private IJarvisClient JarvisClient { get; }
        private ITransferWriteRepository TransferWriteRepository { get; }

        #endregion

        #region Constructor

        public AchTransferMoneyCommand(
            ILogger<AchTransferMoneyCommand> logger,
            ITransactionOperation transactionOperation,
            IOptions<ProcessConfig> ProcessConfig,
            ITransferInitiationClient transferInitiationClient,
            IOptions<FeedzaiConfig> feedzaiConfig,
            IInquiryOperation inquiryOperation,
            IReceiverReadRepository receiverReadRepository,
            IJarvisClient jarvisClient,
            IGetTokenClient tokenClient,
            ISearchAddressClient searchAddressClient,
            IOptions<GetTokenParams> salesforceTokenSalesforce,
            ITransferWriteRepository transferWriteRepository)
        {
            Logger = logger;
            TransactionOperation = transactionOperation;
            ProcessConfig = ProcessConfig.Value;
            TransferInitiationClient = transferInitiationClient;
            FeedzaiConfig = feedzaiConfig.Value;
            InquiryOperation = inquiryOperation;
            ReceiverReadRepository = receiverReadRepository;
            TokenClient = tokenClient;
            SearchAddressClient = searchAddressClient;
            SalesforceTokenParams = salesforceTokenSalesforce.Value;
            JarvisClient = jarvisClient;
            TransferWriteRepository = transferWriteRepository;
        }

        #endregion

        #region INotificationHandler implementation

        public async Task<AchTransferMoneyResponse> Handle(AchTransferMoneyRequest request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("Starting process for money ach transfer");

                await ValidateAccount(request, cancellationToken);

                var receiver = ValidateReceiver(request);

                var adapter = new AchTransferMoneyAdapter(ProcessConfig);


                var adaptRequest = adapter.AdaptValidate(request);

                var resultValidate = await TransactionOperation.TransferAddValidateAsync(adaptRequest, cancellationToken);
                if (resultValidate.ResponseStatus != "Success")
                {
                    Logger.LogInformation($"Error during TransferAddValidate execution - error status: {resultValidate.ResponseStatus}");
                    throw new UnprocessableEntityException("Error during TransferAddValidate execution", $"error status: {resultValidate.ResponseStatus}");
                }

                var authToken = await TokenClient.GetToken(SalesforceTokenParams, cancellationToken);
                var searchAddressResult = await SearchAddressClient.SearchAddress(new SearchAddressParams() { SystemId = request.SystemId }, authToken.Result.AccessToken, cancellationToken);
                var address = searchAddressResult.Result.Records.FirstOrDefault();

                if (address == null)
                {
                    Logger.LogError("SearchAddress did not return address");
                    throw new UnprocessableEntityException($"SearchAddress did not return address", new ErrorStructure(ErrorCodes.NoAddressAvailable));
                }

                var transferInitiationResult = await GetTransferInitiation(request, cancellationToken, receiver, address);

                if (transferInitiationResult.Result.Decision.Equals("decline") || transferInitiationResult.Result.Decision.Equals("review"))
                {
                    Logger.LogError($"Transaction not approved for the Life Cycle {transferInitiationResult.Result.LifecycleId}");
                    throw new UnprocessableEntityException("Transaction not approved", transferInitiationResult.Result.ToString());
                }

                var transferRequest = adapter.Adapt(request);

                var result = await TransactionOperation.TransferAddAsync(transferRequest, cancellationToken);
                if (result.ResponseStatus != "Success")
                {
                    Logger.LogInformation($"Error during TransferAdd execution - error with {result.TransferKey} transfer key");
                    throw new UnprocessableEntityException("Error during TransferAdd execution", $"Error with {result.TransferKey} transfer key");
                }

                Logger.LogInformation("Transfer add operation completed");

                return await Task.FromResult(new AchTransferMoneyResponse
                {
                    TransactionId = result.TransferKey
                });
            }
            catch (ServiceException ex)
            {
                throw new UnprocessableEntityException(ex.Message, ex, new ErrorStructure(ex.ErrorDetails?.FirstOrDefault()?.ErrorCode, Providers.JackHenry));
            }
        }

        #endregion

        #region Methods

        private async Task<BaseResult<TransferInitiationResult>> GetTransferInitiation(AchTransferMoneyRequest request, CancellationToken cancellationToken, Receiver receiver, SearchAddressResponse searchAddressResult)
        {
            try
            {
                var adapter = new AchTransferInitiationAdapter(ProcessConfig);

                var feedzaiRequest = adapter.Adapt(request);

                if (!string.IsNullOrWhiteSpace(request.SessionId))
                {
                    var deviceDetails = await JarvisClient.GetDeviceDetails(request.SessionId, cancellationToken);

                    feedzaiRequest.DeviceLatitude = deviceDetails?.Latitude ?? string.Empty;
                    feedzaiRequest.DeviceLongitude = deviceDetails?.Longitude ?? string.Empty;
                    feedzaiRequest.DeviceIpAddress = deviceDetails?.IpAddress ?? string.Empty;
                    feedzaiRequest.DeviceIpAddressLocation = deviceDetails?.TimeZone ?? string.Empty;
                    feedzaiRequest.DeviceId = deviceDetails?.MacAddress ?? string.Empty;
                    feedzaiRequest.DeviceOperatingSystem = deviceDetails?.Platform ?? string.Empty;
                }

                Guid transactionId = Guid.NewGuid(); // Transaction ID for manual feedbacks later on
                feedzaiRequest.LifecycleId = transactionId.ToString();
                feedzaiRequest.SenderName = searchAddressResult.Account.LegalName;
                feedzaiRequest.SenderStreetAddr = searchAddressResult.AddressLine1;
                feedzaiRequest.SenderCountryCode = searchAddressResult.Country;
                feedzaiRequest.ReceiverId = receiver.ReceiverId.ToString();
                feedzaiRequest.ReceiverName = receiver.FirstName;
                return await TransferInitiationClient.TransferInitiation(feedzaiRequest, FeedzaiConfig.Token, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Fail during GetTransferInitiation execution");
                throw new UnprocessableEntityException($"Fail during GetTransferInitiation execution", ex);
            }
        }

        private async Task<ProcessearchResponse> ConsultAccount(ProcessearchRequest accountRequest, CancellationToken cancellationToken)
        {
            try
            {
                return await InquiryOperation.ProcessearchAsync(accountRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                var message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Logger.LogInformation($"Error when trying to consulting account - AccountId: {accountRequest.AccountId} - Description: { message }");
                throw new NotFoundException("Error when trying to consulting account", $"AccountId: {accountRequest.AccountId} - Description: { message }");
            }
        }

        private async Task ValidateAccount(AchTransferMoneyRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Started the account search with CustomerId {request.CustomerId}");

            var consultProcessAdapter = new AchConsultProcessAdapter();

            var response = await ConsultAccount(consultProcessAdapter.Adapt(request), cancellationToken);

            var account = response.ProcessearchRecInfo.Find(x => x.AccountId.AccountNumber == request.AccountFrom.AccountId);

            if (account == null)
            {
                Logger.LogError($"Account {request.AccountFrom.AccountId} not found");
                throw new NotFoundException($"Account {request.AccountFrom.AccountId} not found");
            }

            if (account.CustomerId != request.CustomerId)
            {
                Logger.LogError("Account does not belong to customer");
                throw new UnprocessableEntityException($"Account {request.AccountFrom.AccountId} does not belong to customer {request.CustomerId}", response.ToString());
            }

            if (account.Amount < request.Amount)
            {
                Logger.LogError("Account does not have enough balance");
                throw new UnprocessableEntityException($"Account {request.AccountFrom.AccountId} does not have enough balance", new ErrorStructure(ErrorCodes.AvailableBalanceExceeded));
            }
        }

        private Receiver ValidateReceiver(AchTransferMoneyRequest request)
        {
            Logger.LogInformation($"Started the account search with ReceiverId {request.ReceiverId}");

            var consultReceiverAdapter = new AchConsultReceiverAdapter();
            var receiver = ReceiverReadRepository.Find(consultReceiverAdapter.Adapt(request)).FirstOrDefault();

            if (receiver == null)
            {
                Logger.LogError($"Receiver Id {request.ReceiverId} not found");
                throw new NotFoundException($"Receiver Id {request.ReceiverId} not found");
            }

            if (!receiver.RoutingNumber.Equals(request.AccountTo.RoutingNumber) ||
                !receiver.AccountNumber.Equals(request.AccountTo.AccountId) ||
                !receiver.AccountType.Equals(request.AccountTo.AccountType))
            {
                Logger.LogError("Receiver sent is not registered");
                throw new UnprocessableEntityException($"Receiver {request.ReceiverId} sent is not registered", receiver.ToString());
            }

            return receiver;
        }
        #endregion
    }
}