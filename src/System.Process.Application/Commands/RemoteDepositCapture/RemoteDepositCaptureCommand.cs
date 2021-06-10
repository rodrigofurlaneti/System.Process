using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Application.Clients.Jarvis;
using System.Process.Application.Commands.RemoteDepositCapture.Adapters;
using System.Process.Domain.Constants;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Configs;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Feedzai.Base.Config;
using System.Proxy.Feedzai.TransferInitiation;
using System.Proxy.Feedzai.TransferInitiation.Messages;
using System.Proxy.Rda.AddItem;
using System.Proxy.Rda.AddItem.Messages;
using System.Proxy.Rda.Authenticate;
using System.Proxy.Rda.Authenticate.Messages;
using System.Proxy.Rda.CreateBatch;
using System.Proxy.Rda.CreateBatch.Messages;
using System.Proxy.Rda.UpdateBatch;
using System.Proxy.Rda.UpdateBatch.Messages;
using System.Proxy.RdaAdmin.GetProcessCriteriaReference;
using System.Proxy.RdaAdmin.GetProcessCriteriaReference.Messages;
using System.Proxy.RdaAdmin.GetCustomersCriteria;
using System.Proxy.RdaAdmin.GetCustomersCriteria.Messages;
using System.Proxy.RdaAdmin.Messages;
using System.Proxy.Salesforce.GetAccountInformations;
using System.Proxy.Salesforce.GetAccountInformations.Messages;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.Messages;
using System.Proxy.Salesforce.SearchAddress;
using System.Proxy.Salesforce.SearchAddress.Messages;

namespace System.Process.Application.Commands.RemoteDepositCapture
{
    public class RemoteDepositCaptureCommand : IRequestHandler<RemoteDepositCaptureRequest, RemoteDepositCaptureResponse>
    {
        #region Properties

        private ILogger<RemoteDepositCaptureCommand> Logger { get; }
        private IGetCustomersCriteriaClient GetCustomersCriteriaClient { get; }
        private IGetProcessCriteriaReferenceClient GetProcessCriteriaReferenceClient { get; }
        private IAuthenticateClient AuthenticateClient { get; }
        private ICreateBatchClient CreateBatchClient { get; }
        private IAddItemClient AddItemClient { get; }
        private IUpdateBatchClient UpdateBatchClient { get; }
        private ITransferInitiationClient TransferInitiationClient { get; }
        private IGetAccountInformationsClient GetAccountInformationsClient { get; }
        private IGetTokenClient GetTokenClient { get; }
        private IJarvisClient JarvisClient { get; }
        private GetTokenParams ConfigSalesforce { get; }
        private RdaCredentialsConfig RdaCredentialsConfig { get; }
        private RemoteDepositCaptureRequest Request { get; set; }
        private FeedzaiConfig FeedzaiConfig { get; set; }
        private ProcessConfig ProcessConfig { get; }
        private ISearchAddressClient SearchAddressClient { get; }
        private ITransferWriteRepository TransferWriteRepository { get; }
        private ITransferItemWriteRepository TransferItemWriteRepository { get; }

        #endregion

        #region Constructor

        public RemoteDepositCaptureCommand(
            ILogger<RemoteDepositCaptureCommand> logger,
            IGetCustomersCriteriaClient getCustomersCriteriaClient,
            IGetProcessCriteriaReferenceClient getProcessCriteriaReferenceClient,
            IAuthenticateClient authenticateClient,
            ICreateBatchClient createBatchClient,
            IAddItemClient addItemClient,
            IUpdateBatchClient updateBatchClient,
            ITransferInitiationClient transferInitiationClient,
            IGetAccountInformationsClient getAccountInformationsClient,
            IGetTokenClient getTokenClient,
            IJarvisClient jarvisClient,
            IOptions<GetTokenParams> configSalesforce,
            IOptions<RdaCredentialsConfig> rdaCredentialsConfig,
            IOptions<FeedzaiConfig> feedzaiConfig,
            IOptions<ProcessConfig> ProcessConfig,
            ISearchAddressClient searchAddressClient,
            ITransferWriteRepository transferWriteRepository,
            ITransferItemWriteRepository transferItemWriteRepository
            )
        {
            Logger = logger;
            GetCustomersCriteriaClient = getCustomersCriteriaClient;
            GetProcessCriteriaReferenceClient = getProcessCriteriaReferenceClient;
            AuthenticateClient = authenticateClient;
            CreateBatchClient = createBatchClient;
            AddItemClient = addItemClient;
            UpdateBatchClient = updateBatchClient;
            TransferInitiationClient = transferInitiationClient;
            GetAccountInformationsClient = getAccountInformationsClient;
            GetTokenClient = getTokenClient;
            JarvisClient = jarvisClient;
            ConfigSalesforce = configSalesforce.Value;
            RdaCredentialsConfig = rdaCredentialsConfig.Value;
            FeedzaiConfig = feedzaiConfig.Value;
            ProcessConfig = ProcessConfig.Value;
            SearchAddressClient = searchAddressClient;
            TransferWriteRepository = transferWriteRepository;
            TransferItemWriteRepository = transferItemWriteRepository;
        }

        #endregion

        #region IRequestHandler
        public async Task<RemoteDepositCaptureResponse> Handle(RemoteDepositCaptureRequest request, CancellationToken cancellationToken)
        {
            Request = request;

            try
            {
                //Get Process Information by SystemId.
                var result = await ProcessInformation(request, cancellationToken);

                //Get Customer by SystemId in Rda.
                var customer = await ValidateCustomer(request, cancellationToken);

                //Autheticate Customer by HomeBankingId.
                var auth = await Authenticate(customer, cancellationToken);

                //Get Account by HomeBankingId and ReferenceId.
                var Process = await ValidateAccount(result, cancellationToken);

                //Validate Fraud in Feedzai 
                var validateFraud = await ValidateFraud(request, Process, cancellationToken);

                if (validateFraud.Result.Decision.Equals("decline") || validateFraud.Result.Decision.Equals("review"))
                {
                    Logger.LogError($"Transaction not approved for the Life Cycle {validateFraud.Result.LifecycleId}");
                    throw new UnprocessableEntityException("Transaction not approved", validateFraud.Result.ToString());
                }

                //Create Batch in Rda by HomebankingId 
                var createBatch = await CreateBatch(auth, cancellationToken);

                //AddItem in Rda by BatchReference 
                var addItem = await AddItem(createBatch, Process, auth, cancellationToken);

                //Update Batch in Rda by BatchReference
                var updateBatch = await UpdateBatch(createBatch, auth, addItem, cancellationToken);

                var responseAdapter = new RemoteDepositCaptureResponseAdapter(request, updateBatch);
                var response = responseAdapter.Adapt(addItem);

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new UnprocessableEntityException(ex.Message);
            }
        }
        #endregion

        #region Methods

        private async Task<BaseResult<QueryResult<GetAccountInformationsResponse>>> ProcessInformation(RemoteDepositCaptureRequest input, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Step Get Account Information Started - SystemId: {input.SystemId}");

            try
            {
                var authToken = await GetTokenClient.GetToken(ConfigSalesforce, cancellationToken);
                if (!authToken.IsSuccess)
                    throw new Exception("Error while obtaining access_token");

                var adapter = new GetAccountInformationsAdapter();
                var adapt = adapter.Adapt(input);
                var result = await GetAccountInformationsClient.GetAccount(adapt, authToken.Result.AccessToken, cancellationToken);

                if (!result.IsSuccess == true)
                {
                    throw new UnprocessableEntityException("Cannot Get Account Information in salesforce", result?.Result?.Errors?.First()?.Message);
                }
                if (result.Result.Records.Count() <= 0)
                {
                    throw new UnprocessableEntityException("Account Information not found", "Cannot Get Account Information in salesforce");
                }

                Logger.LogInformation($"Step Get Account Information Success - SystemId: {input.SystemId}");

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new Exception(ex.Message, null);
            }
        }

        private async Task<AdminBaseResult<GetCustomersCriteriaResponse>> ValidateCustomer(RemoteDepositCaptureRequest input, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Step Validate Customer Started - SystemId: {input.SystemId}");

            try
            {
                var adapter = new GetCustomersCriteriaAdapter(RdaCredentialsConfig);
                var adapt = adapter.Adapt(input);
                var result = await GetCustomersCriteriaClient.GetCustomersCriteria(adapt, cancellationToken);

                if (result.Result.Result != 1)
                {
                    throw new UnprocessableEntityException("Cannot Validate Customer in Rda", result?.Result?.ValidationResults?.First().ToString());
                }
                if (result.Result.Customers.Count <= 0)
                {
                    throw new UnprocessableEntityException("Customer Not Found in Rda", "Cannot Validate Customer in Rda");
                }

                Logger.LogInformation($"Step Validate Customer Success - SystemId: {input.SystemId}");

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new UnprocessableEntityException(ex.Message);
            }
        }

        private async Task<Proxy.Rda.Messages.BaseResult<AuthenticateResponse>> Authenticate(AdminBaseResult<GetCustomersCriteriaResponse> input, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Step Authenticate Started - SystemId: {Request.SystemId}");

            try
            {
                if (input.Result.Customers.First().IsEnabled == true)
                {
                    var adapter = new AuthenticateAdapter(RdaCredentialsConfig);
                    var adapt = adapter.Adapt(input);
                    var result = await AuthenticateClient.Authenticate(adapt, cancellationToken);

                    if (result.Result.ValidationResults.Count() > 0)
                    {
                        throw new UnprocessableEntityException("Cannot authenticate customer in rda", result?.Result?.ValidationResults?.First().ToString());
                    }

                    Logger.LogInformation($"Step Authenticate Customer Success - SystemId: {Request.SystemId}");

                    return result;
                }

                throw new UnprocessableEntityException("Customer not enable");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new UnprocessableEntityException(ex.Message);
            }
        }

        private async Task<AdminBaseResult<GetProcessCriteriaReferenceResponse>> ValidateAccount(BaseResult<QueryResult<GetAccountInformationsResponse>> input, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Step Validate Account Started - SystemId: {Request.SystemId}");

            try
            {
                var adapter = new GetProcessCriteriaReferenceAdapter(RdaCredentialsConfig);
                var adapt = adapter.Adapt(input);
                var result = await GetProcessCriteriaReferenceClient.GetProcessCriteriaReference(adapt, cancellationToken);

                if (result.Result.Process.Count() <= 0)
                {
                    throw new UnprocessableEntityException("Account Not Found in rda", "Cannot validate account in rda");
                }
                if (result.Result.Process.First().IsEnabled == false)
                {
                    throw new UnprocessableEntityException("Cannot validate account in rda", result?.Result?.ValidationResults?.First().ToString());
                }
                if (result.Result.Process.First().AccountNumber != Request.ToAccount || Convert.ToInt32(result?.Result?.Process?.First()?.RoutingNumber) != Convert.ToInt32(Request.ToRoutingNumber))
                {
                    throw new UnprocessableEntityException($"Wrong account ({Request.ToAccount}) or routing number({Request.ToRoutingNumber})", "Wrong account number or routing number");
                }

                Logger.LogInformation($"Step Validate Account Success - SystemId: {Request.SystemId}");

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new UnprocessableEntityException(ex.Message);
            }
        }

        private async Task<Proxy.Feedzai.Base.Messages.BaseResult<TransferInitiationResult>> ValidateFraud(RemoteDepositCaptureRequest input, AdminBaseResult<GetProcessCriteriaReferenceResponse> Process, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Step Create Batch Started - SystemId: {Request.SystemId}");

            try
            {

                var authToken = await GetTokenClient.GetToken(ConfigSalesforce, cancellationToken);
                var searchAddressResult = await SearchAddressClient.SearchAddress(new SearchAddressParams() { SystemId = input.SystemId }, authToken.Result.AccessToken, cancellationToken);
                var address = searchAddressResult.Result.Records.FirstOrDefault();

                if (address == null)
                {
                    Logger.LogError("SearchAddress did not return address");
                    throw new UnprocessableEntityException($"SearchAddress did not return address", new ErrorStructure(ErrorCodes.NoAddressAvailable));
                }

                var adapter = new TransferInitiationAdapter(Process, ProcessConfig);
                var feedzaiRequest = adapter.Adapt(input);

                if (!string.IsNullOrWhiteSpace(input.SessionId))
                {
                    var deviceDetails = await JarvisClient.GetDeviceDetails(input.SessionId, cancellationToken);

                    feedzaiRequest.DeviceLatitude = deviceDetails?.Latitude ?? string.Empty;
                    feedzaiRequest.DeviceLongitude = deviceDetails?.Longitude ?? string.Empty;
                    feedzaiRequest.DeviceIpAddress = deviceDetails?.IpAddress ?? string.Empty;
                    feedzaiRequest.DeviceId = deviceDetails?.MacAddress ?? string.Empty;
                    feedzaiRequest.DeviceOperatingSystem = deviceDetails?.Platform ?? string.Empty;
                }

                Guid transactionId = Guid.NewGuid(); // Transaction ID for manual feedbacks later on
                feedzaiRequest.LifecycleId = transactionId.ToString();
                feedzaiRequest.ReceiverName = address.Account.LegalName;
                feedzaiRequest.ReceiverCountryCode = address.Country;
                feedzaiRequest.ReceiverStreetAddr = address.AddressLine1;

                var result = await TransferInitiationClient.TransferInitiation(feedzaiRequest, FeedzaiConfig.Token, cancellationToken);

                if (result.Result.Errors?.Count() > 0)
                {
                    throw new UnprocessableEntityException("Cannot Validate Fraud in feedzai", result?.Result?.Errors?.First().ToString());
                }

                Logger.LogInformation($"Step Validate Fraud Success - SystemId: {Request.SystemId}");

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw new UnprocessableEntityException(ex.Message, ex);
            }
        }

        private async Task<Proxy.Rda.Messages.BaseResult<CreateBatchResponse>> CreateBatch(Proxy.Rda.Messages.BaseResult<AuthenticateResponse> input, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Step Create Batch Started - SystemId: {Request.SystemId}");

            try
            {
                var adapter = new CreateBatchAdapter();
                var adapt = adapter.Adapt(input.Result);
                var result = await CreateBatchClient.CreateBatch(adapt, cancellationToken);

                if (result.Result.ValidationResults.Count() > 0)
                {
                    throw new UnprocessableEntityException("Cannot Create Batch in rda", result?.Result?.ValidationResults?.First().ToString());
                }

                Logger.LogInformation($"Step Create Batch Success - SystemId: {Request.SystemId}");

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new UnprocessableEntityException(ex.Message);
            }
        }

        private async Task<List<Proxy.Rda.Messages.BaseResult<AddItemResponse>>> AddItem(Proxy.Rda.Messages.BaseResult<CreateBatchResponse> input, AdminBaseResult<GetProcessCriteriaReferenceResponse> account, Proxy.Rda.Messages.BaseResult<AuthenticateResponse> auth, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Step Add Item Started - SystemId: {Request.SystemId}");

            try
            {
                var resultList = new List<Proxy.Rda.Messages.BaseResult<AddItemResponse>>();

                foreach (var item in Request.Item)
                {
                    var adapter = new AddItemAdapter(auth, account, item);
                    var adapt = adapter.Adapt(input);
                    var result = await AddItemClient.AddItem(adapt, cancellationToken);

                    resultList.Add(result);

                    Logger.LogInformation($"Step Add Item Success - SystemId: {Request.SystemId}");
                }

                return resultList;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new UnprocessableEntityException(ex.Message);
            }
        }

        private async Task<Proxy.Rda.Messages.BaseResult<UpdateBatchResponse>> UpdateBatch(Proxy.Rda.Messages.BaseResult<CreateBatchResponse> input, Proxy.Rda.Messages.BaseResult<AuthenticateResponse> auth, List<Proxy.Rda.Messages.BaseResult<AddItemResponse>> addItem, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Step Update Batch Started - SystemId: {Request.SystemId}");

            try
            {
                var adapter = new UpdateBatchAdapter(auth, addItem, RdaCredentialsConfig);
                var adapt = adapter.Adapt(input.Result);
                var result = await UpdateBatchClient.UpdateBatch(adapt, cancellationToken);

                if (result.Result.ValidationResults.Count() > 0)
                {
                    throw new UnprocessableEntityException("Cannot Update Batch in rda", result?.Result?.ValidationResults?.First().ToString());
                }

                Logger.LogInformation($"Step Update Batch Success - SystemId: {Request.SystemId}");

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new UnprocessableEntityException(ex.Message);
            }
        }

        #endregion
    }
}
