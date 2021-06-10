using System.Net;
using System.Phoenix.Common.Exceptions;

namespace System.Process.CrossCutting.Web.ExceptionHandling
{
    public class ConflictBusinessProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        public ConflictBusinessProblemDetails(ConflictException exception)
        {
            Title = exception.Message;
            Status = (int)HttpStatusCode.Conflict;
            Detail = exception.Details ?? "Conflict";
            Type = "https://httpstatuses.com/409";
        }
    }
}