using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Process.Application.Commands.RemoteDepositCapture;

namespace System.Deposit.Api.Controllers.V1
{
    [Authorize]
    [Authorize(Policy = "ValidateRemoteDeposit")]
    [ApiController]
    [Produces("application/json")]
    [Route("v{version:apiVersion}/[controller]")]
    public class DepositController : Controller
    {
        #region Properties
        private IMediator Mediator { get; }
        private ILogger<DepositController> Logger { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public DepositController(
            IMediator mediator,
            ILogger<DepositController> logger)
        {
            Logger = logger;
            Mediator = mediator;
        }
        #endregion

        #region Actions

        ///// <summary>
        ///// Send Remote Deposit Capture
        ///// </summary>
        ///// <returns>Remote Deposit Capture</returns>
        [HttpPost("remotedepositcapture")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> RemoteDepositCapture([FromBody] RemoteDepositCaptureRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Receive Send Remote Deposit Capture");
            var response = await Mediator.Send(request, cancellationToken);

            if (response.Errors?.Count > 0)
            {
                return UnprocessableEntity(response);
            }

            return Created(string.Empty, response);
        }     

        #endregion
    }
}
