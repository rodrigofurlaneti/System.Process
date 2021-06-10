using System;
using System.Collections.Generic;
using System.Linq;
using System.Process.Application.Commands.Utils;
using System.Process.Application.DataTransferObjects;
using System.Process.Domain.Constants;
using System.Process.Domain.Enums;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Silverlake.Inquiry.Common;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Proxy.Silverlake.Inquiry.Messages.Response;

namespace System.Process.Application.Commands.ValidateAccount
{
    public class ValidateAccountAdapter :
        IAdapter<AccountInquiryRequest, ValidateAccountRequest>
    {

        private ProcessConfig ProcessConfig { get; set; }

        public ValidateAccountAdapter(ProcessConfig ProcessConfig)
        {
            ProcessConfig = ProcessConfig;
        }
        public AccountInquiryRequest Adapt(ValidateAccountRequest input)
        {
            return new AccountInquiryRequest
            {
                AccountNumber = input.AccountId,
                AccountType = input.AccountType,
                IncXtendElemArray = new List<IncXtendElemInfoRequest>()
                {
                    new IncXtendElemInfoRequest
                    {
                        XtendElem = GetElementInfo(input)
                    }
                }
            };
        }

        public ValidateAccountResponse Adapt(ProcessearchResponse input)
        {
            var ProcessearchRecords = GetProcessearchRecordsDto(input.ProcessearchRecInfo);
            return new ValidateAccountResponse
            {
                Valid = Enum.Parse<Processtatus>(input.ProcessearchRecInfo.First()?.Processtatus) == Processtatus.Active,
                TotalProcessBalance = ProcessearchRecords != null ? ProcessearchRecords.Sum(x => x.AvailableBalance) : null,
                ProcessearchRecords = ProcessearchRecords
            };
        }

        #region Private Methods


        private string GetElementInfo(ValidateAccountRequest input)
        {
            var accountType = (AccountType)Enum.Parse(typeof(AccountType), input.AccountType);
            var elementInfo = EnumAttribute.GetElementInfo(accountType);

            return elementInfo;
        }

        private IList<ProcessearchRecordsDto> GetProcessearchRecordsDto(List<ProcessearchRecInfo> Process)
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

            return Equals(0, ProcessResult.Count()) ? null : ProcessResult;
        }

        private ProcessearchRecordsDto FilterAccount(ProcessearchRecInfo account)
        {
            ProcessearchRecordsDto ProcessearchRecords;

            var Processtatus = Enum.Parse<Processtatus>(account?.Processtatus);

            switch (Processtatus)
            {
                case Processtatus.Closed:
                case Processtatus.ChargedOff:
                    ProcessearchRecords = GetProcessearchRecordsDto(account);
                    ProcessearchRecords.AvailableBalance = null;
                    ProcessearchRecords.CurrentBalance = null;
                    ProcessearchRecords.AvailableBalanceCurrency = null;
                    ProcessearchRecords.CurrentBalanceCurrency = null;
                    break;
                case Processtatus.Active:
                case Processtatus.Dormant:
                case Processtatus.NewToday:
                case Processtatus.PendingClosed:
                case Processtatus.Restricted:
                case Processtatus.NoPost:
                    ProcessearchRecords = GetProcessearchRecordsDto(account);
                    break;
                case Processtatus.Escheat:
                case Processtatus.NoCredits:
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