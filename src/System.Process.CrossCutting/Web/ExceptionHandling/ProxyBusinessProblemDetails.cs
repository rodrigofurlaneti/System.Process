using System.Net;
using System.Phoenix.Common.Exceptions;

namespace System.Process.CrossCutting.Web.ExceptionHandling
{
    public class ProxyBusinessProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        public ProxyBusinessProblemDetails(ProxyException exception)
        {
            Title = exception.Message;
            Status = exception.StatusCode ?? (int)HttpStatusCode.InternalServerError;
            Detail = exception.Details ?? $"Error executing {exception.Provider}'s operation.";
            Type = $"https://httpstatuses.com/{Status}";
        }
    }
}