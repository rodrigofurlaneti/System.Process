using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Clients.Cards;
using System.Process.Application.DataTransferObjects;
using System.Process.Application.Queries.ConsultCardsByCustomerId;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.IntegrationTests.Application.Queries.ConsultCardByCustomerId
{
    public class ConsultCardsByCustomerIdQueryTests
    {
        #region Properties

        private static ServiceCollection Services;
        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;

        private Mock<ILogger<ConsultCardsByCustomerIdQuery>> Logger { get; }
        private Mock<ICardService> CardService { get; }
        #endregion

        #region Constructor

        public ConsultCardsByCustomerIdQueryTests()
        {
            Logger = new Mock<ILogger<ConsultCardsByCustomerIdQuery>>();
            CardService = new Mock<ICardService>();

            Services = new ServiceCollection();
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Tests

        [Trait("Integration", "Success")]
        [Fact(DisplayName = "ConsultCardsByCustomerIdQuery_Success")]
        public async void ShouldConsultCardByCustomerIdSuccessfully()
        {
            ConsultCardsByCustomerIdResponse response = new ConsultCardsByCustomerIdResponse
            {
                CardRecords = new List<CustomerCardDto>
                {
                    new CustomerCardDto
                    {
                        AccountBalance = "0",
                        AssetId = "00aa0",
                        Bin = "00000",
                        BusinessName = "CORPORATE INC.",
                        CardHolder = "CORPORATE CARD HOLDER",
                        CardStatus = "Pending Activation",
                        CardId = 1,
                        ExpirationDate = "2312",
                        Pan = "3133",
                        CardType = "DR",
                        Locked = true
                    }
                }
            };

            IList<Card> cards = new List<Card>
            {
                new Card
                {
                    CardId = 1,
                    AssetId = "00aa0",
                    CustomerId = "SP-340",
                    ExpirationDate = "2312",
                    LastFour = "3133",
                    CardType = "DR",
                    AccountBalance = "0",
                    Bin = "00000",
                    BusinessName = "COMPORATE INC.",
                    CardHolder = "CORPORATE CARD HOLDER",
                    Locked = 1,
                    CardStatus = "Pending Activation"
                }
            };
            var cancellationToken = new CancellationToken();
            CardService
                .Setup(r => r.FindByCardType(It.IsAny<string>(), It.IsAny<string>(), cancellationToken)).Returns(Task.FromResult(cards));

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(CardService.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);
            var request = new ConsultCardsByCustomerIdRequest("SP-340", "DR");

            var result = await controller.ConsultDebitCardByCustomerId(request.CustomerId, request.CardType, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);

        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "ConsultCardByCustomerIdQuery_Error - Not found")]
        public async void ShouldNotConsultCardByCustomerIdNotFound()
        {
            IList<Card> cards = new List<Card>();

            var cancellationToken = new CancellationToken();
            CardService
                .Setup(r => r.FindByCardType(It.IsAny<string>(), It.IsAny<string>(), cancellationToken)).Returns(Task.FromResult(cards));

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(CardService.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);
            var request = new ConsultCardsByCustomerIdRequest("SP-340", "DR");

            try
            {
                await controller.ConsultDebitCardByCustomerId(request.CustomerId, request.CardType, new CancellationToken());
            }
            catch (Exception ex)
            {
                Assert.Equal(ex.Message, "No card was found for this client");
            }
        }
        #endregion

        #region Methods

        public static T GetInstance<T>()
        {
            T result = Provider.GetRequiredService<T>();
            ControllerBase controllerBase = result as ControllerBase;
            if (controllerBase != null)
            {
                SetControllerContext(controllerBase);
            }
            Controller controller = result as Controller;
            if (controller != null)
            {
                SetControllerContext(controller);
            }
            return result;
        }

        private static void SetControllerContext(Controller controller)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = HttpContextAccessor.Object.HttpContext
            };
        }

        private static void SetControllerContext(ControllerBase controller)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = HttpContextAccessor.Object.HttpContext
            };
        }
        #endregion
    }
}
