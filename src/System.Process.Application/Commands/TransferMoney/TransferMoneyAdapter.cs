using System.Process.Domain.Constants;
using System.Process.Domain.Entities;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Silverlake.Inquiry.Common;
using System.Proxy.Silverlake.Transaction.Common;
using System.Proxy.Silverlake.Transaction.Messages;
using System.Proxy.Silverlake.Transaction.Messages.Request;
using System.Collections.Generic;

namespace System.Process.Application.Commands.TransferMoney
{
    public class TransferMoneyAdapter : IAdapter<TransferAddRequest, TransferMoneyRequest>
    {
        #region Properties

        private ProcessConfig ProcessConfig { get; set; }
        public ProcessearchRecInfo AccountValidationResult { get; }
        public Receiver ReceiverValidationResult { get; }

        #endregion
        public TransferMoneyAdapter(ProcessConfig ProcessConfig)
        {
            ProcessConfig = ProcessConfig;
        }

        public TransferMoneyAdapter(ProcessConfig ProcessConfig, ProcessearchRecInfo accountValidationResult, Receiver receiverValidationResult)
        {
            ProcessConfig = ProcessConfig;
            AccountValidationResult = accountValidationResult;
            ReceiverValidationResult = receiverValidationResult;
        }

        public TransferMoneyAdapter()
        {

        }

        public TransferAddRequest Adapt(TransferMoneyRequest input)
        {
            var eftDescriptionArray = new List<EftDescriptionArray>() {
                new EftDescriptionArray
                {
                    EftDescription = string.IsNullOrWhiteSpace(input.Message) ? "-" : input.Message
                },
                new EftDescriptionArray
                {
                    EftDescription = "Internal Transfer"
                },
                new EftDescriptionArray
                {
                    EftDescription = $" from {AccountValidationResult.PersonNameInfo.ComName ?? ""}"
                },
                new EftDescriptionArray
                {
                    EftDescription = $" to {ReceiverValidationResult.CompanyName}"
                }
            };

            return new TransferAddRequest
            {
                AccountIdFrom = new AccountIdFrom
                {
                    FromAccountNumber = input.AccountFrom.FromAccountNumber,
                    FromAccountType = input.AccountFrom.FromAccountType
                },
                AccountIdTo = new AccountIdTo
                {
                    ToAccountNumber = input.AccountTo.ToAccountNumber,
                    ToAccountType = input.AccountTo.ToAccountType
                },
                TransferReceive = new TransferReceive
                {
                    Amount = input.Amount,
                    ReducedPrincipal = input.ReducedPrincipal,
                    OfficerCode = ProcessConfig.OfficerCode,
                    TransferSourceType = ProcessConfig.TransferSourceType,
                    EftDescriptionArray = eftDescriptionArray
                },
                TransferType = Constants.Xfer,
                ConsumerName = ProcessConfig.TransferConfig.XferConsumerName,
                ConsumerProduct = ProcessConfig.TransferConfig.XferConsumerProd
            };
        }

        public TransferAddValidateRequest AdaptValidate(TransferMoneyRequest input)
        {
            return new TransferAddValidateRequest
            {
                AccountIdFrom = new AccountIdFrom
                {
                    FromAccountNumber = input.AccountFrom.FromAccountNumber,
                    FromAccountType = input.AccountFrom.FromAccountType
                },
                AccountIdTo = new AccountIdTo
                {
                    ToAccountNumber = input.AccountTo.ToAccountNumber,
                    ToAccountType = input.AccountTo.ToAccountType
                },
                TransferReceive = new TransferReceive
                {
                    Amount = input.Amount,
                    ReducedPrincipal = input.ReducedPrincipal,
                    OfficerCode = ProcessConfig.OfficerCode,
                    TransferSourceType = ProcessConfig.TransferSourceType
                },
                TransferType = Constants.Xfer
            };
        }
    }
}