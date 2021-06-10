using System;
using System.Net;

namespace System.Process.CrossCutting.Web.ExceptionHandling
{
    public class TimeoutProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
    {
        public TimeoutProblemDetails(TimeoutException exception)
        {
            Title = exception.Message;
            Status = (int)HttpStatusCode.InternalServerError;
            Detail = $"Timeout Exception on {exception.Source}";
            Type = "https://httpstatuses.com/500";
        }
    }
}