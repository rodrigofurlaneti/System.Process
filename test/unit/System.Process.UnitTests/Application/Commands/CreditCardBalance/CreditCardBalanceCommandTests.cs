using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Commands.CreditCardBalance;
using System.Process.Base.UnitTests;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.UnitTests.Common;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Rtdx.BalanceInquiry;
using System.Proxy.Rtdx.BalanceInquiry.Messages;
using System.Proxy.Rtdx.GetToken;
using System.Proxy.Rtdx.GetToken.Messages;
using System.Proxy.Rtdx.Messages;
using Xunit;

namespace System.Process.UnitTests.Application.Commands.CreditCardBalance
{
    public class CreditCardBalanceCommandTests
    {
        #region Properties

        private Mock<ILogger<CreditCardBalanceCommand>> Logger { get; }
        private Mock<ICardReadRepository> CardReadRepository { get; }
        private Mock<IBalanceInquiryOperation> BalanceInquiryOperation { get; }
        private Mock<IGetTokenOperation> GetTokenOperation { get; }
        private IOptions<GetTokenParams> RtdxTokenParams { get; }
        private IOptions<ProcessConfig> ProcessConfig { get; }

        #endregion

        #region Constructor

        public CreditCardBalanceCommandTests()
        {
            Logger = new Mock<ILogger<CreditCardBalanceCommand>>();
            CardReadRepository = new Mock<ICardReadRepository>();
            BalanceInquiryOperation = new Mock<IBalanceInquiryOperation>();
            GetTokenOperation = new Mock<IGetTokenOperation>();
            var param = new GetTokenParams();
            RtdxTokenParams = Options.Create(param);
            var ProcessConfig = ProcessUnitTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json");
            ProcessConfig = Options.Create(ProcessConfig);
        }

        #endregion

        #region Methods

        [Fact(DisplayName = "Should Handle credit card Balance Successfully")]
        public async void ShouldCreditCardBalanceSuccessfully()
        {
            CardReadRepository.Setup(x => x.FindByCardId(It.IsAny<int>())).Returns(GetCard());
            BalanceInquiryOperation.Setup(x => x.BalanceInquiryAsync(It.IsAny<BalanceInquiryParams>(), It.IsAny<CancellationToken>())).Returns(GetInquiryBalance());
            GetTokenOperation.Setup(x => x.GetTokenAsync(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenResult());
            var command = new CreditCardBalanceCommand(Logger.Object, CardReadRepository.Object,
                BalanceInquiryOperation.Object, GetTokenOperation.Object, RtdxTokenParams, ProcessConfig);
            var request = ConvertJson.ReadJson<CreditCardBalanceRequest>("CreditCardBalanceRequest.json");

            var result = await command.Handle(request, new CancellationToken());

            result.Should().NotBeNull();
        }

        [Fact(DisplayName = "Should CreditCardValidation Throws new Exception")]
        public async void ShouldCreditCardValidationThrowsException()
        {
            var creditCard = GetCard();
            creditCard.CardStatus = "error";
            CardReadRepository.Setup(x => x.FindByCardId(It.IsAny<int>())).Returns(creditCard);
            BalanceInquiryOperation.Setup(x => x.BalanceInquiryAsync(It.IsAny<BalanceInquiryParams>(), It.IsAny<CancellationToken>())).Returns(GetInquiryBalance());
            var command = new CreditCardBalanceCommand(Logger.Object, CardReadRepository.Object,
                BalanceInquiryOperation.Object, GetTokenOperation.Object, RtdxTokenParams, ProcessConfig);
            var request = ConvertJson.ReadJson<CreditCardBalanceRequest>("CreditCardBalanceRequest.json");

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, new CancellationToken()));
        }

        [Fact(DisplayName = "Should FindCreditCard new Throws new Exception")]
        public async void ShouldFindCreditCardNewThrowsException()
        {
            var command = new CreditCardBalanceCommand(Logger.Object, CardReadRepository.Object,
                BalanceInquiryOperation.Object, GetTokenOperation.Object, RtdxTokenParams, ProcessConfig);
            var request = ConvertJson.ReadJson<CreditCardBalanceRequest>("CreditCardBalanceRequest.json");

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, new CancellationToken()));
        }

        [Fact(DisplayName = "Should FindCreditCard Throws new Exception")]
        public async void ShouldFindCreditCardThrowsException()
        {
            CardReadRepository.Setup(x => x.FindByCardId(It.IsAny<int>())).Throws(new Exception());
            var command = new CreditCardBalanceCommand(Logger.Object, CardReadRepository.Object,
                BalanceInquiryOperation.Object, GetTokenOperation.Object, RtdxTokenParams, ProcessConfig);
            var request = ConvertJson.ReadJson<CreditCardBalanceRequest>("CreditCardBalanceRequest.json");

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, new CancellationToken()));
        }

        [Fact(DisplayName = "Should FindByAssetId Throws new Exception")]
        public async void ShouldFindByAssetIdThrowsException()
        {
            Card creditCard = null;
            CardReadRepository.Setup(x => x.FindByCardId(It.IsAny<int>())).Returns(creditCard);
            var command = new CreditCardBalanceCommand(Logger.Object, CardReadRepository.Object,
                BalanceInquiryOperation.Object, GetTokenOperation.Object, RtdxTokenParams, ProcessConfig);
            var request = ConvertJson.ReadJson<CreditCardBalanceRequest>("CreditCardBalanceRequest.json");

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, new CancellationToken()));
        }

        [Fact(DisplayName = "Should BalanceInquiry Throws new Exception")]
        public async void ShouldBalanceInquiryThrowsException()
        {
            CardReadRepository.Setup(x => x.FindByAssetId(It.IsAny<string>())).Returns(GetCard());
            BalanceInquiryOperation.Setup(x => x.BalanceInquiryAsync(It.IsAny<BalanceInquiryParams>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            var command = new CreditCardBalanceCommand(Logger.Object, CardReadRepository.Object,
                BalanceInquiryOperation.Object, GetTokenOperation.Object, RtdxTokenParams, ProcessConfig);
            var request = ConvertJson.ReadJson<CreditCardBalanceRequest>("CreditCardBalanceRequest.json");

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, new CancellationToken()));
        }

        #endregion

        #region Private Methods

        private Task<BaseResult<BalanceInquiryResult>> GetInquiryBalance()
        {
            return Task.FromResult(new BaseResult<BalanceInquiryResult>
            {
                IsSuccess = true,
                Result = ConvertJson.ReadJson<BalanceInquiryResult>("BalanceInquiryResult.json")
            });
        }

        private Task<BaseResult<GetTokenResult>> GetTokenResult()
        {
            return Task.FromResult(new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult { SecurityToken = "token" }
            });
        }

        public Card GetCard()
        {
            return ConvertJson.ReadJson<Card>("Cards.json");
        }

        #endregion
    }
}
