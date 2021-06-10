using System.Process.Application.Utils;
using System.Process.Domain.Constants;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Silverlake.Inquiry.Common;
using System.Proxy.Silverlake.TranferWire.Messages.Request;
using System.Proxy.Silverlake.TranferWire.Messages.Response;
using System.Collections.Generic;

namespace System.Process.Application.Commands.WireTransfer
{
    public class WireTransferAdapter : IAdapter<TransferWireAddRequest, WireTransferAddRequest>
    {
        private ProcessConfig ProcessConfig { get; set; }
        public ProcessearchRecInfo AccountValidationResult { get; set; }

        public WireTransferAdapter(ProcessConfig ProcessConfig, ProcessearchRecInfo accountValidationResult)
        {
            ProcessConfig = ProcessConfig;
            AccountValidationResult = accountValidationResult;
        }

        public TransferWireAddRequest Adapt(WireTransferAddRequest input)
        {
            return new TransferWireAddRequest
            {
                AccountId = input.FromAccountId,
                AccountType = input.FromAccountType,
                TemplateCreated = ProcessConfig.TemplateCreated,
                TransferInformation = new TransferInfoRecord
                {
                    Amount = input.Amount,
                    TransactionalType = Constants.Outgoing,
                    CommonName = ProcessConfig.SafraDigitalBank,
                    FeeAmount = decimal.Parse(ProcessConfig.FeeAmount),
                    InstitutionRoutingId = input.ToRoutingNumber,
                    ReceivingFinancialInstitutionName = input.BankName,
                    WireAnlysCode = ProcessConfig.WireAnlysCode,
                    BeneficiaryInformation = new BeneficiaryInformationRecord
                    {
                        BeneficiaryName = $"{input.ReceiverFirstName} {input.ReceiverLastName}",
                        BeneficiaryAccountId = input.ToAccountId,
                        BeneficiaryAccountType = input.ToAccountType,
                        BeneficiaryAddress = new Proxy.Silverlake.TranferWire.Common.Address
                        {
                            City = input.ReceiverAddress.City,
                            Country = CountryInfo.GetCountryInfo(input.ReceiverAddress.Country).ShortThree,
                            StateProvince = input.ReceiverAddress.State,
                            StreetAddressOne = input.ReceiverAddress.Line1,
                            StreetAddressTwo = input.ReceiverAddress.Line2,
                            StreetAddressThree = input.ReceiverAddress.Line3,
                            ZipCode = input.ReceiverAddress.ZipCode
                        },
                        FinancialInstitutionAddress = new Proxy.Silverlake.TranferWire.Common.Address
                        {
                            City = input.BankAddress.City,
                            Country = CountryInfo.GetCountryInfo(input.BankAddress.Country).ShortThree,
                            StateProvince = input.BankAddress.State,
                            StreetAddressOne = input.BankAddress.Line1,
                            StreetAddressTwo = input.BankAddress.Line2,
                            StreetAddressThree = input.BankAddress.Line3,
                            ZipCode = input.BankAddress.ZipCode
                        },
                        FinancialInstitutionName = input.BankName,
                        Remarks = GetRemarksInfo(input)
                    }
                }
            };
        }

        public WireTransferAddResponse AdaptResponse(TransferWireAddResponse input)
        {
            return new WireTransferAddResponse
            {
                TransactionId = input.TransactionalReceiptId
            };
        }

        private List<RemarksInfo> GetRemarksInfo(WireTransferAddRequest input)
        {
            return new List<RemarksInfo>
            {
                new RemarksInfo
                {
                    Remark = string.IsNullOrWhiteSpace(input.Message) ? "-" : input.Message
                },
                new RemarksInfo
                {
                    Remark = "Wire Transfer"
                },
                new RemarksInfo
                {
                    Remark = $" from {AccountValidationResult.PersonNameInfo.ComName ?? ""}"
                },
                new RemarksInfo
                {
                    Remark = $" to {input.ReceiverFirstName ?? ""}"
                }
            };
        }
    }
}