using Elastic.Apm.NetCoreAll;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace System.Process.CrossCutting.Web.Middlewares.ElasticApm
{
    public static class UseElasticApmApplicationBuilderExtensions
    {
        private static readonly string Local = "Local";

        public static IApplicationBuilder UseElasticApmApplication(this IApplicationBuilder builder, IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            if (!hostEnvironment.IsEnvironment(Local))
            {
                var config = configuration.GetSection("ElasticApm");
                if (config.Exists())
                {
                    builder.UseAllElasticApm(configuration);
                }
            }

            return builder;
        }
    }
}
