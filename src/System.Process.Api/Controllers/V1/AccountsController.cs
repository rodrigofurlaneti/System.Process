using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Process.Application.Commands.CardReplace;
using System.Process.Application.Commands.ChangeCardStatus;
using System.Process.Application.Commands.ValidateAccount;
using System.Process.Application.Queries.ConsultProcessByAccountId;
using System.Process.Application.Queries.ConsultProcessByCustomerId;
using System.Process.Application.Queries.GetAccountHistory;

namespace System.Process.Api.Controllers.V1
{
    [ApiController]
    [Produces("application/json")]
    [Route("v{version:apiVersion}/[controller]")]
    public class ProcessController : Controller
    {
        #region Properties
        private IMediator Mediator { get; }
        private ILogger<ProcessController> Logger { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public ProcessController(
            IMediator mediator,
            ILogger<ProcessController> logger)
        {
            Logger = logger;
            Mediator = mediator;
        }
        #endregion

        #region Actions

        ///// <summary>
        ///// Consult Process by customer id
        ///// </summary>
        ///// <returns> Process </returns>
        [Authorize]
        [Authorize(Policy = "ValidateConsultProcess")]
        [HttpGet("consult/customer/{applicationId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ConsultProcessByCustomerIdResponse), StatusCodes.Status200OK)]
        [ValidateCustomerId]
        public async Task<ActionResult> ConsultProcess([FromRoute] string applicationId, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Consult the Process of a customer - applicationId: { applicationId }");
            var response = await Mediator.Send(new ConsultProcessByCustomerIdRequest(applicationId), cancellationToken);
            return Ok(response);
        }

        ///// <summary>
        ///// Consult Process by account id
        ///// </summary>
        ///// <returns> Process </returns>
        [Authorize]
        [HttpGet("consult/account/{accountId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ConsultProcessByAccountIdResponse), StatusCodes.Status200OK)]
        [ValidateAccountId]
        public async Task<ActionResult> ConsultProcessByAccountId([FromRoute] string accountId, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Consult the Process of an account - accountId: { accountId }");
            var response = await Mediator.Send(new ConsultProcessByAccountIdRequest(accountId), cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Get Account History
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("accounthistory")]
        [ProducesResponseType(typeof(GetAccountHistoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAccountHistory([FromQuery] GetAccountHistoryRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Receive account history query request");
            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        ///// <summary>
        ///// Validate Process by account id
        ///// </summary>
        ///// <returns> Process </returns>
        [Authorize]
        [HttpGet("validate/{accountId}/{accountType}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ConsultProcessByAccountIdResponse), StatusCodes.Status200OK)]
        [ValidateAccountValidator]
        public async Task<ActionResult> ValidateAccount([FromRoute] string accountId, string accountType, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Validate the account - accountId: { accountId }");
            var request = new ValidateAccountRequest
            {
                AccountId = accountId,
                AccountType = accountType
            };
            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        ///// <summary>
        ///// Lock or Unlock the debit card on FIS and in debit card database
        ///// </summary>
        ///// <returns>Lock card</returns>
        [Authorize]
        [HttpPut("changecardstatus")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ChangeCardStatusResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> ChangeCardStatus([FromBody] ChangeCardStatusRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Receive Change Card Status Request");
            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        ///// Card replace
        ///// </summary>
        ///// <returns>Card replace</returns>
        [Authorize]
        [HttpPost("cardreplace")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(CardReplaceResponse), StatusCodes.Status201Created)]
        public async Task<ActionResult> CardReplace([FromBody] CardReplaceRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Receive Card Replace Request");
            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        #endregion
    }
}
