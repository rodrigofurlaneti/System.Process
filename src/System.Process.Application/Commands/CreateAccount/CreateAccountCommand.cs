using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Process.Application.Commands.CreateAccount.Card;
using System.Process.Application.Commands.CreateAccount.Response;
using System.Process.Application.DataTransferObjects;
using System.Process.Domain.Constants;
using System.Process.Domain.Containers;
using System.Process.Domain.Enums;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Configs;
using System.Process.Infrastructure.Messages;
using System.Process.Worker.Clients.Product;
using System.Phoenix.Common.Exceptions;
using System.Phoenix.Event;
using System.Phoenix.Event.Kafka.Config;
using System.Phoenix.Event.Messages;
using System.Phoenix.Pipeline.Orchestrator;
using System.Proxy.Fis.RegisterCard;
using System.Proxy.Fis.RegisterCard.Messages;
using System.Proxy.Salesforce;
using System.Proxy.Salesforce.GetBusinessInformation;
using System.Proxy.Salesforce.GetBusinessInformation.Message;
using System.Proxy.Salesforce.GetTerms;
using System.Proxy.Salesforce.GetTerms.Messages;
using System.Proxy.Salesforce.Messages;
using System.Proxy.Salesforce.RegisterAsset;
using System.Proxy.Salesforce.RegisterAsset.Messages;
using System.Proxy.Salesforce.Terms;
using System.Proxy.Salesforce.Terms.Messages;
using System.Proxy.Salesforce.UpdateAsset;
using System.Proxy.Silverlake.Base.Config;
using System.Proxy.Silverlake.Base.Exceptions;
using System.Proxy.Silverlake.Customer;
using System.Proxy.Silverlake.Deposit;
using System.Proxy.Silverlake.Deposit.Common;
using System.Proxy.Silverlake.Deposit.Messages.Request;
using System.Proxy.Silverlake.Deposit.Messages.Response;
using FisToken = System.Proxy.Fis.GetToken;
using SalesforceToken = System.Proxy.Salesforce.GetToken;

namespace System.Process.Application.Commands.CreateAccount
{
    public class CreateAccountCommand : INotificationHandler<CreateAccountNotification>
    {
        #region Properties
        private ICustomerOperation CustomerOperation { get; }
        private IDepositOperation DepositOperation { get; }
        private ProducerConfig ProducerConfig { get; }
        private IProducer Producer { get; }
        private IPipeline<string> Pipeline { get; }
        private ILogger<CreateAccountCommand> Logger { get; }
        private AccountMessage AccountMessage { get; set; }
        private string Topic { get; set; }
        private AccountAddRequest AccountAddRequest { get; set; }
        private SalesforceToken.Messages.GetTokenParams ConfigSalesforce { get; }
        private SalesforceToken.IGetTokenClient TokenClientSalesforce { get; }
        private FisToken.Messages.GetTokenParams ConfigFis { get; }
        private FisToken.IGetTokenClient TokenClientFis { get; }
        private IRegisterAssetClient RegisterAsset { get; }
        private RecordTypesConfig RecordTypesConfig { get; }
        private IUpdateAssetClient UpdateAssetClient { get; }
        private IRegisterCardClient RegisterCardClient { get; }
        private HeaderParams HeaderParams { get; }
        private ICardWriteRepository CardWriteRepository { get; }
        private ICardReadRepository CardReadRepository { get; }
        private ProcessConfig ProcessConfig { get; }
        private IGetTermsClient GetTermsClient { get; }
        private IGetBusinessInformationClient GetBusinessInformationClient { get; }
        private CancellationToken CancellationToken { get; set; }
        private ITermsClient TermsClient { get; }
        #endregion

        #region Constructor

        public CreateAccountCommand(
            ICustomerOperation customerOperation,
            IDepositOperation depositOperation,
            IOptions<ProducerConfig> producerConfig,
            IProducer producer,
            IPipeline<string> pipeline,
            ILogger<CreateAccountCommand> logger,
            IOptions<SalesforceToken.Messages.GetTokenParams> configSalesforce,
            SalesforceToken.IGetTokenClient tokenClientSalesforce,
            IOptions<FisToken.Messages.GetTokenParams> configFis,
            FisToken.IGetTokenClient tokenClientFis,
            IRegisterAssetClient registerAsset,
            IOptions<RecordTypesConfig> recordTypesConfig,
            IUpdateAssetClient updateAssetClient,
            IRegisterCardClient registerCardClient,
            IOptions<HeaderParams> headerParams,
            ICardWriteRepository cardWriteRepository,
            ICardReadRepository cardReadRepository,
            IOptions<ProcessConfig> ProcessConfig,
            IGetTermsClient getTermsClient,
            IGetBusinessInformationClient getBusinessInformationClient,
            ITermsClient termsClient
            )
        {
            CustomerOperation = customerOperation;
            DepositOperation = depositOperation;
            ProducerConfig = producerConfig.Value;
            Producer = producer;
            Pipeline = pipeline;
            Logger = logger;
            ConfigSalesforce = configSalesforce.Value;
            TokenClientSalesforce = tokenClientSalesforce;
            ConfigFis = configFis.Value;
            TokenClientFis = tokenClientFis;
            RegisterAsset = registerAsset;
            RecordTypesConfig = recordTypesConfig.Value;
            UpdateAssetClient = updateAssetClient;
            RegisterCardClient = registerCardClient;
            HeaderParams = headerParams.Value;
            CardWriteRepository = cardWriteRepository;
            CardReadRepository = cardReadRepository;
            ProcessConfig = ProcessConfig.Value;
            GetTermsClient = getTermsClient;
            GetBusinessInformationClient = getBusinessInformationClient;
            TermsClient = termsClient;
        }

        #endregion

        #region INotificationHandler implementation

        public async Task Handle(CreateAccountNotification notification, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation($"Message received from {notification.MessageContent.Topic} - ApplicationId: {notification.MessageContent.Payload.ApplicationId}");

                AccountMessage = notification.MessageContent.Payload;
                Topic = notification.MessageContent.Topic;
                CancellationToken = cancellationToken;

                if (AccountMessage.OpenCheckingAccount.HasValue && AccountMessage.OpenCheckingAccount.Value)
                {
                    await OpenAccount(cancellationToken);
                }
                else
                {
                    await UpdateAsset();
                    await ProduceMessageProcessCompleted();
                }

                Logger.LogInformation($"Account Successfully created. ApplicationId: {notification.MessageContent.Payload.ApplicationId}");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);

                if (ex.GetType() != typeof(UnprocessableEntityException) ||
                    ex.GetType() != typeof(DeadletterException))
                {
                    throw new UnprocessableEntityException(ex.Message);
                }

                throw;
            }
        }

        #endregion

        #region Methods

        private async Task<BaseResult<SalesforceResult>> CreateAsset(PipelineMessageContainer<CreateAccountResponse> product)
        {
            try
            {
                Logger.LogInformation("CreateAsset process initiated");
                var adapter = new RegisterAssetAdapter(RecordTypesConfig);
                var assetParams = new RegisterAssetParamsAdapter
                {
                    RoutingNumber = HeaderParams.InstitutionRoutingId,
                    AccountMessage = AccountMessage,
                    Product = product.Message,
                    RecordTypesConfig = RecordTypesConfig
                };

                var asset = adapter.Adapt(assetParams);

                Logger.LogInformation("Getting Salesforce token.");
                var accessToken = await GetSalesforceToken(CancellationToken);

                Logger.LogInformation("Initiating GetBusinessInformations request");
                var businessInformations = await GetBusinessInformations(AccountMessage.SalesforceId, accessToken, product.CancellationToken);

                if (AccountMessage?.BankAccount?.SafraDigitalTerm?.Type != null)
                {
                    Logger.LogInformation("Inserting SafraDigitalTerm to Asset");
                    await AddAssetTerm(asset, accessToken, AccountMessage.BankAccount.SafraDigitalTerm.Type, product.CancellationToken, businessInformations.Id, false);
                }

                if (AccountMessage?.BankAccount?.FeeScheduleTerm?.Type != null)
                {
                    Logger.LogInformation("Inserting FeeScheduleTerm to Asset");
                    await AddAssetTerm(asset, accessToken, AccountMessage.BankAccount.FeeScheduleTerm.Type, product.CancellationToken, businessInformations.Id, true);
                }

                if (AccountMessage?.Terms != null && AccountMessage?.Terms.Count > 0)
                {
                    Logger.LogInformation($"Terms: {AccountMessage.Terms} - SafrapyId: {AccountMessage.SalesforceId}");
                    await AddTerms(accessToken, AccountMessage.Terms, product.CancellationToken, AccountMessage.SalesforceId);
                }

                Logger.LogInformation("Initiating RegisterAsset request.");
                var result = await RegisterAsset.RegisterAsset(asset, accessToken, product.CancellationToken);

                Logger.LogInformation("CreateAsset process executed successfully");
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Execution error on CreateAsset process");
                throw;
            }
        }

        private async Task<BaseResult<SalesforceResult>> UpdateAsset(BaseResult<SalesforceResult> result = null)
        {
            try
            {
                Logger.LogInformation("CreateAsset process initiated");

                Logger.LogInformation("Getting Salesforce token");
                var accessToken = await GetSalesforceToken(CancellationToken);

                var updateAdapter = new UpdateAssetAdapter();
                var assetParams = updateAdapter.Adapt(AccountMessage.Process);
                assetParams.UpdateAssetBody.IsSettlement = !AccountMessage.BankAccount.SettlementSafra;

                if (assetParams.Id == null)
                {
                    return new BaseResult<SalesforceResult> { IsSuccess = true };
                }

                Logger.LogInformation("Initiating UpdateAsset request.");
                var response = await UpdateAssetClient.UpdateAsset(assetParams, accessToken, CancellationToken);

                if (!response.IsSuccess)
                {
                    throw new UnprocessableEntityException("Cannot update asset in Salesforce.", response.Message);
                }

                Logger.LogInformation("UpdateAsset process executed successfully");
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Execution error on UpdateAsset process");
                throw;
            }
        }

        private async Task OpenAccount(CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("OpenAccount process initiated");

                Logger.LogInformation("Initiating GetProduct request");
                var productResponse = GetProduct();

                Logger.LogInformation("Initiating Pipeline");
                await ExecutePipeline(productResponse, cancellationToken);

                Logger.LogInformation("Fetching Pipeline result");
                var responseAccount = Pipeline.FetchOutput<PipelineMessageContainer<CreateAccountResponse>>("AddProcess");

                var Process = responseAccount.Message.AccountIdList.Select(
                        acc => new AccountInfo
                        {
                            Number = acc.AccountNumber,
                            RoutingNumber = HeaderParams.InstitutionRoutingId,
                            Type = productResponse.AccountType,
                            Origin = OriginAccount.S
                        }
                    ).ToList();

                Logger.LogInformation($"Inserting Process created to {AccountMessage.ApplicationId}");
                Process.AddRange(AccountMessage.Process.ToList());

                AccountMessage.Process = Process;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Execution error on OpenAccount process");
                throw;
            }
        }

        private async Task ExecutePipeline(ProductMessage product, CancellationToken cancellationToken)
        {
            Logger.LogInformation("ExecutePipeline process initiated");
            var adapter = new CreateAccountAdapter();

            var accountParams = new CreateAccountParamsAdapter()
            {
                Request = product,
                Message = AccountMessage
            };

            var messageContainer = new PipelineMessageContainer<ProductMessage>(product, cancellationToken);

            AccountAddRequest = adapter.Adapt(accountParams);

            Logger.LogInformation("Configuring Pipeline for CreateAccountCommand");
            Pipeline.ConfigurePipeline(AccountMessage.ApplicationId, "CreateAccount");

            Pipeline.AddStep<PipelineMessageContainer<ProductMessage>, PipelineMessageContainer<List<AccountInformation>>>(AccountIdGeneration, cancellationToken);
            Pipeline.AddStep<PipelineMessageContainer<List<AccountInformation>>, PipelineMessageContainer<CreateAccountResponse>>(AddProcess, cancellationToken);
            Pipeline.AddStep<PipelineMessageContainer<CreateAccountResponse>, BaseResult<SalesforceResult>>(CreateAsset, cancellationToken);
            Pipeline.AddStep<BaseResult<SalesforceResult>, BaseResult<SalesforceResult>>(UpdateAsset, cancellationToken);
            Pipeline.AddStep<BaseResult<SalesforceResult>, Proxy.Fis.Messages.BaseResult<RegisterCardResult>>(AddCard, cancellationToken);
            Pipeline.AddStep<Proxy.Fis.Messages.BaseResult<RegisterCardResult>, Proxy.Fis.Messages.BaseResult<RegisterCardResult>>(ProduceMessageProcessCompleted, cancellationToken);
            Pipeline.CreatePipeline<Proxy.Fis.Messages.BaseResult<RegisterCardResult>>(info => Logger.LogInformation("Account Created Successfully"));

            Logger.LogInformation("Executing Pipeline for CreateAccountCommand");
            await Pipeline.Execute(messageContainer);
        }

        private async Task<PipelineMessageContainer<List<AccountInformation>>> AccountIdGeneration(PipelineMessageContainer<ProductMessage> product)
        {
            try
            {
                Logger.LogInformation("AccountIdGeneration process initiated");

                var accountIds = new List<AccountInformation>();
                var adapter = new AccountIdGeneratorAdapter(ProcessConfig);

                var accountParams = new CreateAccountParamsAdapter()
                {
                    Request = product.Message,
                    Message = AccountMessage
                };

                var accountIdRequestAdapt = adapter.Adapt(accountParams);


                //CONFIGURACAO DO PRODUTO
                accountIdRequestAdapt.AccountType = ProcessConfig.AccountType;
                accountIdRequestAdapt.ProductCode = ProcessConfig.AccountCommandProductCode;
                accountIdRequestAdapt.BusinessDetail.BranchCode = ProcessConfig.AccountCommandBranchCode;
                accountIdRequestAdapt.BusinessDetail.OfficerCode = ProcessConfig.OfficerCode;

                Logger.LogInformation("Initiating AccountIdGeneratorAsync request");
                var result = await CustomerOperation.AccountIdGeneratorAsync(accountIdRequestAdapt, product.CancellationToken);

                result.AccountIdGeneratorInfo.ForEach(
                    item =>
                    {
                        accountIds.Add(new AccountInformation
                        {
                            AccountNumber = item.AccountId,
                            ProductCode = item.ProductCode
                        });
                    }
                );

                Logger.LogInformation("AccountIdGeneration process executed successfully");
                return new PipelineMessageContainer<List<AccountInformation>>(accountIds, product.CancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Execution error on AccountIdGeneration process");
                throw;
            }
        }

        private PipelineMessageContainer<CreateAccountResponse> AddProcess(PipelineMessageContainer<List<AccountInformation>> input)
        {
            try
            {
                Logger.LogInformation("AddProcess process initiated");

                List<Task> tasks = new List<Task>();
                foreach (AccountInformation item in input.Message)
                {
                    tasks.Add(AddAccount(item.AccountNumber, input.CancellationToken));
                }
                Task.WhenAll(tasks)
                    .Wait();

                var response = new CreateAccountResponse
                {
                    ProcessId = AccountMessage.ApplicationId,
                    AccountIdList = input.Message
                };

                Logger.LogInformation("AddProcess process executed successfully");
                return new PipelineMessageContainer<CreateAccountResponse>(response, input.CancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Execution error on AddProcess process");
                throw;
            }
        }

        private async Task<AccountAddResponse> AddAccount(string accountNumber, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("AddAccount process initiated");
                AccountAddRequest.AccountId = new AccountId
                {
                    AccountNumber = accountNumber,
                    AccountType = ProcessConfig.AccountType,
                };

                AccountAddRequest.ProductCode = ProcessConfig.AccountCommandProductCode;
                AccountAddRequest.BranchCode = ProcessConfig.AccountCommandBranchCode;
                AccountAddRequest.DepositAdd.DepositInformationRecord.AccountClassificationCode = ProcessConfig.AccountClassificationCode;
                AccountAddRequest.DepositAdd.DepositNonSufficientOverdraftsInfo.ChargeODCode = ProcessConfig.ChargeODCode;
                AccountAddRequest.DepositAdd.DepositInformationRecord.ProdCode = ProcessConfig.AccountCommandProductCode;
                AccountAddRequest.DepositAdd.DepositInformationRecord.BranchCode = ProcessConfig.AccountCommandBranchCode;
                AccountAddRequest.DepositAdd.DepositInformationRecord.OpenDate = DateTime.Today;

                Logger.LogInformation("Initiating AccountAddAsync request");
                return await DepositOperation.AccountAddAsync(AccountAddRequest, cancellationToken);
            }
            catch (SilverlakeException ex)
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine($"Error while creating account for {AccountMessage.ApplicationId}");
                errorMessage.AppendLine("Error List: ");

                foreach (var error in ex.Errors)
                {
                    errorMessage.AppendLine(error);
                }

                errorMessage = errorMessage.Replace("One or more errors occurred. (", "").Replace(")", "");

                Logger.LogError("Execution error on AddAccount process");
                throw new DeadletterException(ex.Message, "Error while creating account", Topic, $"Details: {errorMessage}");

            }
        }

        private ProductMessage GetProduct()
        {
            var product = new ProductMessage()
            {
                AccountType = ProcessConfig.AccountType,
                ProductCode = ProcessConfig.AccountCommandProductCode,
                QuantityOfNumberProcess = int.Parse(ProcessConfig.QuantityOfNumberProcess),
                BranchCode = ProcessConfig.AccountCommandBranchCode,
                DepositAdd = new DepositAddDto()
                {
                    DepositInformationRecord = new DepositInformationRecordDto()
                    {
                        AccountClassificationCode = ProcessConfig.AccountClassificationCode,
                        ServiceChargeWaived = ProcessConfig.ServiceChargeWaived,
                        SignatureVerificationCode = ProcessConfig.SignatureVerificationCode
                    },
                    DepositStatementInfo = new DepositStatementDto()
                    {
                        IncludeCombinedStatement = ProcessConfig.IncludeCombinedStatement,
                        ItemTruncation = ProcessConfig.ItemTruncation,
                        ImagePrintCheckOrderCode = ProcessConfig.ImagePrintCheckOrderCode,
                        NextStatementDate = DateTime.Now.AddMonths(1).ToString("yyyy-MM-dd"),
                        StatementFrequency = int.Parse(ProcessConfig.StatementFrequency),
                        StatementFrequencyCode = ProcessConfig.StatementFrequencyCode,
                        StatementPrintCode = ProcessConfig.StatementPrintCode,
                        StatementServiceCharge = ProcessConfig.StatementServiceCharge,
                        StatementCreditInterest = Constants.NoCrInt
                    },
                    DepositNonSufficientOverdraftsInfo = new DepositOverdrawInformationDto()
                    {
                        ChargeODCode = ProcessConfig.ChargeODCode,
                        AllowReDepositCode = ProcessConfig.AllowReDepositCode,
                        RedepositNoticeCode = ProcessConfig.RedepositNoticeCode,
                        NumberAllowedRedepositItems = int.Parse(ProcessConfig.NumberAllowedRedepositItems)
                    },
                    DepositAccountInfo = new DepositAccountInformationDto()
                    {
                        CheckGuaranty = ProcessConfig.CheckGuaranty,
                        ATMCard = ProcessConfig.ATMCard,
                        CloseOnZeroBalance = ProcessConfig.CloseOnZeroBalance,
                        HighVolumeAccountCode = ProcessConfig.HighVolumeAccountCode,
                        LstPostAccountCode = ProcessConfig.LstPostAccountCode
                    }
                }
            };
            return product;
        }

        private async Task<Proxy.Fis.Messages.BaseResult<RegisterCardResult>> AddCard(BaseResult<SalesforceResult> result = null)
        {
            try
            {
                Logger.LogInformation($"Starting the FIS GetToken request [{AccountMessage.SalesforceId}]");
                var authToken = await TokenClientFis.GetToken(ConfigFis, CancellationToken);

                var addCardAdapter = new CreateCardAdapter(ProcessConfig);

                var cardParams = new CreateCardParamsAdapter()
                {
                    Message = AccountMessage
                };

                var addCardParams = addCardAdapter.Adapt(cardParams);

                Logger.LogInformation($"Starting the RegisterDebitCard request [{AccountMessage.SalesforceId}] \n {System.Text.Json.JsonSerializer.Serialize(addCardParams)}");
                var response = await RegisterCardClient.RegisterCardAsync(addCardParams, authToken.Result.AccessToken, CancellationToken);

                if (!response.IsSuccess || response.Result.Metadata.MessageList.FirstOrDefault().Text != "SUCCESS")
                {
                    Logger.LogInformation($"Cannot create new card { JsonConvert.SerializeObject(response?.Result?.Metadata?.MessageList?.First()?.Text) } ");
                    throw new UnprocessableEntityException("Cannot create new card", response?.Result?.Metadata?.MessageList?.First()?.Text);
                }

                var cardHolder = cardParams.Message.Principals.FirstOrDefault().FirstName + " " + cardParams.Message?.Principals.FirstOrDefault().LastName;

                if (cardHolder.Length > 26)
                {
                    cardHolder = cardHolder.Substring(0, 26);
                }

                var requestCard = new AddCardRequest
                {
                    CustomerId = cardParams.Message?.SalesforceId,
                    Pan = response.Result?.Entity?.Pan?.PlainText,
                    Bin = ProcessConfig.Bin,
                    BusinessName = cardParams.Message?.BusinessInformation?.LegalName,
                    CardHolder = cardHolder,
                    ExpirationDate = response.Result?.Entity?.Card?.ExpirationDate,
                    CardType = ProcessConfig.CardTypeHostValue,
                    CardStatus = Constants.PendingActivationStatus,
                    LastFour = response.Result?.Entity?.Pan?.PlainText.Substring(12, 4)
                };

                var adapter = new AddCardAdapter(ProcessConfig);
                var adaptedRequest = adapter.Adapt(requestCard);

                Logger.LogInformation("Checking if the card already exists");
                var cards = CardReadRepository.FindExistent(adaptedRequest);

                if (cards.Count > 0)
                {
                    Logger.LogInformation("Card already exists in database");
                    throw new UnprocessableEntityException("Card already exists in database");
                }

                var SystemId = await SaveDebitCardSalesforce(requestCard, CancellationToken);

                adaptedRequest.AssetId = SystemId;

                Logger.LogInformation($"Starting Add Card in Oracle DB [{adaptedRequest.CustomerId}] \n {System.Text.Json.JsonSerializer.Serialize(adaptedRequest)}");
                await CardWriteRepository.Add(adaptedRequest, CancellationToken);

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error during the AddCard process");
                throw;
            }
        }

        private async Task<string> SaveDebitCardSalesforce(AddCardRequest input, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation($"Starting the SaveDebitCardSalesforce request [{input.CustomerId}] \n {System.Text.Json.JsonSerializer.Serialize(input)}");
                var adapter = new RegisterAssetAdapter(RecordTypesConfig);
                var asset = adapter.Adapt(input);

                Logger.LogInformation("Initiating GetSalesforceToken request");
                var accessToken = await GetSalesforceToken(cancellationToken);

                Logger.LogInformation("Initiating GetBusinessInformations request");
                var businessInformations = await GetBusinessInformations(AccountMessage.SalesforceId, accessToken, cancellationToken);

                asset.ContactId = businessInformations.Id;

                var result = await RegisterAsset.RegisterAsset(asset, accessToken, cancellationToken);

                return result.Result.Id;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Execution error on SaveDebitCardSalesforce process");
                throw;
            }

        }

        private async Task AddAssetTerm(RegisterAssetParams asset, string token, string termType, CancellationToken cancellationToken, string signerId, bool isFeeSchedule)
        {
            Logger.LogInformation("AddAssetTerm process initiated");

            Logger.LogInformation("Initiating GetTermsSalesforce request");
            var term = await GetTermsSalesforce(termType, token, cancellationToken);

            if (asset != null && term != null)
            {
                if (isFeeSchedule)
                {
                    asset.FeeScheduleTermVersion = term.Id;
                    asset.FeeScheduleTermSigner = signerId;
                    asset.FeeScheduleTermSignDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
                }
                else
                {
                    asset.TermAndConditionType = term.Type;
                    asset.TermAndConditionVersion = term.Id;
                    asset.TermAndConditionSigner = signerId;
                    asset.TermAndConditionSignDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
                }
            }
        }

        private async Task<Terms> GetTermsSalesforce(string termType, string accessToken, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("GetTermsSalesforce process initiated");
                var req = new GetTermsParams
                {
                    OriginalChannel = OriginChannelConstants.Application,
                    TermType = termType
                };

                Logger.LogInformation("Initiating GetTerms request");
                var result = await GetTermsClient.GetTerms(req, accessToken, cancellationToken);
                var response = new Terms();
                var salesforceTerms = result.Result.Records?.ToList();

                if (salesforceTerms != null && salesforceTerms?.Count > 0)
                {
                    var version = salesforceTerms.Max(st => st.Version);
                    response = salesforceTerms.Find(t => t.Version == version);
                }

                Logger.LogInformation("GetTermsSalesforce process executed successfully");
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Execution error on GetTermsSalesforce process");
                throw;
            }

        }

        private async Task<GetBusinessInformationResponse> GetBusinessInformations(string SystemId, string accessToken, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("GetBusinessInformations process initiated");
                var getBusinessInformationParams = new GetBusinessInformationParams
                {
                    SystemId = SystemId
                };

                Logger.LogInformation("Initiating GetBusinessInformation request");
                var result = await GetBusinessInformationClient.GetBusinessInformation(getBusinessInformationParams, accessToken, cancellationToken);

                Logger.LogInformation("GetBusinessInformation process executed successfully");
                return result.Result.Records.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Execution error on GetBusinessInformations process");
                throw;
            }
        }

        private async Task<Proxy.Fis.Messages.BaseResult<RegisterCardResult>> ProduceMessageProcessCompleted(Proxy.Fis.Messages.BaseResult<RegisterCardResult> result = null)
        {
            Logger.LogInformation("ProduceMessageProcessCompleted process initiated");

            AccountMessage.ProcessStep = ProcessStep.AccountCreated;

            var messageContent = new MessageContent<AccountMessage>()
            {
                Topic = ProducerConfig.Topic,
                Payload = AccountMessage
            };

            Logger.LogInformation($"Account Message updated - { AccountMessage }");

            Logger.LogInformation("Initiating Producer.Produce request");
            await Producer.Produce(messageContent);

            Logger.LogInformation($"{ProducerConfig.Topic} - Step { AccountMessage.ProcessStep } Success - ApplicationId: { AccountMessage.ApplicationId }");

            Logger.LogInformation("ProduceMessageProcessCompleted process executed successfully");
            return result;
        }

        private async Task<string> GetSalesforceToken(CancellationToken cancellationToken)
        {
            var getTokenResult = await TokenClientSalesforce.GetToken(ConfigSalesforce, cancellationToken);

            if (!getTokenResult.IsSuccess)
            {
                Logger.LogError("Execution error on GetSalesforceToken process");
                throw new UnprocessableEntityException($"Could not get Salesforce Token. Error message: {getTokenResult.ErrorMessage}");
            }

            return getTokenResult.Result.AccessToken;
        }

        private async Task AddTerms(string token, IList<Domain.ValueObjects.Term> terms, CancellationToken cancellationToken, string SystemId)
        {
            if (terms != null && terms.Count > 0)
            {
                var termsParams = new List<Proxy.Salesforce.Terms.Messages.Term>();

                var validTerms = terms.Where(term => term.Type != null && term.Type.Equals("DigitalBankPrivacyPolicy")).ToList();

                validTerms.ForEach(term => { termsParams.Add(new Proxy.Salesforce.Terms.Messages.Term() { Id = term.Version }); });

                if (termsParams.Count > 0)
                {
                    var registerTermsParams = new RegisterTermsParams()
                    {
                        Request = new Request()
                        {
                            SystemId = SystemId,
                            Terms = termsParams
                        }
                    };
                    await TermsClient.RegisterTerms(registerTermsParams, token, cancellationToken);
                }
            }
        }

        #endregion
    }
}
