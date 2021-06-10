using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Commands.ResumeTransfer;
using System.Process.Base.UnitTests;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.UnitTests.Common;
using System.Proxy.Rda.AddItem;
using System.Proxy.Rda.Authenticate;
using System.Proxy.Rda.CreateBatch;
using System.Proxy.Rda.UpdateBatch;
using System.Proxy.Silverlake.TranferWire;
using System.Proxy.Silverlake.Transaction;
using System.Proxy.Silverlake.Transaction.Messages;
using System.Proxy.Silverlake.Transaction.Messages.Request;
using System.Proxy.Silverlake.Transaction.Messages.Response;
using Xunit;

namespace System.Process.UnitTests.Application.Commands.ResumeTransfer
{
    public class ResumeTransferCommandTests
    {
        #region Properties

        private CancellationToken CancellationToken { get; set; }
        private Mock<ILogger<ResumeTransferCommand>> Logger { get; }
        private Mock<ITransferReadRepository> TransferReadRepository { get; }
        private Mock<ITransferItemReadRepository> TransferItemReadRepository { get; }
        private Mock<ITransferItemWriteRepository> TransferItemWriteRepository { get; }
        private Mock<ITransferWriteRepository> TransferWriteRepository { get; }
        private Mock<ITransactionOperation> TransactionOperation { get; }
        private Mock<ITransferWireOperation> TransferWireOperation { get; }
        private Mock<IAuthenticateClient> AuthenticateClient { get; }
        private Mock<ICreateBatchClient> CreateBatchClient { get; }
        private Mock<IAddItemClient> AddItemClient { get; }
        private Mock<IUpdateBatchClient> UpdateBatchClient { get; }
        private Mock<IOptions<ProcessConfig>> ProcessConfig { get; }

        #endregion

        #region Constructor

        public ResumeTransferCommandTests()
        {
            CancellationToken = new CancellationToken();
            Logger = new Mock<ILogger<ResumeTransferCommand>>();

            TransferReadRepository = new Mock<ITransferReadRepository>();
            TransferReadRepository
                .Setup(x => x.FindByLifeCycleId(It.IsAny<string>())).Returns(GetTransfer());

            TransferItemReadRepository = new Mock<ITransferItemReadRepository>();
            TransferItemReadRepository
                .Setup(x => x.Find(It.IsAny<string>())).Returns(GetTransferItem());

            TransferWriteRepository = new Mock<ITransferWriteRepository>();
            TransferWriteRepository
                .Setup(x => x.Remove(It.IsAny<Transfer>(), It.IsAny<CancellationToken>())).Verifiable();

            TransferItemWriteRepository = new Mock<ITransferItemWriteRepository>();
            TransferItemWriteRepository
                .Setup(x => x.Remove(It.IsAny<TransferItem>(), It.IsAny<CancellationToken>())).Verifiable();

            TransferWireOperation = new Mock<ITransferWireOperation>();

            TransactionOperation = new Mock<ITransactionOperation>();
            TransactionOperation
            .Setup(x => x.StopCheckCancelAsync(It.IsAny<StopCheckCancelRequest>(), It.IsAny<CancellationToken>()))
            .Returns(GetStopCheckCancelResponse());

            TransactionOperation
                .Setup(x => x.TransferAddAsync(It.IsAny<TransferAddRequest>(), It.IsAny<CancellationToken>())).Returns(GetTransferAddResponse());

            AuthenticateClient = new Mock<IAuthenticateClient>();
            CreateBatchClient = new Mock<ICreateBatchClient>();
            AddItemClient = new Mock<IAddItemClient>();
            UpdateBatchClient = new Mock<IUpdateBatchClient>();
            ProcessConfig = new Mock<IOptions<ProcessConfig>>();
        }

        #endregion

        #region Tests

        [Fact(DisplayName = "Should Send Handle Resume Transfer")]
        public async Task ShouldSendHandleResumeTransfer()
        {
            ProcessConfig.Setup(p => p.Value).Returns(ProcessUnitTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));

            var command = new ResumeTransferCommand(
                Logger.Object,
                TransferReadRepository.Object,
                TransferWriteRepository.Object,
                TransferItemReadRepository.Object,
                TransferItemWriteRepository.Object,
                TransactionOperation.Object,
                TransferWireOperation.Object,
                AuthenticateClient.Object,
                CreateBatchClient.Object,
                AddItemClient.Object,
                UpdateBatchClient.Object,
                ProcessConfig.Object);

            var request = new ResumeTransferRequest
            {
                Decision = "approve",
                LifeCycleId = "1234"
            };

            var result = await command.Handle(request, CancellationToken);

            Assert.NotNull(result);
        }

        [Fact(DisplayName = "Should Send Handle Resume Transfer Error")]
        public async Task ShouldSendResumeTransferError()
        {
            ProcessConfig.Setup(p => p.Value).Returns(ProcessUnitTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));

            TransferReadRepository
                .Setup(x => x.FindByLifeCycleId(It.IsAny<string>())).Throws(new Exception());

            var command = new ResumeTransferCommand(
                Logger.Object,
                TransferReadRepository.Object,
                TransferWriteRepository.Object,
                TransferItemReadRepository.Object,
                TransferItemWriteRepository.Object,
                TransactionOperation.Object,
                TransferWireOperation.Object,
                AuthenticateClient.Object,
                CreateBatchClient.Object,
                AddItemClient.Object,
                UpdateBatchClient.Object,
                ProcessConfig.Object);

            var request = new ResumeTransferRequest
            {
                Decision = "approve",
                LifeCycleId = null
            };

            await Assert.ThrowsAsync<Exception>(() => command.Handle(request, CancellationToken));
        }


        #endregion

        #region Private Methods

        private Transfer GetTransfer()
        {
            return ConvertJson.ReadJson<Transfer>("Transfer.json");
        }

        private List<TransferItem> GetTransferItem()
        {
            return ConvertJson.ReadJson<List<TransferItem>>("TransferItem.json");
        }

        private Task<StopCheckCancelResponse> GetStopCheckCancelResponse()
        {
            return Task.FromResult(
                new StopCheckCancelResponse
                {
                    ResponseStatus = "Success"
                });
        }

        private Task<TransferAddResponse> GetTransferAddResponse()
        {
            return Task.FromResult(
                new TransferAddResponse
                {
                    ResponseStatus = "Success",
                    ResponseHeaderInfo = new ResponseMessageHeaderInfo
                    {
                        RecordInformationMessage = new List<RecInfoMessage>
                        {
                            new RecInfoMessage
                            {
                                ErrorDescription = "error"
                            }
                        }
                    }
                });
        }

        #endregion   
    }
}
