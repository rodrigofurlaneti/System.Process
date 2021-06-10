using System.Collections.Generic;
using System.Process.Domain.Constants;
using System.Process.Domain.Entities;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Silverlake.Transaction.Common;
using System.Proxy.Silverlake.Transaction.Messages;

namespace System.Process.Application.Commands.ResumeTransfer.Adapters
{
    public class InternalTransferAdapter : IAdapter<TransferAddRequest, Transfer>
    {
        #region Properties

        private ProcessConfig ProcessConfig { get; set; }

        #endregion

        #region Constructor
        public InternalTransferAdapter(ProcessConfig ProcessConfig)
        {
            ProcessConfig = ProcessConfig;
        }

        #endregion

        public TransferAddRequest Adapt(Transfer input)
        {
            var eftDescriptionArray = new List<EftDescriptionArray>() {
                new EftDescriptionArray
                {
                    EftDescription = input.Message ?? ""
                },
                new EftDescriptionArray
                {
                    EftDescription = "Internal Transfer"
                },
                new EftDescriptionArray
                {
                    EftDescription = $" from {input.SenderName ?? ""}"
                },
                new EftDescriptionArray
                {
                    EftDescription = $" to {input.ReceiverFirstName ?? ""} {input.ReceiverLastName ?? ""}"
                }
            };

            return new TransferAddRequest
            {
                AccountIdTo = new AccountIdTo
                {
                    ToAccountNumber = input.AccountToNumber,
                    ToAccountType = input.AccountToType
                },
                AccountIdFrom = new AccountIdFrom
                {
                    FromAccountNumber = input.AccountFromNumber,
                    FromAccountType = input.AccountFromType
                },
                TransferReceive = new TransferReceive
                {
                    Amount = input.Amount,
                    ReducedPrincipal = input.ReducedPrincipal,
                    OfficerCode = ProcessConfig.OfficerCode,
                    TransferSourceType = ProcessConfig.TransferSourceType,
                    EftDescriptionArray = eftDescriptionArray
                },
                TransferType = Constants.Xfer
            };
        }
    }
}
