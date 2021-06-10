using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Process.Infrastructure.Data;

namespace System.Process.CrossCutting.DependencyInjection
{
    public static class OracleDbServiceCollectionExtensions
    {
        public static IServiceCollection AddOracleClient(this IServiceCollection services, IConfiguration configuration, string entity)
        {
            var connectionString = configuration.GetSection($"OracleDb:{entity}:ConnectionString").Value;
            services.AddDbContext<DataContext>(options => options.UseOracle(connectionString), ServiceLifetime.Transient);

            return services;
        }

        public static IServiceCollection AddOracleCardClient(this IServiceCollection services, IConfiguration configuration, string entity)
        {
            var connectionString = configuration.GetSection($"OracleDb:{entity}:ConnectionString").Value;
            services.AddDbContext<DataCardContext>(options => options.UseOracle(connectionString), ServiceLifetime.Transient);

            return services;
        }
        public static IServiceCollection AddOracleTransferClient(this IServiceCollection services, IConfiguration configuration, string entity)
        {
            var connectionString = configuration.GetSection($"OracleDb:{entity}:ConnectionString").Value;
            services.AddDbContext<DataTransferContext>(options => options.UseOracle(connectionString), ServiceLifetime.Transient);

            return services;
        }
    }
}
