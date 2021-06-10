using System;
using Hellang.Middleware.ProblemDetails;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Process.CrossCutting.Web.ExceptionHandling;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Silverlake.Base.Exceptions;

namespace System.Process.CrossCutting.DependencyInjection
{
    /// <summary>
    /// Responsible for adding middleware for problems and exceptions
    /// </summary>
    public static class WebServiceCollectionExtensions
    {
        /// <summary>
        /// Uses Hellang.Middleware.ProblemDetails AddProblemDetails method for mapping exception to problem details
        /// </summary>
        /// <param name="services">Interface fo Service Collection</param>
        /// <returns>Interface fo Service Collection</returns>
        public static IServiceCollection AddProblems(this IServiceCollection services,
            IHostEnvironment hostEnvironment) =>
                services.AddProblemDetails(setup =>
                {
                    setup.IncludeExceptionDetails = x => hostEnvironment.IsEnvironment("LOCAL");
                    setup.Map<NotFoundException>(ex => new NotFoundBusinessProblemDetails(ex));
                    setup.Map<ConflictException>(ex => new ConflictBusinessProblemDetails(ex));
                    setup.Map<PipelineException>(ex => new PipelineProblemDetails(ex));
                    setup.Map<UnprocessableEntityException>(ex => new UnprocessableRequestProblemDetails(ex));
                    setup.Map<TimeoutException>(ex => new TimeoutProblemDetails(ex));
                    setup.Map<ProxyException>(ex => new ProxyBusinessProblemDetails(ex));
                    setup.Map<SilverlakeException>(ex => new ServiceExceptionProblemDetails(ex));
                });
    }
}