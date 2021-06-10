using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Domain.Constants;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
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

namespace System.Process.Application.Queries.ConsultProcessByCustomerId
{
    public class ConsultProcessByCustomerIdQuery : IRequestHandler<ConsultProcessByCustomerIdRequest, ConsultProcessByCustomerIdResponse>
    {
        #region Properties
        private IInquiryOperation InquiryOperation { get; }
        private ICustomerReadRepository CustomerReadRepository { get; }
        private ILogger<ConsultProcessByCustomerIdQuery> Logger { get; }
        private ProcessConfig ProcessConfig { get; }

        #endregion

        #region Constructor
        public ConsultProcessByCustomerIdQuery(
            IInquiryOperation inquiryOperation,
            ICustomerReadRepository customerReadRepository,
            ILogger<ConsultProcessByCustomerIdQuery> logger,
            IOptions<ProcessConfig> ProcessConfig)
        {
            InquiryOperation = inquiryOperation;
            CustomerReadRepository = customerReadRepository;
            Logger = logger;
            ProcessConfig = ProcessConfig.Value;
        }

        #endregion

        #region IRequestHandler implementation

        public async Task<ConsultProcessByCustomerIdResponse> Handle(ConsultProcessByCustomerIdRequest request, CancellationToken cancellationToken)
        {
            var adapter = new ConsultProcessByCustomerIdAdapter(ProcessConfig);
            var customer = GetCustomer(request.ApplicationId);

            var response = await ConsultAccount(adapter.Adapt(customer.BusinessCif), cancellationToken);

            if (response.ProcessearchRecInfo.Count == 0)
            {
                Logger.LogError($"Account details not found. BusinessCif: {customer.BusinessCif}");
                throw new NotFoundException($"Account details not found. BusinessCif: { customer.BusinessCif }");
            }

            return adapter.Adapt(response);
        }

        #endregion

        #region Methods

        private Customer GetCustomer(string applicationId)
        {
            var customer = CustomerReadRepository.FindBy(applicationId);

            if (customer == null)
            {
                Logger.LogError($"Customer not found. ApplicationId: {applicationId}");
                throw new NotFoundException($"Customer not found. ApplicationId: { applicationId }");
            }

            return customer;
        }

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
