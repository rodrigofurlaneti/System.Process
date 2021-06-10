using System.Collections.Generic;
using System.Process.Domain.Constants;
using System.Process.Domain.Entities;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Silverlake.TranferWire.Messages.Request;

namespace System.Process.Application.Commands.ResumeTransfer.Adapters
{
    public class WireTransferAdapter : IAdapter<TransferWireAddRequest, Transfer>
    {
        private ProcessConfig ProcessConfig { get; set; }
        public WireTransferAdapter(ProcessConfig ProcessConfig)
        {
            ProcessConfig = ProcessConfig;
        }

        public TransferWireAddRequest Adapt(Transfer input)
        {
            return new TransferWireAddRequest
            {
                AccountId = input.AccountFromNumber,
                AccountType = input.AccountFromType,
                TemplateCreated = ProcessConfig.TemplateCreated,
                TransferInformation = new TransferInfoRecord
                {
                    Amount = input.Amount,
                    TransactionalType = Constants.Outgoing,
                    CommonName = Constants.System,
                    InstitutionRoutingId = input.AccountToRoutingNumber,
                    ReceivingFinancialInstitutionName = input.BankName,
                    WireAnlysCode = ProcessConfig.WireAnlysCode,
                    BeneficiaryInformation = new BeneficiaryInformationRecord
                    {
                        BeneficiaryName = $"{input.ReceiverFirstName} {input.ReceiverLastName}",
                        BeneficiaryAccountId = input.AccountToNumber,
                        BeneficiaryAccountType = input.AccountToType,
                        FinancialInstitutionName = input.BankName,
                        BeneficiaryAddress = new Proxy.Silverlake.TranferWire.Common.Address
                        {
                            City = input.ReceiverAddressCity,
                            Country = input.ReceiverAddressCountry,
                            StateProvince = input.ReceiverAddressState,
                            StreetAddressOne = input.ReceiverAddressLine1,
                            StreetAddressTwo = input.ReceiverAddressLine2,
                            StreetAddressThree = input.ReceiverAddressLine3,
                            ZipCode = input.ReceiverAddressZipCode
                        },
                        FinancialInstitutionAddress = new Proxy.Silverlake.TranferWire.Common.Address
                        {
                            City = input.BankAddressCity,
                            Country = input.BankAddressCountry,
                            StateProvince = input.BankAddressState,
                            StreetAddressOne = input.BankAddressLine1,
                            StreetAddressTwo = input.BankAddressLine2,
                            StreetAddressThree = input.BankAddressLine3,
                            ZipCode = input.BankAddressZipCode
                        },
                        Remarks = GetRemarksInfo(input)
                    },
                    FeeAmount = decimal.Parse(ProcessConfig.FeeAmount)
                }
            };
        }

        private List<RemarksInfo> GetRemarksInfo(Transfer input)
        {
            return new List<RemarksInfo>
            {
                new RemarksInfo
                {
                    Remark = input.Message
                },
                new RemarksInfo
                {
                    Remark = "Wire Transfer"
                },
                new RemarksInfo
                {
                    Remark = $" from {input.SenderName ?? ""}"
                },
                new RemarksInfo
                {
                    Remark = $" to {input.ReceiverFirstName ?? ""} {input.ReceiverLastName ?? ""}"
                }
            };
        }
    }
}
