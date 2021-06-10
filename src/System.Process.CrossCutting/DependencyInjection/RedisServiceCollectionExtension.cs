using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Phoenix.DataAccess.Redis;

namespace System.Process.CrossCutting.DependencyInjection
{
    public static class RedisServiceCollectionExtension
    {
        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped((context) =>
            {
                var config = configuration.GetSection($"Redis");
                var redisConfig = config.Get<RedisConfig>();

                return redisConfig;
            });

            services.AddScoped<IRedisService, RedisService>();

            return services;
        }
    }
}
