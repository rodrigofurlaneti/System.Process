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

namespace System.Process.Application.Queries.ConsultProcessByAccountId
{
    public class ConsultProcessByAccountIdAdapter :
        IAdapter<ProcessearchRequest, ConsultProcessByAccountIdRequest>
    {
        #region Properties
        private ProcessConfig ProcessConfig { get; set; }

        #endregion

        public ConsultProcessByAccountIdAdapter(ProcessConfig ProcessConfig)
        {
            ProcessConfig = ProcessConfig;
        }

        #region IAdapter implementation
        public ProcessearchRequest Adapt(ConsultProcessByAccountIdRequest input)
        {
            return new ProcessearchRequest
            {
                MaximumRecords = 4000,
                AccountId = input.AccountId,
                AccountType = input.AccountType,
                IncXtendElemArray = new List<IncXtendElemInfoRequest>()
            };
        }

        public ConsultProcessByAccountIdResponse Adapt(ProcessearchResponse input)
        {
            var ProcessearchRecords = GetListProcessearchRecordsDto(input.ProcessearchRecInfo);

            return new ConsultProcessByAccountIdResponse
            {
                TotalProcessBalance = ProcessearchRecords?.First()?.AvailableBalance,
                ProcessearchRecords = ProcessearchRecords
            };
        }

        #endregion

        #region Methods

        private IList<ProcessearchRecordsDto> GetListProcessearchRecordsDto(List<ProcessearchRecInfo> Process)
        {
            var ProcessResult = new List<ProcessearchRecordsDto>();

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

        private ProcessearchRecordsDto FilterAccount(ProcessearchRecInfo account)
        {
            ProcessearchRecordsDto ProcessearchRecords;

            var Processtatus = Enum.Parse<Processtatus>(account?.Processtatus);

            switch (Processtatus)
            {
                case Processtatus.Active:
                case Processtatus.Dormant:
                case Processtatus.NewToday:
                case Processtatus.PendingClosed:
                    ProcessearchRecords = GetProcessearchRecordsDto(account);
                    break;
                default:
                    ProcessearchRecords = null;
                    break;
            }

            return ProcessearchRecords;
        }

        private ProcessearchRecordsDto GetProcessearchRecordsDto(ProcessearchRecInfo account)
        {
            return new ProcessearchRecordsDto
            {
                CustomerId = account?.CustomerId,
                Name = account?.ProductDesc,
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
