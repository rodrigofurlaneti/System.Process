using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Process.Application.Commands.DownloadStatements;
using System.Process.Application.Commands.RetrieveStatement;
using System.Process.Application.Commands.StatementTypes;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Api.Controllers.V1
{
    [ApiController]
    [Produces("application/json")]
    [Route("v{version:apiVersion}/[controller]")]
    public class StatementsController : Controller
    {
        #region Properties
        private IMediator Mediator { get; }
        private ILogger<StatementsController> Logger { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public StatementsController(
            IMediator mediator,
            ILogger<StatementsController> logger)
        {
            Logger = logger;
            Mediator = mediator;
        }
        #endregion

        #region Actions

        ///// <summary>
        ///// Check Statement Types
        ///// </summary>
        ///// <returns>Statement Types</returns>
        [Authorize]
        [Authorize(Policy = "ValidateStatementTypes")]
        [HttpGet("{SystemId}")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> StatementTypes([FromRoute] string SystemId, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Receive Send Negotiation Request");
            var request = new StatementTypesRequest
            {
                SalesforceId = SystemId
            };
            var response = await Mediator.Send(request, cancellationToken);
            return Created(string.Empty, response);
        }

        ///// <summary>
        ///// Check download statements
        ///// </summary>
        ///// <returns>Download Statements</returns>
        [Authorize]
        [HttpGet("download/{statementId}")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DownloadStatements([FromRoute] string statementId, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Receive Send Negotiation Request");
            var request = new DownloadStatementsRequest
            {
                StatementId = statementId
            };

            var response = await Mediator.Send(request, cancellationToken);
            return Created(string.Empty, response);
        }

        ///// <summary>
        ///// Check Statements 
        ///// </summary>
        ///// <returns>Retrieve Statement</returns>
        [Authorize]
        [Authorize(Policy = "ValidateRetrieveStatement")]
        [HttpPost("retrieve")]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RetrieveStatement([FromBody] RetrieveStatementRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Receive Retrieve Statement Request");
            var response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        #endregion
    }
}
