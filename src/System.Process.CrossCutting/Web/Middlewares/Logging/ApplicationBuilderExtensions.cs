using Microsoft.AspNetCore.Builder;
using System.Phoenix.Web.Middlewares.Logging;

namespace System.Process.CrossCutting.Web.Middlewares.Logging
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseStructuredLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingMiddleware>();
        }
    }
}