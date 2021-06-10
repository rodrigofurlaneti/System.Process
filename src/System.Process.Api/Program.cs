using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Process.CrossCutting.Logging;
using Steeltoe.Extensions.Configuration.ConfigServer;

namespace System.Process.Api
{
    public static class Program
    {
        public static void Main(string[] args) =>
            CreateHostBuilder(args)
                .Build()
                .Run();

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging()
                .ConfigureAppConfiguration((webHostBuilderContext, configurationBuilder) =>
                {
                    var hostingEnvironment = webHostBuilderContext.HostingEnvironment;
                    configurationBuilder.AddEnvironmentVariables();
                    configurationBuilder.AddConfigServer(hostingEnvironment.EnvironmentName);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
