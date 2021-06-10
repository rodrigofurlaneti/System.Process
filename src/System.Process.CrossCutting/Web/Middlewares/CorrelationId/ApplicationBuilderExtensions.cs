using System;
using Microsoft.AspNetCore.Builder;
using System.Phoenix.Common.CorrelationId;
using System.Phoenix.Web.Middlewares.CorrelationId;

namespace System.Process.CrossCutting.Web.Middlewares.CorrelationId
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        {
            return app.UseCorrelationId(new CorrelationIdConfig());
        }

        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app, string header)
        {
            return app.UseCorrelationId(new CorrelationIdConfig
            {
                Header = header
            });
        }

        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app, CorrelationIdConfig options)
        {
            if (app.ApplicationServices.GetService(typeof(ICorrelationContextFactory)) == null)
            {
                throw new InvalidOperationException("Unable to find the required services. You must call the AddCorrelationId method in ConfigureServices in the application startup code.");
            }

            return app.UseMiddleware<CorrelationIdMiddleware>(Microsoft.Extensions.Options.Options.Create(options));
        }
    }
}