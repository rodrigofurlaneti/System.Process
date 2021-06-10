using System;
using System.Collections.Generic;
using System.Process.Domain.Entities;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Silverlake.Transaction.Common;
using System.Proxy.Silverlake.Transaction.Messages;

namespace System.Process.Application.Commands.ResumeTransfer.Adapters
{
    public class AchTransferAdapter : IAdapter<TransferAddRequest, Transfer>
    {

        #region Properties
        private ProcessConfig ProcessConfig { get; set; }

        #endregion

        #region Constructor
        public AchTransferAdapter(ProcessConfig ProcessContig)
        {
            ProcessConfig = ProcessContig;
        }

        #endregion

        public TransferAddRequest Adapt(Transfer input)
        {
            return new TransferAddRequest
            {
                AccountIdFrom = new AccountIdFrom
                {
                    FromAccountNumber = "7" + (new Random().Next(100000, 999999).ToString())
                },
                ConsumerName = ProcessConfig.TransferConfig.ConsumerNameACH,
                ConsumerProduct = ProcessConfig.TransferConfig.ConsumerProductACH,
                TransferReceive = new TransferReceive
                {
                    EftDescriptionArray = new List<EftDescriptionArray>()
                    {
                        new EftDescriptionArray
                        {
                            EftDescription = input.Message ?? ""
                        },
                         new EftDescriptionArray
                        {
                           EftDescription = "ACH"
                        },
                          new EftDescriptionArray
                        {
                            EftDescription = $" from {input.SenderName ?? ""}"
                        },
                           new EftDescriptionArray
                        {
                            EftDescription = $" to {input.ReceiverFirstName ?? ""}"
                        }
                    }
                },
                AchTransferReceive = new AchTransferReceive
                {
                    AchDebitName = input.SenderName,
                    AchDebitAccountId = input.AccountFromNumber,
                    AchDebitRoutingNumber = int.Parse(input.AccountFromRoutingNumber),
                    AchDebitAccountType = input.AccountFromType,

                    AchCreditName = input.ReceiverFirstName,
                    AchCreditAccountId = input.AccountToNumber,
                    AchCreditRoutingNumber = int.Parse(input.AccountToRoutingNumber),
                    AchCreditAccountType = input.AccountToType,

                    AchTransferAmount = input.Amount,
                    AchTransferExpireDate = DateTime.UtcNow.AddDays(6),

                    AchDebitTransactionCodeCode = ProcessConfig.AchDebitTransactionCodeCode,
                    AchTermCount = ProcessConfig.AchTermCount,
                    AchTermUnits = ProcessConfig.AchTermUnits,

                    AchCreditTransactionCodeCode = ProcessConfig.AchCreditTransactionCodeCode,
                    AchFeeAmount = ProcessConfig.AchFeeAmount,
                    AchSendPreNoteCode = ProcessConfig.AchSendPreNoteCode,
                    AchOneTime = ProcessConfig.AchOneTime,
                    AchUseLoanDateCode = ProcessConfig.AchUseLoanDateCode,
                    AchUseLoanAmountCode = ProcessConfig.AchUseLoanAmountCode,
                    AchNsfCode = ProcessConfig.AchNsfCode,
                    AchNextTransferDate = input.NextDay == "standard" ? DateTime.UtcNow.AddDays(2) : DateTime.UtcNow.AddDays(0),

                    AchCompanyName = ProcessConfig.CompanyName,
                    AchCompanyId = ProcessConfig.CompanyId,
                    AchCompanyDiscretionaryData = input.AccountFromNumber
                },
                TransferType = input.TransferType
            };
        }
    }
}
