using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Silverlake.Transaction.Common;
using System.Proxy.Silverlake.Transaction.Messages;
using System.Proxy.Silverlake.Transaction.Messages.Request;
using System;
using System.Collections.Generic;

namespace System.Process.Application.Commands.AchTransferMoney
{
    public class AchTransferMoneyAdapter : IAdapter<TransferAddRequest, AchTransferMoneyRequest>
    {
        #region Properties
        private ProcessConfig ProcessConfig { get; set; }

        #endregion

        public AchTransferMoneyAdapter(ProcessConfig ProcessContig)
        {
            ProcessConfig = ProcessContig;
        }

        public TransferAddRequest Adapt(AchTransferMoneyRequest input)
        {
            return new TransferAddRequest
            {

                AccountIdFrom = new AccountIdFrom
                {
                    FromAccountNumber = "7" + (new Random().Next(100000, 999999).ToString())
                },
                ConsumerName = ProcessConfig.TransferConfig.ConsumerNameACH,
                ConsumerProduct = ProcessConfig.TransferConfig.ConsumerProductACH,
                TransferType = ProcessConfig.TransferTypeACH,
                TransferReceive = new TransferReceive
                {
                    EftDescriptionArray = new List<EftDescriptionArray>()
                    {
                        new EftDescriptionArray
                        {
                            EftDescription = string.IsNullOrWhiteSpace(input.Memo) ? "-" : input.Memo
                        },
                         new EftDescriptionArray
                        {
                           EftDescription = "ACH"
                        },
                          new EftDescriptionArray
                        {
                            EftDescription = $" from {input.AccountFrom.Name ?? ""}"
                        },
                           new EftDescriptionArray
                        {
                            EftDescription = $" to {input.AccountTo.Name ?? ""}"
                        }
                    }
                },
                AchTransferReceive = new AchTransferReceive
                {
                    AchDebitName = input.AccountFrom.Name,
                    AchDebitAccountId = input.AccountFrom.AccountId,
                    AchDebitRoutingNumber = int.Parse(input.AccountFrom.RoutingNumber),
                    AchDebitAccountType = input.AccountFrom.AccountType,

                    AchCreditName = input.AccountTo.Name,
                    AchCreditAccountId = input.AccountTo.AccountId,
                    AchCreditRoutingNumber = int.Parse(input.AccountTo.RoutingNumber),
                    AchCreditAccountType = input.AccountTo.AccountType,

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
                    AchCompanyDiscretionaryData = input.AccountFrom.AccountId
                }
            };
        }

        public TransferAddValidateRequest AdaptValidate(AchTransferMoneyRequest input)
        {
            return new TransferAddValidateRequest
            {
                AccountIdFrom = new AccountIdFrom
                {
                    FromAccountNumber = new Random().Next(100000000, 999999999).ToString()
                },
                TransferType = ProcessConfig.TransferTypeACH,
                ConsumerName = ProcessConfig.TransferConfig.ConsumerNameACH,
                AchTransferReceive = new AchTransferReceive
                {
                    AchDebitName = input.AccountFrom.Name,
                    AchDebitAccountId = input.AccountFrom.AccountId,
                    AchDebitRoutingNumber = int.Parse(input.AccountFrom.RoutingNumber),
                    AchDebitAccountType = input.AccountFrom.AccountType,

                    AchCreditName = input.AccountTo.Name,
                    AchCreditAccountId = input.AccountTo.AccountId,
                    AchCreditRoutingNumber = int.Parse(input.AccountTo.RoutingNumber),
                    AchCreditAccountType = input.AccountTo.AccountType,

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
                    AchCompanyDiscretionaryData = input.AccountFrom.AccountId
                }
            };
        }

    }
}
