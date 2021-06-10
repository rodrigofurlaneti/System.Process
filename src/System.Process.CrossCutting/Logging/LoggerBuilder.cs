using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Serilog;
using Serilog.Extensions.Logging;

namespace System.Process.CrossCutting.Logging
{
    public static class LoggerBuilder
    {
        public static IHostBuilder ConfigureLogging(this IHostBuilder builder) =>
            builder.ConfigureServices((HostBuilderContext context, IServiceCollection services) =>
            {
                services.AddSingleton<ILoggerFactory>(provider =>
                {
                    var loggerConfiguration = new LoggerConfiguration();
                    loggerConfiguration.ReadFrom.Configuration(context.Configuration);
                    var logger = loggerConfiguration.CreateLogger();
                    Log.Logger = logger;

                    logger.Information("Logging Start");

                    return new SerilogLoggerFactory(logger, true);
                });

                services.AddSingleton<Microsoft.Extensions.Logging.ILogger>(new LoggerFactory().CreateLogger<ConsoleLoggerProvider>());
            });
    }
}