using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Application.Queries.ConsultProcessByAccountId;
using System.Process.Domain.ValueObjects;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Silverlake.Base.Exceptions;
using System.Proxy.Silverlake.Inquiry;

namespace System.Process.Application.Commands.ValidateAccount
{
    public class ValidateAccountCommand : IRequestHandler<ValidateAccountRequest, ValidateAccountResponse>
    {
        #region Properties

        private ILogger<ValidateAccountCommand> Logger { get; }
        private IInquiryOperation InquiryOperation { get; }

        private ProcessConfig ProcessConfig { get; }

        #endregion

        #region Constructor

        public ValidateAccountCommand(
            ILogger<ValidateAccountCommand> logger,
            IInquiryOperation inquiryOperation,
            IOptions<ProcessConfig> ProcessConfig
            )
        {
            Logger = logger;
            InquiryOperation = inquiryOperation;
            ProcessConfig = ProcessConfig.Value;
        }

        #endregion

        #region INotificationHandler implementation

        public async Task<ValidateAccountResponse> Handle(ValidateAccountRequest request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation("Starting process for validate account");
                var consultProcessByIdRequest = new ConsultProcessByAccountIdRequest(request.AccountId);
                var consultProcessByAccountIdAdapter = new ConsultProcessByAccountIdAdapter(ProcessConfig);
                var validateAccountAdapter = new ValidateAccountAdapter(ProcessConfig);
                var paramsconsultProcessByAccountId = consultProcessByAccountIdAdapter.Adapt(consultProcessByIdRequest);
                var ProcessearchResult = await InquiryOperation.ProcessearchAsync(paramsconsultProcessByAccountId, cancellationToken);
                var accountInfo = validateAccountAdapter.Adapt(ProcessearchResult);
                if(accountInfo == null)
                {
                    throw new Exception($"Account {request.AccountId} not Found");
                }
                return await Task.FromResult(accountInfo);
            }
            catch (SilverlakeException ex)
            {
                throw new UnprocessableEntityException(ex.Message, ex.Errors[0]);
            }
            catch (Exception ex)
            {
                throw new UnprocessableEntityException("Error while validate account", ex.Message);
            }
        }

        #endregion
    }
}
