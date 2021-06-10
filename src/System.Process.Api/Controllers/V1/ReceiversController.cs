using Ardalis.GuardClauses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Process.Application.Commands.AddReceiver;
using System.Process.Application.Commands.DeleteReceiver;
using System.Process.Application.Queries.FindReceivers;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Api.Controllers.V1
{
    [ApiController]
    [Produces("application/json")]
    [Route("v{version:apiVersion}/[controller]")]
    public class ReceiversController : ControllerBase
    {
        #region Properties

        private IMediator Mediator { get; }
        private ILogger<ReceiversController> Logger { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public ReceiversController(
            IMediator mediator,
            ILogger<ReceiversController> logger)
        {

            Guard.Against.Null(mediator, nameof(mediator));
            Guard.Against.Null(logger, nameof(logger));

            Logger = logger;
            Mediator = mediator;
        }

        #endregion

        #region Actions

        ///// <summary>
        ///// Find Receiver
        ///// </summary>
        ///// <returns></returns>
        [Authorize]
        [HttpGet("{customerId}/{inquiryType}/{ownership}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(FindReceiversResponse), StatusCodes.Status200OK)]
        [FindReceiversValidator]
        public async Task<ActionResult> Find([FromRoute] string customerId, string inquiryType, string ownership, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Find Receive Request");
            var request = new FindReceiversRequest 
            { 
                CustomerId = customerId, 
                InquiryType = inquiryType,
                Ownership = ownership
            };
            var response = await Mediator.Send(request, cancellationToken);

            return Ok(response);
        }

        ///// <summary>
        ///// Add Receiver
        ///// </summary>
        ///// <returns></returns>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(AddReceiverResponse), StatusCodes.Status201Created)]
        public async Task<ActionResult> Add([FromBody] AddReceiverRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Add Receive Request");
            var response = await Mediator.Send(request, cancellationToken);

            return Created("", response);
        }

        ///// <summary>
        ///// Delete Receiver
        ///// </summary>
        ///// <returns></returns>
        [Authorize]
        [HttpDelete("{receiverId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(DeleteReceiverResponse), StatusCodes.Status200OK)]
        [DeleteReceiverValidator]
        public async Task<ActionResult> Delete([FromRoute] string receiverId, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Delete Receive Request");
            var request = new DeleteReceiverRequest
            {
                ReceiverId = receiverId
            };
            var response = await Mediator.Send(request, cancellationToken);

            return Ok(response);
        }

        #endregion
    }
}
