using System.Net;
using System.Phoenix.Common.Exceptions;

namespace System.Process.CrossCutting.Web.ExceptionHandling
{
    public class UnprocessableRequestProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        public string ErrorCode { get; set; }
        public bool IsSelfHealing { get; set; }

        public UnprocessableRequestProblemDetails(UnprocessableEntityException exception)
        {
            Title = exception.Message;
            Status = (int)HttpStatusCode.UnprocessableEntity;
            Detail = exception.Details ?? "Unable to process the contained instructions";
            Type = "https://httpstatuses.com/422";
            ErrorCode = exception.ErrorCode;
            IsSelfHealing = exception.IsSelfHealing;
        }
    }
}