using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Phoenix.DataAccess.MongoDb;
using System.Phoenix.Domain;

namespace System.Process.CrossCutting.DependencyInjection
{
    public static class MongoDbServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoClient<TEntity, TId>(this IServiceCollection services, IConfiguration configuration)
            where TEntity : BaseEntity<TId>
        {
            var name = GetNameWithoutGenerics(typeof(TEntity));
            var config = configuration.GetSection($"MongoDB:{name}");
            var clientConfig = config.Get<MongoConfig>();

            if (clientConfig == null)
            {
                throw new InvalidOperationException($"The configuration section 'MongoDB:{name}' was not found.");
            }

            return services.AddScoped((context) =>
            {
                var logger = services.BuildServiceProvider().GetService<ILogger<MongoDbClient<TEntity, TId>>>();
                return new MongoDbClient<TEntity, TId>(clientConfig, logger);
            });
        }

        private static string GetNameWithoutGenerics(Type t)
        {
            string name = t.Name;
            int index = name.IndexOf('`');
            return index == -1 ? name : name.Substring(0, index);
        }
    }
}