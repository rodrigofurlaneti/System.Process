using System;
using System.Net.Http;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Process.Infrastructure.Configs;
using System.Phoenix.Communication.Soap;
using System.Phoenix.DependencyInjection.Activation;
using System.Proxy.Feedzai.Base;
using System.Proxy.Feedzai.Base.Config;
using System.Proxy.Feedzai.TransferInitiation;
using System.Proxy.Fis.ActivateCard;
using System.Proxy.Fis.CardReplace;
using System.Proxy.Fis.ChangeCardPin;
using System.Proxy.Fis.ChangeCardStatus;
using System.Proxy.Fis.Config;
using System.Proxy.Fis.LockUnlock;
using System.Proxy.Fis.RegisterCard;
using System.Proxy.Fis.ReissueCard;
using System.Proxy.Fis.SearchTransactions;
using System.Proxy.FourSight.StatementGenerate;
using System.Proxy.FourSight.StatementGenerateInquiry;
using System.Proxy.FourSight.StatementSearch;
using System.Proxy.Rda;
using System.Proxy.Rda.AddItem;
using System.Proxy.Rda.Authenticate;
using System.Proxy.Rda.Common.Config;
using System.Proxy.Rda.CreateBatch;
using System.Proxy.Rda.UpdateBatch;
using System.Proxy.RdaAdmin;
using System.Proxy.RdaAdmin.AddAccount;
using System.Proxy.RdaAdmin.AddCustomer;
using System.Proxy.RdaAdmin.AddCutosmer;
using System.Proxy.RdaAdmin.Common.Config;
using System.Proxy.RdaAdmin.GetProcessCriteria;
using System.Proxy.RdaAdmin.GetProcessCriteriaReference;
using System.Proxy.RdaAdmin.GetCustomersCriteria;
using System.Proxy.RdaAdmin.UpdateAccount;
using System.Proxy.RdaAdmin.UpdateCustomer;
using System.Proxy.Rtdx.BalanceInquiry;
using System.Proxy.Rtdx.CardActivation;
using System.Proxy.Rtdx.CommercialCompanyAdd;
using System.Proxy.Rtdx.Config;
using System.Proxy.Rtdx.GetToken;
using System.Proxy.Rtdx.NewAccountAdd;
using System.Proxy.Rtdx.OrderNewPlastic;
using System.Proxy.Rtdx.PaymentInquiry;
using System.Proxy.Rtdx.PendingActivityDetails;
using System.Proxy.Rtdx.TransactionDetails;
using System.Proxy.Salesforce.Config;
using System.Proxy.Salesforce.GetAccountInformations;
using System.Proxy.Salesforce.GetBusinessInformation;
using System.Proxy.Salesforce.GetCommercialCompanyInfo;
using System.Proxy.Salesforce.GetCreditCards;
using System.Proxy.Salesforce.GetCustomerInformations;
using System.Proxy.Salesforce.GetRecordType;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.RegisterAsset;
using System.Proxy.Salesforce.SearchAddress;
using System.Proxy.Salesforce.UpdateAsset;
using System.Proxy.Silverlake.Customer;
using System.Proxy.Silverlake.Deposit;
using System.Proxy.Silverlake.Inquiry;
using System.Proxy.Silverlake.TranferWire;
using System.Proxy.Silverlake.Transaction;
using FisToken = System.Proxy.Fis.GetToken;
using FisTransaction = System.Proxy.Fis.TransactionDetail;
using SalesforceUpdateAccount = System.Proxy.Salesforce.UpdateAccount;
using FourSightConfig = System.Proxy.FourSight.Config;
using RtdxToken = System.Proxy.Rtdx.GetToken;
using SilverlakeConfig = System.Proxy.Silverlake.Base.Config;
using System.Proxy.Fis;
using System.Security.Cryptography.X509Certificates;
using System.Proxy.Fis.GetCard;
using System.Proxy.Salesforce.GetTerms;
using System.Proxy.Salesforce.Terms;

namespace System.Process.CrossCutting.DependencyInjection
{
    public static class ProxyServiceCollectionExtensions
    {
        public static IServiceCollection AddSalesforce(this IServiceCollection services, IConfiguration configuration)
        {
            var configSectionToken = configuration.GetSection("Salesforce:Token");
            var configToken = configSectionToken.Get<GetTokenParams>();
            if (configToken == null)
            {
                throw new InvalidOperationException($"The configuration section 'Salesforce: Token' was not found.");
            }

            services.Configure<GetTokenParams>(configSectionToken);

            var configSectionAuth = configuration.GetSection("Salesforce:Authentication");
            var configAuth = configSectionAuth.Get<SalesforceConfig>();
            if (configAuth == null)
            {
                throw new InvalidOperationException($"The configuration section 'Salesforce: Authentication' was not found.");
            }

            services.Configure<SalesforceConfig>(configSectionAuth);

            services.AddInjectables<SalesforceConfig>();

            services.AddScoped<IGetTokenClient, GetTokenClient>();
            services.AddScoped<IUpdateAssetClient, UpdateAssetClient>();
            services.AddScoped<IRegisterAssetClient, RegisterAssetClient>();
            services.AddScoped<IGetCustomerInformationsClient, GetCustomerInformationsClient>();
            services.AddScoped<IGetRecordTypeClient, GetRecordTypeClient>();
            services.AddScoped<IGetCreditCardsClient, GetCreditCardsClient>();
            services.AddScoped<IGetBusinessInformationClient, GetBusinessInformationClient>();
            services.AddScoped<IGetAccountInformationsClient, GetAccountInformationsClient>();
            services.AddScoped<ISearchAddressClient, SearchAddressClient>();
            services.AddScoped<IGetCommercialCompanyInfoClient, GetCommercialCompanyInfoClient>();
            services.AddScoped<IGetTermsClient, GetTermsClient>();
            services.AddScoped<ITermsClient, TermsClient>();
            services.AddScoped<SalesforceUpdateAccount.IUpdateAccountClient, SalesforceUpdateAccount.UpdateAccountClient>();

            return services;
        }

        public static IServiceCollection AddProxies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<HttpClient>();

            AddHeaderParams(services, configuration);

            services.AddServiceFactory<SilverlakeConfig.JackHenryConfig>(configuration.GetSection("JackHenry"))
              .AddScoped<IClientMessageInspector, MiddlewareIspector>()
              .AddScoped<IEndpointBehavior, LoggerBehavior>()
              .AddScoped<ICustomerOperation, CustomerOperation>()
              .AddScoped<IDepositOperation, DepositOperation>()
              .AddScoped<IInquiryOperation, InquiryOperation>()
              .AddScoped<ITransferWireOperation, TransferWireOperation>()
              .AddScoped<ITransactionOperation, TransactionOperation>();

            services.AddServiceFactory<FourSightConfig.JackHenryConfig>(configuration.GetSection("JackHenry"))
                    .AddScoped<IClientMessageInspector, MiddlewareIspector>()
                    .AddScoped<IEndpointBehavior, LoggerBehavior>()
                    .AddScoped<IStatementSearch, StatementSearch>()
                    .AddScoped<IStatementGenerate, StatementGenerate>()
                    .AddScoped<IStatementGenerateInquiry, StatementGenerateInquiry>();

            return services;
        }

        public static IServiceCollection AddHeaderParams(this IServiceCollection services, IConfiguration configuration)
        {
            var configSection = configuration.GetSection("JackHenry:HeaderParams");

            if (configSection.Get<SilverlakeConfig.HeaderParams>() == null)
            {
                throw new InvalidOperationException($"The SilverlakeConfig configuration section 'JackHenry: HeaderParams' was not found.");
            }

            if (configSection.Get<FourSightConfig.HeaderParams>() == null)
            {
                throw new InvalidOperationException($"The FourSightConfig configuration section 'JackHenry: HeaderParams' was not found.");
            }

            services.Configure<SilverlakeConfig.HeaderParams>(configSection);
            services.Configure<FourSightConfig.HeaderParams>(configSection);

            return services;
        }

        public static IServiceCollection AddRecordTypes(this IServiceCollection services, IConfiguration configuration)
        {
            var configSection = configuration.GetSection("Salesforce:RecordTypes");
            var config = configSection.Get<RecordTypesConfig>();

            if (config == null)
            {
                throw new InvalidOperationException($"The configuration section 'Salesforce: RecordTypes' was not found.");
            }

            services.Configure<RecordTypesConfig>(configSection);

            return services;
        }

        public static IServiceCollection AddFis(this IServiceCollection services, IConfiguration configuration)
        {
            var configSectionToken = configuration.GetSection("Fis:Token");
            var configToken = configSectionToken.Get<FisConfig>();
            var configTokenParams = configSectionToken.Get<FisToken.Messages.GetTokenParams>();
            if (configToken == null)
            {
                throw new InvalidOperationException($"The configuration section 'Fis: Token' was not found.");
            }

            services.Configure<FisConfig>(configSectionToken);
            services.Configure<FisToken.Messages.GetTokenParams>(configSectionToken);


            services.AddHttpClient<IFisClient, FisClient>()
                         .ConfigurePrimaryHttpMessageHandler(() =>
                         {
                             var handler = new HttpClientHandler();
                             if (!string.IsNullOrWhiteSpace(configToken.CertificatePath) && !string.IsNullOrWhiteSpace(configToken.CertificatePassword))
                             {
                                 var cert = new X509Certificate2(configToken.CertificatePath, configToken.CertificatePassword);
                                 handler.ClientCertificates.Add(cert);
                             }
                             return handler;
                         });

            services.AddScoped<FisToken.IGetTokenClient, FisToken.GetTokenClient>();
            services.AddScoped<IRegisterCardClient, RegisterCardClient>();
            services.AddScoped<IReissueCardClient, ReissueCardClient>();
            services.AddScoped<IActivateCardClient, ActivateCardClient>();
            services.AddScoped<IChangeCardPinClient, ChangeCardPinClient>();
            services.AddScoped<ISearchTransactionsClient, SearchTransactionsClient>();
            services.AddScoped<FisTransaction.ITransactionDetailClient, FisTransaction.TransactionDetailClient>();
            services.AddScoped<ICardReplaceClient, CardReplaceClient>();
            services.AddScoped<IChangeCardStatusClient, ChangeCardStatusClient>();
            services.AddScoped<ILockUnlockClient, LockUnlockClient>();
            services.AddScoped<IGetCardClient, GetCardClient>();

            return services;
        }

        public static IServiceCollection AddFeedzai(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FeedzaiConfig>(configuration.GetSection("FeedzaiConfig"));

            services.AddInjectables<FeedzaiConfig>();

            services.AddScoped<IFeedzaiClient, FeedzaiClient>();
            services.AddScoped<ITransferInitiationClient, TransferInitiationClient>();

            return services;
        }

        public static IServiceCollection AddRdaAdmin(this IServiceCollection services, IConfiguration configuration)
        {
            var configSection = configuration.GetSection("RdaAdminConfig");
            var config = configSection.Get<RdaAdminConfig>();

            if (config == null)
            {
                throw new InvalidOperationException($"The configuration section 'RdaAdminConfig' was not found.");
            }
            services.Configure<RdaAdminConfig>(configSection);
            services.AddInjectables<RdaAdminConfig>();
            services.AddScoped<IRdaAdminClient, RdaAdminClient>();
            services.AddScoped<IRdaClient, RdaClient>();
            services.AddScoped<IGetProcessCriteriaClient, GetProcessCriteriaClient>();
            services.AddScoped<IGetProcessCriteriaReferenceClient, GetProcessCriteriaReferenceClient>();
            services.AddScoped<IGetCustomersCriteriaClient, GetCustomersCriteriaClient>();
            services.AddScoped<IAddCustomerClient, AddCustomerClient>();
            services.AddScoped<IUpdateCustomerClient, UpdateCustomerClient>();
            services.AddScoped<IAddAccountClient, AddAccountClient>();
            services.AddScoped<IUpdateAccountClient, UpdateAccountClient>();

            return services;
        }

        public static IServiceCollection AddRda(this IServiceCollection services, IConfiguration configuration)
        {
            var configSection = configuration.GetSection("RdaConfig");
            var config = configSection.Get<Proxy.Rda.Common.Config.RdaConfig>();

            if (config == null)
            {
                throw new InvalidOperationException($"The configuration section 'RdaConfig' was not found.");
            }
            services.Configure<RdaConfig>(configSection);
            services.AddInjectables<RdaConfig>();
            services.AddScoped<IRdaClient, RdaClient>();
            services.AddScoped<IAuthenticateClient, AuthenticateClient>();
            services.AddScoped<ICreateBatchClient, CreateBatchClient>();
            services.AddScoped<IAddItemClient, AddItemClient>();
            services.AddScoped<IUpdateBatchClient, UpdateBatchClient>();

            return services;
        }

        public static IServiceCollection AddRdaConfig(this IServiceCollection services, IConfiguration configuration)
        {
            var configSection = configuration.GetSection("RdaCredentialsConfig");
            var config = configSection.Get<RdaCredentialsConfig>();

            if (config == null)
            {
                throw new InvalidOperationException($"The configuration section 'RdaCredentialsConfig' was not found.");
            }
            services.Configure<RdaCredentialsConfig>(configSection);
            services.AddInjectables<RdaCredentialsConfig>();

            return services;
        }
        public static IServiceCollection AddRtdx(this IServiceCollection services, IConfiguration configuration)
        {

            var configSectionToken = configuration.GetSection("Rtdx:Token");
            var configToken = configSectionToken.Get<RtdxToken.Messages.GetTokenParams>();
            if (configToken == null)
            {
                throw new InvalidOperationException($"The configuration section 'Rtdx: Token' was not found.");
            }

            services.Configure<RtdxToken.Messages.GetTokenParams>(configSectionToken);

            services.Configure<RtdxConfig>(configSectionToken);

            services.AddInjectables<RtdxConfig>();

            services.AddServiceFactory<RtdxConfig>(configuration.GetSection("Rtdx"))
                    .AddScoped<IGetTokenOperation, GetTokenOperation>()
                    .AddScoped<INewAccountAddOperation, NewAccountAddOperation>()
                    .AddScoped<ICommercialCompanyAddOperation, CommercialCompanyAddOperation>()
                    .AddScoped<IBalanceInquiryOperation, BalanceInquiryOperation>()
                    .AddScoped<ITransactionDetailsOperation, TransactionDetailsOperation>()
                    .AddScoped<IPaymentInquiryOperation, PaymentInquiryOperation>()
                    .AddScoped<IPendingActivityDetailsOperation, PendingActivityDetailsOperation>()
                    .AddScoped<IOrderNewPlasticOperation, OrderNewPlasticOperation>()
                    .AddScoped<ICardActivationOperation, CardActivationOperation>();


            return services;
        }

        public static IServiceCollection AddStatementsConfig(this IServiceCollection services, IConfiguration configuration)
        {
            var configSection = configuration.GetSection("StatementsConfig");
            var config = configSection.Get<GenerateStatementsConfig>();

            if (config == null)
            {
                throw new InvalidOperationException($"The configuration section 'GenerateStatementsConfig' was not found.");
            }

            services.Configure<GenerateStatementsConfig>(configSection);
            services.AddInjectables<GenerateStatementsConfig>();

            return services;
        }
    }
}