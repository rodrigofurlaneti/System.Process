using System.Net;
using System.Phoenix.Common.Exceptions;

namespace System.Process.CrossCutting.Web.ExceptionHandling
{
    public class PipelineProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        public PipelineProblemDetails(PipelineException exception)
        {
            Title = exception.Message;
            Status = (int)HttpStatusCode.InternalServerError;
            Detail = exception.Details ?? "InternalServerError";
            Type = "https://httpstatuses.com/500";
        }
    }
}