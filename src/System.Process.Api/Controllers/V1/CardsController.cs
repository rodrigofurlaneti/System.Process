using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Process.Application.Clients.Cards;
using System.Process.Application.Commands.ActivateCard;
using System.Process.Application.Commands.ChangeCardPin;
using System.Process.Application.Commands.CreditCard;
using System.Process.Application.Commands.CreditCardActivation;
using System.Process.Application.Commands.CreditCardAgreement;
using System.Process.Application.Commands.CreditCardBalance;
using System.Process.Application.Commands.CreditCardCancellation;
using System.Process.Application.Commands.CreditCardChangeStatus;
using System.Process.Application.Commands.CreditCardDeclinedByCredit;
using System.Process.Application.Commands.CreditCardMakePayment;
using System.Process.Application.Commands.CreditCardReplace;
using System.Process.Application.Commands.SearchTransactions;
using System.Process.Application.Commands.TransactionDetail;
using System.Process.Application.Queries.ConsultProcessByCustomerId;
using System.Process.Application.Queries.ConsultCardsByCustomerId;
using System.Process.Application.Queries.SearchCreditCard;
using System.Process.Application.Queries.SearchCreditCardsTransactions;

namespace System.Process.Api.Controllers.V1
{
    [ApiController]
    [Produces("application/json")]
    [Route("v{version:apiVersion}/[controller]")]
    public class CardsController : Controller
    {
        #region Properties
        private IMediator Mediator { get; }
        private ILogger<CardsController> Logger { get; }
        private ICardService CardService { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public CardsController(
            IMediator mediator,
            ILogger<CardsController> logger,
            ICardService cardService
        )
        {
            Logger = logger;
            Mediator = mediator;
            CardService = cardService;
        }
        #endregion

        #region Actions

        ///// <summary>
        ///// Get cards by customer ID
        ///// </summary>
        ///// <returns> C </returns>
        [Authorize]
        [HttpGet("{customerId}/{cardType:alpha}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ConsultCardsByCustomerIdResponse), StatusCodes.Status200OK)]
        [ValidateConsultCardsByCustomerId]
        public async Task<ActionResult> ConsultDebitCardByCustomerId([FromRoute] string customerId, [FromRoute] string cardType, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Consult cards of a customer - customerId: { customerId } - { cardType }");
            var response = await Mediator.Send(new ConsultCardsByCustomerIdRequest(customerId, cardType), cancellationToken);
            return Ok(response);
        }

        ///// <summary>
        ///// Activate Card on FIS and update on Oracle
        ///// </summary>
        ///// <returns>Download Statements</returns>
        [Authorize]
        [HttpPost("activatecard")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ValidateCustomerId]
        public async Task<ActionResult> ActivateCard([FromBody] ActivateCardRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Receive Activate Card Request. CardId: {request.CardId} Pan: {request.Pan} ExpirationDate: {request.ExpireDate}");
            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        ///// <summary>
        ///// Change PIN on FIS and update on Oracle
        ///// </summary>
        ///// <returns>Download Statements</returns>
        [Authorize]
        [HttpPut("pin/change")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ChangeCardPin([FromBody] ChangeCardPinRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Receive ChangeCardPin Request. CardId: {request.CardId} CustomerId: {request.CustomerId}");
            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("searchtransactions")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(SearchTransactionsResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchTransactions([FromBody] ConsultCardsByidCardRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Activate Card on FIS and Update on Oracle DB");
            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("transactiondetail")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(TransactionDetailResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> TransactionDetail([FromBody] ConsultCardsByKeyTransactionRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Receive Get Transaction Detail Request");
            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [Authorize]
        [Authorize(Policy = "ValidateSearchCreditCards")]
        [HttpGet("credit/{SystemId}")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(SearchCreditCardResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> SearchCreditCards([FromRoute] string SystemId, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Receive search credit cards Request - SystemId: { SystemId }");
            var request = new SearchCreditCardRequest
            {
                SystemId = SystemId
            };

            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [Authorize]
        [Authorize(Policy = "ValidateCreditCardRequest")]
        [HttpPost("credit/request")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CreditCardResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> CreditCardRequest([FromBody] CreditCardRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Receive credit card request - SystemId: { request.SystemId }");

            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [Authorize]
        [Authorize(Policy = "ValidateCreditCardAgreement")]
        [HttpPost("credit/agreement")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CreditCardAgreementResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> CreditCardAgreement([FromBody] CreditCardAgreementRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Receive credit card agreement request - AssetId: { request.AssetId }");

            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [Authorize]
        [Authorize(Policy = "ValidateCreditCardActivation")]
        [HttpPost("credit/activation")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CreditCardActivationResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> CreditCardActivation([FromBody] CreditCardActivationRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Receive credit card activation request - AssetId: { request.CardId }");

            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("credit/cancel")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CreditCardCancellationResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> CancelCreditCardRequest([FromBody] CreditCardCancellationRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Receive credit card cancellation request - AssetId: { request.AssetId }");

            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("credit/balance/{cardId}")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CreditCardBalanceResponse), StatusCodes.Status200OK)]
        [ValidateCardId]
        public async Task<ActionResult> CreditCardBalance([FromRoute] string cardId, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Receive credit card balance inquiry request - CardId: { cardId }");

            var request = new CreditCardBalanceRequest
            {
                CardId = int.Parse(cardId)
            };

            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("credit/transactions/{cardId}")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(SearchCreditCardsTransactionsResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> SearchCreditCardsTransactions([FromRoute] string cardId, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Receive search credit cards transactions Request");
            var request = new SearchCreditCardsTransactionsRequest
            {
                CardId = cardId
            };

            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [Authorize]
        [HttpPut("credit/status")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ChangeCreditCardStatus([FromBody] CreditCardChangeStatusRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Receive change credit card status request. CardId: {request.CardId}");
            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("credit/replace")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CreditCardReplaceResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> CreditCardReplace([FromBody] CreditCardReplaceRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Receive credit card replace request - CardId: { request.CardId }");

            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [Authorize]
        [HttpPut("credit/declinedByCredit")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> CreditCardDeclinedByCredit([FromBody] CreditCardDeclinedByCreditRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Receive Credit Card Declined By Credit request. AssetId: {request.AssetId}");
            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }



        [Authorize]
        [HttpPost("credit/makePayment")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> CreditCardMakePayment([FromBody] CreditCardMakePaymentRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Receive Credit Card Make A Payment request. CardId: {request.CardId}");
            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }


        #endregion
    }
}
