using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Process.Application.Commands.CreateCustomerId.Adapters;
using System.Process.Domain.Containers;
using System.Process.Domain.Enums;
using System.Process.Infrastructure.Configs;
using System.Process.Infrastructure.Messages;
using System.Phoenix.Common.Exceptions;
using System.Phoenix.Event;
using System.Phoenix.Event.Kafka.Config;
using System.Phoenix.Event.Messages;
using System.Phoenix.Pipeline.Orchestrator;
using System.Proxy.RdaAdmin.AddAccount;
using System.Proxy.RdaAdmin.AddCutosmer;
using System.Proxy.RdaAdmin.GetProcessCriteriaReference;
using System.Proxy.RdaAdmin.GetProcessCriteriaReference.Messages;
using System.Proxy.RdaAdmin.GetCustomersCriteria;
using System.Proxy.RdaAdmin.GetCustomersCriteria.Messages;
using System.Proxy.RdaAdmin.Messages;
using System.Proxy.RdaAdmin.UpdateAccount;
using System.Proxy.RdaAdmin.UpdateCustomer;
using System.Proxy.Salesforce;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.Messages;
using System.Proxy.Salesforce.UpdateAsset;

namespace System.Process.Application.Commands.CreateCustomerId
{
    public class CreateCustomerIdCommand : INotificationHandler<CreateCustomerIdNotification>
    {
        #region Properties
        private RdaCredentialsConfig RdaConfig { get; set; }
        private IProducer Producer { get; }
        private ProducerConfig ProducerConfig { get; }
        private ILogger<CreateCustomerIdCommand> Logger { get; }
        private IGetCustomersCriteriaClient GetCustomersCriteriaClient { get; }
        private IGetProcessCriteriaReferenceClient GetProcessCriteriaReferenceClient { get; }
        private IAddCustomerClient AddCustomerClient { get; }
        private IAddAccountClient AddAccountClient { get; }
        private IUpdateCustomerClient UpdateCustomerClient { get; }
        private IUpdateAccountClient UpdateAccountClient { get; }
        private IUpdateAssetClient UpdateAssetClient { get; }
        private IGetTokenClient GetTokenClient { get; }
        private GetTokenParams ConfigSalesforce { get; }
        private IPipeline<string> Pipeline { get; }
        private PipelineMessageContainer<AccountMessage> AccountMessage { get; set; }

        #endregion

        #region Constructor

        public CreateCustomerIdCommand(
            IOptions<RdaCredentialsConfig> rdaConfig,
            IProducer producer,
            IOptions<ProducerConfig> producerConfig,
            ILogger<CreateCustomerIdCommand> logger,
            IPipeline<string> pipeline,
            IGetCustomersCriteriaClient getCustomersCriteriaClient,
            IAddCustomerClient addCustomerClient,
            IAddAccountClient addAccountClient,
            IUpdateCustomerClient updateCustomerClient,
            IGetProcessCriteriaReferenceClient getProcessCriteriaReferenceClient,
            IUpdateAccountClient updateAccountClient,
            IGetTokenClient getTokenClient,
            IUpdateAssetClient updateAssetClient,
            IOptions<GetTokenParams> getTokenParams
            )
        {
            RdaConfig = rdaConfig.Value;
            Producer = producer;
            ProducerConfig = producerConfig.Value;
            Logger = logger;
            Pipeline = pipeline;
            GetCustomersCriteriaClient = getCustomersCriteriaClient;
            AddCustomerClient = addCustomerClient;
            AddAccountClient = addAccountClient;
            UpdateCustomerClient = updateCustomerClient;
            GetProcessCriteriaReferenceClient = getProcessCriteriaReferenceClient;
            UpdateAccountClient = updateAccountClient;
            GetTokenClient = getTokenClient;
            UpdateAssetClient = updateAssetClient;
            ConfigSalesforce = getTokenParams.Value;
        }

        #endregion

        #region INotificationHandler implementation

        public async Task Handle(CreateCustomerIdNotification notification, CancellationToken cancellationToken)
        {
            try
            {
                AccountMessage = new PipelineMessageContainer<AccountMessage>
                {
                    Message = notification.MessageContent.Payload,
                    CancellationToken = cancellationToken
                };

                if (AccountMessage.Message.OpenCheckingAccount.HasValue && AccountMessage.Message.OpenCheckingAccount.Value)
                {
                    Logger.LogInformation("Start customer Rda Creation");
                    await ExecutePipeline(AccountMessage);
                }

                AccountMessage.Message.ProcessStep = ProcessStep.CustomerRdaCreated;

                var messageContent = new MessageContent<AccountMessage>()
                {
                    Topic = ProducerConfig.Topic,
                    Payload = AccountMessage.Message
                };

                Producer.Produce(messageContent).Wait();

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new UnprocessableEntityException(ex.Message);
            }
        }

        #endregion

        #region Methods

        private async Task ExecutePipeline(PipelineMessageContainer<AccountMessage> message)
        {
            Pipeline.ConfigurePipeline(message.Message.ApplicationId, "CreateCustomerId");

            var messageContainer = new PipelineMessageContainer<AccountMessage>(message.Message, message.CancellationToken);

            Pipeline.AddStep<PipelineMessageContainer<AccountMessage>, PipelineMessageContainer<AdminBaseResult<GetCustomersCriteriaResponse>>>(ValidateCustomer, message.CancellationToken);
            Pipeline.AddStep<PipelineMessageContainer<AdminBaseResult<GetCustomersCriteriaResponse>>, PipelineMessageContainer<AdminBaseResult<RdaAdminResult>>>(CreateCustomer, message.CancellationToken);
            Pipeline.AddStep<PipelineMessageContainer<AdminBaseResult<RdaAdminResult>>, PipelineMessageContainer<AdminBaseResult<RdaAdminResult>>>(ActivateCustomer, message.CancellationToken);
            Pipeline.AddStep<PipelineMessageContainer<AdminBaseResult<RdaAdminResult>>, PipelineMessageContainer<AdminBaseResult<GetProcessCriteriaReferenceResponse>>>(ValidateProcess, message.CancellationToken);
            Pipeline.AddStep<PipelineMessageContainer<AdminBaseResult<GetProcessCriteriaReferenceResponse>>, PipelineMessageContainer<AdminBaseResult<RdaAdminResult>>>(CreateAccount, message.CancellationToken);
            Pipeline.AddStep<PipelineMessageContainer<AdminBaseResult<RdaAdminResult>>, PipelineMessageContainer<AdminBaseResult<RdaAdminResult>>>(UpdateAccount, message.CancellationToken);
            //Pipeline.AddStep<PipelineMessageContainer<AdminBaseResult<RdaAdminResult>>, BaseResult<SalesforceResult>>(UpdateAsset, message.CancellationToken);

            Pipeline.CreatePipeline<PipelineMessageContainer<AdminBaseResult<RdaAdminResult>>>(info => Logger.LogInformation("CustomerId Created Successfully"));

            await Pipeline.Execute(messageContainer);
        }

        private async Task<PipelineMessageContainer<AdminBaseResult<GetCustomersCriteriaResponse>>> ValidateCustomer(PipelineMessageContainer<AccountMessage> input)
        {
            Logger.LogInformation($"Step Validate Customer Started - ApplicationId: { AccountMessage.Message.ApplicationId }");

            try
            {
                var adapter = new GetCustomersCriteriaAdapter(RdaConfig);
                var adapt = adapter.Adapt(input.Message);
                var result = await GetCustomersCriteriaClient.GetCustomersCriteria(adapt, input.CancellationToken);

                if (result.Result.Result != 1)
                {
                    Logger.LogInformation($"Cannot Validate Customer in Rda: { JsonConvert.SerializeObject(result?.Result) } ");
                    throw new UnprocessableEntityException("Cannot Validate Customer in Rda", result?.Result?.ValidationResults?.First().ToString());
                }

                Logger.LogInformation($"Step Validate Customer Success - ApplicationId: { AccountMessage.Message.ApplicationId }");

                return new PipelineMessageContainer<AdminBaseResult<GetCustomersCriteriaResponse>>
                {
                    Message = result,
                    CancellationToken = input.CancellationToken
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new UnprocessableEntityException(ex.Message);
            }
        }

        private async Task<PipelineMessageContainer<AdminBaseResult<RdaAdminResult>>> CreateCustomer(PipelineMessageContainer<AdminBaseResult<GetCustomersCriteriaResponse>> input)
        {
            Logger.LogInformation($"Step Create Customer Started - ApplicationId: { AccountMessage.Message.ApplicationId }");

            try
            {
                if (input.Message.Result.Customers.Count <= 0)
                {
                    var adapter = new AddCustomerAdapter(RdaConfig);
                    var adapt = adapter.Adapt(AccountMessage.Message);
                    var result = await AddCustomerClient.AddCustomer(adapt, input.CancellationToken);

                    if (result.Result.Result != 1)
                    {
                        Logger.LogInformation($"Cannot Create Customer in Rda: { JsonConvert.SerializeObject(result?.Result) }");
                        throw new UnprocessableEntityException("Cannot Create Customer in Rda", result?.Result?.ValidationResults?.First().ToString());
                    }

                    Logger.LogInformation($"Step Create Customer Success - ApplicationId: { AccountMessage.Message.ApplicationId }");

                    return new PipelineMessageContainer<AdminBaseResult<RdaAdminResult>>
                    {
                        Message = result,
                        CancellationToken = input.CancellationToken
                    };
                }

                return new PipelineMessageContainer<AdminBaseResult<RdaAdminResult>>
                {
                    Message = null,
                    CancellationToken = input.CancellationToken
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new UnprocessableEntityException(ex.Message);
            }
        }

        private async Task<PipelineMessageContainer<AdminBaseResult<RdaAdminResult>>> ActivateCustomer(PipelineMessageContainer<AdminBaseResult<RdaAdminResult>> input)
        {
            Logger.LogInformation($"Step Activate Customer Started - ApplicationId: { AccountMessage.Message.ApplicationId }");

            try
            {
                var customer = Pipeline.FetchOutput<PipelineMessageContainer<AdminBaseResult<GetCustomersCriteriaResponse>>>("ValidateCustomer");

                if (input.Message == null && customer.Message.Result.Customers.First().IsEnabled == false)
                {
                    var adapter = new UpdateCustomerAdapter(RdaConfig);
                    var adapt = adapter.Adapt(customer);
                    var result = await UpdateCustomerClient.UpdateCustomer(adapt, input.CancellationToken);

                    if (result.Result.Result != 1)
                    {
                        Logger.LogInformation($"Cannot Activate Customer in Rda: { JsonConvert.SerializeObject(result?.Result) }");
                        throw new UnprocessableEntityException("Cannot Activate Customer in Rda", result?.Result?.ValidationResults?.First().ToString());
                    }

                    Logger.LogInformation($"Step Activate Customer Success - ApplicationId: { AccountMessage.Message.ApplicationId }");

                    return new PipelineMessageContainer<AdminBaseResult<RdaAdminResult>>
                    {
                        Message = result,
                        CancellationToken = input.CancellationToken
                    };
                }

                return new PipelineMessageContainer<AdminBaseResult<RdaAdminResult>>
                {
                    Message = null,
                    CancellationToken = input.CancellationToken
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new UnprocessableEntityException(ex.Message);
            }
        }

        private async Task<PipelineMessageContainer<AdminBaseResult<GetProcessCriteriaReferenceResponse>>> ValidateProcess(PipelineMessageContainer<AdminBaseResult<RdaAdminResult>> input)
        {
            Logger.LogInformation($"Step Validate Process Started - ApplicationId: { AccountMessage.Message.ApplicationId }");

            try
            {
                if (input.Message != null)
                {
                    var adapter = new GetProcessCriteriaReferenceAdapter(RdaConfig);
                    var adapt = adapter.Adapt(AccountMessage.Message);
                    var result = await GetProcessCriteriaReferenceClient.GetProcessCriteriaReference(adapt, input.CancellationToken);

                    if (result.Result.Result != 1)
                    {
                        Logger.LogInformation($"Cannot Validate Account in Rda: { JsonConvert.SerializeObject(result?.Result) }");
                        throw new UnprocessableEntityException("Cannot Validate Account in Rda", result?.Result?.ValidationResults?.First().ToString());
                    }

                    Logger.LogInformation($"Step Validate Customer Success - ApplicationId: { AccountMessage.Message.ApplicationId }");

                    return new PipelineMessageContainer<AdminBaseResult<GetProcessCriteriaReferenceResponse>>
                    {
                        Message = result,
                        CancellationToken = input.CancellationToken
                    };
                }

                return new PipelineMessageContainer<AdminBaseResult<GetProcessCriteriaReferenceResponse>>
                {
                    Message = null,
                    CancellationToken = input.CancellationToken
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new UnprocessableEntityException(ex.Message);
            }
        }

        private async Task<PipelineMessageContainer<AdminBaseResult<RdaAdminResult>>> CreateAccount(PipelineMessageContainer<AdminBaseResult<GetProcessCriteriaReferenceResponse>> input)
        {
            Logger.LogInformation($"Step Create Account Started - ApplicationId: { AccountMessage.Message.ApplicationId }");

            try
            {
                if (input.Message == null || input.Message.Result.Process.Count <= 0)
                {
                    var adapter = new AddAccountAdapter(RdaConfig);
                    var adapt = adapter.Adapt(AccountMessage.Message);
                    var result = await AddAccountClient.AddAccount(adapt, input.CancellationToken);

                    if (result.Result.Result != 1)
                    {
                        Logger.LogInformation($"Cannot Create Account in Rda: { JsonConvert.SerializeObject(result?.Result) }");
                        throw new UnprocessableEntityException("Cannot Create Account in Rda", result?.Result?.ValidationResults?.First().ToString());
                    }

                    Logger.LogInformation($"Step Create Account Success - ApplicationId: { AccountMessage.Message.ApplicationId }");

                    return new PipelineMessageContainer<AdminBaseResult<RdaAdminResult>>
                    {
                        Message = result,
                        CancellationToken = input.CancellationToken
                    };
                }
                return new PipelineMessageContainer<AdminBaseResult<RdaAdminResult>>
                {
                    Message = null,
                    CancellationToken = input.CancellationToken
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new UnprocessableEntityException(ex.Message);
            }
        }

        private async Task<PipelineMessageContainer<AdminBaseResult<RdaAdminResult>>> UpdateAccount(PipelineMessageContainer<AdminBaseResult<RdaAdminResult>> input)
        {
            Logger.LogInformation($"Step Update Account Started - ApplicationId: { AccountMessage.Message.ApplicationId }");

            try
            {
                var Process = Pipeline.FetchOutput<PipelineMessageContainer<AdminBaseResult<GetProcessCriteriaReferenceResponse>>>("ValidateProcess");

                if (input.Message == null && Process.Message.Result.Process.Count > 0 && Process.Message.Result.Process.First().IsEnabled == false)
                {
                    var adapter = new UpdateAccountAdapter(RdaConfig);
                    var adapt = adapter.Adapt(AccountMessage.Message);
                    var result = await UpdateAccountClient.UpdateAccount(adapt, input.CancellationToken);

                    if (result.Result.Result != 1)
                    {
                        Logger.LogInformation($"Cannot Update Account in Rda: { JsonConvert.SerializeObject(result?.Result) }");
                        throw new UnprocessableEntityException("Cannot Update Account in Rda", result?.Result?.ValidationResults?.First().ToString());
                    }

                    Logger.LogInformation($"Step Update Account Success - ApplicationId: { AccountMessage.Message.ApplicationId }");

                    return new PipelineMessageContainer<AdminBaseResult<RdaAdminResult>>
                    {
                        Message = result,
                        CancellationToken = input.CancellationToken
                    };
                }
                else
                {
                    return new PipelineMessageContainer<AdminBaseResult<RdaAdminResult>>
                    {
                        Message = null,
                        CancellationToken = input.CancellationToken
                    };
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new UnprocessableEntityException(ex.Message);
            }
        }

        private async Task<BaseResult<SalesforceResult>> UpdateAsset(PipelineMessageContainer<AdminBaseResult<RdaAdminResult>> input)
        {
            Logger.LogInformation($"Step Update Asset Started - ApplicationId: { AccountMessage.Message.ApplicationId }");

            try
            {
                var authToken = await GetTokenClient.GetToken(ConfigSalesforce, input.CancellationToken);
                var adapter = new UpdateAssetAdapter();
                var adapt = adapter.Adapt(AccountMessage.Message);
                var result = await UpdateAssetClient.UpdateAsset(adapt, authToken.Result.AccessToken, input.CancellationToken);

                if (!result.Result.Success == true)
                {
                    Logger.LogInformation($"Cannot Update Asset in Salesforce: { JsonConvert.SerializeObject(result?.Result) }");
                    throw new UnprocessableEntityException("Cannot Update Asset in Salesforce", result?.Result?.Errors?.First()?.Message);
                }

                Logger.LogInformation($"Step Update Asset Success - ApplicationId: { AccountMessage.Message.ApplicationId }");

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
