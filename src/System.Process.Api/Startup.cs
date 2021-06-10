using System.Reflection;
using FluentValidation.AspNetCore;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Process.Application.AuthorizationHandlers;
using System.Process.Application.Commands.AchTransferMoney;
using System.Process.Application.Commands.ActivateCard;
using System.Process.Application.Commands.AddReceiver;
using System.Process.Application.Commands.CardReplace;
using System.Process.Application.Commands.ChangeCardPin;
using System.Process.Application.Commands.CreditCard;
using System.Process.Application.Commands.CreditCardActivation.Validators;
using System.Process.Application.Commands.CreditCardAgreement.Validators;
using System.Process.Application.Commands.CreditCardBalance;
using System.Process.Application.Commands.CreditCardCancellation;
using System.Process.Application.Commands.DeleteReceiver;
using System.Process.Application.Commands.RemoteDepositCapture;
using System.Process.Application.Commands.TransactionDetail;
using System.Process.Application.Commands.TransferMoney;
using System.Process.Application.Commands.ValidateAccount;
using System.Process.Application.Queries.ConsultProcessByAccountId;
using System.Process.Application.Queries.ConsultProcessByCustomerId;
using System.Process.Application.Queries.FindReceivers;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.CrossCutting.Web.Middlewares.CorrelationId;
using System.Process.CrossCutting.Web.Middlewares.ElasticApm;
using System.Process.CrossCutting.Web.Middlewares.Logging;
using System.Process.CrossCutting.Web.Middlewares.MessageDecoder;
using System.Process.Domain.Enums;
using System.Phoenix.Web.Filters;
using System.Phoenix.Web.Security;
using System.Pricing.Api.Swagger;

namespace System.Process.Api
{
    public class Startup
    {
        #region Attributes

        private static readonly string Local = "Local";

        #endregion

        #region Properties

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public string BasePath { get; set; }

        #endregion

        #region Constructor

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
            BasePath = configuration.GetSection($"Swagger:BasePath").Value;
        }

        #endregion

        #region Methods

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddMvc(options => options.Filters.Add(new ValidationFilterAttribute()))
                .AddFluentValidation(options =>
                {
                    options.RegisterValidatorsFromAssemblyContaining<ValidateCustomerIdAttribute>();
                    options.RegisterValidatorsFromAssemblyContaining<ValidateAccountIdAttribute>();
                    options.RegisterValidatorsFromAssemblyContaining<TransferMoneyValidator>();
                    options.RegisterValidatorsFromAssemblyContaining<AchTransferMoneyValidator>();
                    options.RegisterValidatorsFromAssemblyContaining<FindReceiversValidatorAttribute>();
                    options.RegisterValidatorsFromAssemblyContaining<AddReceiverValidator>();
                    options.RegisterValidatorsFromAssemblyContaining<ActivateCardAttributesValidator>();
                    options.RegisterValidatorsFromAssemblyContaining<ChangeCardPinAttributesValidator>();
                    options.RegisterValidatorsFromAssemblyContaining<DeleteReceiverValidatorAttribute>();
                    options.RegisterValidatorsFromAssemblyContaining<ValidateAccountValidatorAttribute>();
                    options.RegisterValidatorsFromAssemblyContaining<TransactionDetailValidator>();
                    options.RegisterValidatorsFromAssemblyContaining<CardReplaceValidator>();
                    options.RegisterValidatorsFromAssemblyContaining<ValidateAccountIdAttribute>();
                    options.RegisterValidatorsFromAssemblyContaining<CreditCardValidator>();
                    options.RegisterValidatorsFromAssemblyContaining<CreditCardActivationValidator>();
                    options.RegisterValidatorsFromAssemblyContaining<CreditCardAgreementValidator>();
                    options.RegisterValidatorsFromAssemblyContaining<TermValidator>();
                    options.RegisterValidatorsFromAssemblyContaining<CreditCardCancellationValidation>();
                    options.RegisterValidatorsFromAssemblyContaining<ValidateCardIdAttribute>();
                    options.RegisterValidatorsFromAssemblyContaining<RemoteDepositCaptureCommandValidator>();
                });
            services.AddSecurityAuthentication(Configuration);
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ValidateConsultProcess",
                        policy => policy.Requirements.Add(new UserDataRequirement(ValidationProcess.ValidateConsultProcess)));
                options.AddPolicy("ValidateSearchCreditCards",
                        policy => policy.Requirements.Add(new UserDataRequirement(ValidationProcess.ValidateSearchCreditCards)));
                options.AddPolicy("ValidateCreditCardRequest",
                        policy => policy.Requirements.Add(new UserDataRequirement(ValidationProcess.ValidateCreditCardRequest)));
                options.AddPolicy("ValidateCreditCardAgreement",
                        policy => policy.Requirements.Add(new UserDataRequirement(ValidationProcess.ValidateCreditCardAgreement)));
                options.AddPolicy("ValidateCreditCardActivation",
                        policy => policy.Requirements.Add(new UserDataRequirement(ValidationProcess.ValidateCreditCardActivation)));
                options.AddPolicy("ValidateCreditCardActivation",
                        policy => policy.Requirements.Add(new UserDataRequirement(ValidationProcess.ValidateCreditCardActivation)));
                options.AddPolicy("ValidateRemoteDeposit",
                        policy => policy.Requirements.Add(new UserDataRequirement(ValidationProcess.ValidateRemoteDeposit)));
                options.AddPolicy("ValidateStatementTypes",
                        policy => policy.Requirements.Add(new UserDataRequirement(ValidationProcess.ValidateStatementTypes)));
                options.AddPolicy("ValidateRetrieveStatement",
                        policy => policy.Requirements.Add(new UserDataRequirement(ValidationProcess.ValidateRetrieveStatement)));
                options.AddPolicy("ValidateTransfer",
                        policy => policy.Requirements.Add(new UserDataRequirement(ValidationProcess.ValidateTransfer)));
                options.AddPolicy("ValidateCardOwner",
                        policy => policy.Requirements.Add(new UserDataRequirement(ValidationProcess.ValidateCardOwner)));
            });
            services.AddCorrelationId();
            services.AddProcessConfig(Configuration);
            services.AddJarvisConfig(Configuration);
            services.AddRepositories(Configuration);
            services.AddSystemSwagger(Configuration, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
            services.AddProxies(Configuration);
            services.AddSalesforce(Configuration);
            services.AddFis(Configuration);
            services.AddFeedzai(Configuration);
            services.AddRecordTypes(Configuration);
            services.AddRtdx(Configuration);
            services.AddRda(Configuration);
            services.AddRdaAdmin(Configuration);
            services.AddRdaConfig(Configuration);
            services.AddStatementsConfig(Configuration);
            services.AddKafka(Configuration);
            services.AddMessageDecoder();
            services.AddPipeline<string>(Configuration);
            services.AddMediator();
            services.AddProblems(Environment);
            services.AddPolicies(Configuration);
            services.AddRedis(Configuration);
            services.AddHealthChecks();
            services.AddOracleClient(Configuration, "Receiver");
            services.AddOracleCardClient(Configuration, "Receiver");
            services.AddDataMaskingConfig(Configuration);
            services.AddOracleTransferClient(Configuration, "Receiver");
        }

        public void Configure(IApplicationBuilder app, IApiVersionDescriptionProvider provider)
        {
            app.UseApiVersioning();
            app.UseCorrelationId();
            app.UseStructuredLogging();

            app.UseElasticApmApplication(Configuration, Environment);

            if (Environment.IsEnvironment(Local))
            {
                app.UseSystemSwagger(provider, null);
            }
            if (Environment.IsDevelopment())
            {
                app.UseSystemSwagger(provider, BasePath);
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();

            app.UseSecurityMiddleware();
            app.UseProblemDetails();
            app.UseMessageDecoderMiddleware();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });

            app.UseCors();
        }

        #endregion
    }
}
