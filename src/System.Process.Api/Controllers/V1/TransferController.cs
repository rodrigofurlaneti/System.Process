using Ardalis.GuardClauses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Process.Application.Commands.AchTransferMoney;
using System.Process.Application.Commands.ResumeTransfer;
using System.Process.Application.Commands.TransferMoney;
using System.Process.Application.Commands.WireTransfer;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Api.Controllers.V1
{
    [ApiController]
    [Produces("application/json")]
    [Route("v{version:apiVersion}/[controller]")]
    [Authorize]
    [Authorize(Policy = "ValidateTransfer")]
    public class TransferController : ControllerBase
    {
        #region Properties

        private IMediator Mediator { get; }
        private ILogger<TransferController> Logger { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public TransferController(
            IMediator mediator,
            ILogger<TransferController> logger)
        {
            Guard.Against.Null(mediator, nameof(mediator));
            Guard.Against.Null(logger, nameof(logger));

            Logger = logger;
            Mediator = mediator;
        }

        #endregion

        #region Actions

        ///// <summary>
        ///// Transfer money
        ///// </summary>
        ///// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(AchTransferMoneyResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> TransferMoney([FromBody] TransferMoneyRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Receive TransferMoney Request");
            var response = await Mediator.Send(request, cancellationToken);

            return Ok(response);
        }

        [HttpPost("wiretransfer")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(WireTransferAddResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> WireTransferAdd([FromBody] WireTransferAddRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Receive WireTransferAdd Request");
            var response = await Mediator.Send(request, cancellationToken);

            return Ok(response);
        }

        ///// <summary>
        ///// ACH Transfer money
        ///// </summary>
        ///// <returns></returns>
        [HttpPost("Ach")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(AchTransferMoneyResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> AchTransferMoney([FromBody] AchTransferMoneyRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Receive AchTransferMoney Request");
            var response = await Mediator.Send(request, cancellationToken);

            return Ok(response);
        }

        ///// <summary>
        ///// Resume a transfer based in a Life Cycle ID
        ///// </summary>
        ///// <returns></returns>
        [HttpPost("resumeTransfer")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ResumeTransferResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult> ResumeTransfer([FromBody] ResumeTransferRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Resume transfer request for Life Cycle ID {request.LifeCycleId}");
            var response = await Mediator.Send(request, cancellationToken);

            return Ok(response);
        }

        #endregion
    }
}