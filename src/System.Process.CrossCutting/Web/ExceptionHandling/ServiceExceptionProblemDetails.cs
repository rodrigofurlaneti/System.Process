using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Proxy.Silverlake.Base.Exceptions;

namespace System.Process.CrossCutting.Web.ExceptionHandling
{
    public class ServiceExceptionProblemDetails : ProblemDetails
    {
        public ServiceExceptionProblemDetails(SilverlakeException exception)
        {
            Title = exception.Message;
            Status = (int)HttpStatusCode.UnprocessableEntity;
            Detail = exception.Message ?? "Error on Jack Henry Service";
            Type = "https://httpstatuses.com/422";
        }
    }
}
