using System.Net;
using System.Phoenix.Common.Exceptions;

namespace System.Process.CrossCutting.Web.ExceptionHandling
{
    /// <summary>
    /// Indicates that the origin server did not find a current representation 
    /// </summary>
    public class NotFoundBusinessProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        public NotFoundBusinessProblemDetails(NotFoundException exception)
        {
            Title = exception.Message;
            Status = (int)HttpStatusCode.NotFound;
            Detail = exception.Details ?? "Not Found";
            Type = "https://httpstatuses.com/404";
        }
    }
}