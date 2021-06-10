using System.Process.Application.DataTransferObjects;
using System.Process.Domain.Constants;
using System.Process.Domain.Enums;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Silverlake.Inquiry.Common;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Proxy.Silverlake.Inquiry.Messages.Response;
using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Process.Application.Queries.ConsultProcessByCustomerId
{
    public class ConsultProcessByCustomerIdAdapter :
         IAdapter<ProcessearchRequest, string>,
         IAdapter<ConsultProcessByCustomerIdResponse, ProcessearchResponse>
    {

        #region Properties
        private ProcessConfig ProcessConfig { get; set; }

        #endregion

        public ConsultProcessByCustomerIdAdapter(ProcessConfig ProcessConfig)
        {
            ProcessConfig = ProcessConfig;
        }

        #region IAdapter implementation
        public ProcessearchRequest Adapt(string customerId)
        {
            return new ProcessearchRequest
            {
                MaximumRecords = 20,
                CustomerId = customerId,
                IncXtendElemArray = new List<IncXtendElemInfoRequest>()
            };
        }

        public ConsultProcessByCustomerIdResponse Adapt(ProcessearchResponse input)
        {
            var ProcessearchRecords = GetProcessearchRecordsDto(input.ProcessearchRecInfo);

            return new ConsultProcessByCustomerIdResponse
            {
                TotalProcessBalance = ProcessearchRecords?.Sum(x => x.AvailableBalance),
                ProcessearchRecords = ProcessearchRecords
            };
        }

        #endregion

        #region Methods

        private IList<CustomerSearchRecordsDto> GetProcessearchRecordsDto(List<ProcessearchRecInfo> Process)
        {
            var ProcessResult = new List<CustomerSearchRecordsDto>();

            foreach (var account in Process)
            {
                var accountFiltered = FilterAccount(account);
                if (accountFiltered != null)
                {
                    ProcessResult.Add(accountFiltered);
                }
            }

            return Equals(0, ProcessResult.Count) ? null : ProcessResult;
        }

        private CustomerSearchRecordsDto FilterAccount(ProcessearchRecInfo account)
        {
            CustomerSearchRecordsDto ProcessearchRecords;

            var Processtatus = Enum.Parse<Processtatus>(account?.Processtatus);

            switch (Processtatus)
            {
                case Processtatus.Active:
                case Processtatus.Dormant:
                case Processtatus.NewToday:
                case Processtatus.PendingClosed:
                    ProcessearchRecords = GetCustomerSearchRecordsDto(account);
                    break;
                default:
                    ProcessearchRecords = null;
                    break;
            }

            return ProcessearchRecords;
        }

        private CustomerSearchRecordsDto GetCustomerSearchRecordsDto(ProcessearchRecInfo account)
        {
            return new CustomerSearchRecordsDto
            {
                Name = account?.ProductDesc,
                AccountName = account?.PersonNameInfo?.ComName,
                Bank = Constants.Safra,
                AccountType = account?.AccountId?.AccountType,
                RoutingNumber = ProcessConfig.RoutingNumber,
                AccountNumber = account?.AccountId?.AccountNumber,
                Processtatus = string.Concat(account?.Processtatus, " (", account?.ProcesstatusDesc, ")"),
                CurrentBalance = account?.Amount,
                CurrentBalanceCurrency = ProcessConfig.CurrentBalanceCurrency,
                AvailableBalance = account?.AvailableBalance,
                AvailableBalanceCurrency = ProcessConfig.AvailableBalanceCurrency
            };
        }
        #endregion
    }
}
