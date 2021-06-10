using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Domain.Constants;
using System.Process.Domain.ValueObjects;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Silverlake.Base.Exceptions;
using System.Proxy.Silverlake.Inquiry;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Proxy.Silverlake.Inquiry.Messages.Response;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Application.Queries.ConsultProcessByAccountId
{
    public class ConsultProcessByAccountIdQuery : IRequestHandler<ConsultProcessByAccountIdRequest, ConsultProcessByAccountIdResponse>
    {

        #region Properties
        private IInquiryOperation InquiryOperation { get; }
        private ILogger<ConsultProcessByAccountIdQuery> Logger { get; }

        private ProcessConfig ProcessConfig { get; }

        #endregion

        #region Constructor
        public ConsultProcessByAccountIdQuery(IInquiryOperation inquiryOperation,
            ILogger<ConsultProcessByAccountIdQuery> logger,
            IOptions<ProcessConfig> ProcessConfig)
        {
            InquiryOperation = inquiryOperation;
            Logger = logger;
            ProcessConfig = ProcessConfig.Value;
        }

        #endregion

        #region IRequestHandler implementation

        public async Task<ConsultProcessByAccountIdResponse> Handle(ConsultProcessByAccountIdRequest request, CancellationToken cancellationToken)
        {
            var adapter = new ConsultProcessByAccountIdAdapter(ProcessConfig);

            var response = await ConsultAccount(adapter.Adapt(request), cancellationToken);

            if (response.ProcessearchRecInfo.Count == 0)
            {
                Logger.LogError($"Account details not found. AccountId: {request.AccountId}");
                throw new NotFoundException($"Account details not found. AccountId: { request.AccountId }");
            }

            return adapter.Adapt(response);
        }

        #endregion

        #region Methods
        private async Task<ProcessearchResponse> ConsultAccount(ProcessearchRequest accountRequest, CancellationToken cancellationToken)
        {
            try
            {
                return await InquiryOperation.ProcessearchAsync(accountRequest, cancellationToken);
            }
            catch (SilverlakeException ex)
            {
                Logger.LogError("Error on ConsultAccount method");
                throw new UnprocessableEntityException(ex.Message, ex, new ErrorStructure(ex.ErrorDetails?.FirstOrDefault()?.ErrorCode, Providers.JackHenry));
            }
            catch (Exception ex)
            {
                Logger.LogInformation(ex, $"Error on ConsultAccount method");
                throw;
            }
        }

        #endregion
    }
}
