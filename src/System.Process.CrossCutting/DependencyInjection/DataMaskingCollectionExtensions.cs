using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Phoenix.Web.Configs;

namespace System.Process.CrossCutting.DependencyInjection
{
    public static class DataMaskingCollectionExtensions
    {
        public static IServiceCollection AddDataMaskingConfig(this IServiceCollection services, IConfiguration configuration)
        {
            var configSection = configuration.GetSection("DataMasking");
            var config = configSection.Get<DataMasking>();

            if (config != null)
            {
                services.Configure<DataMasking>(configSection);
            }

            return services;
        }
    }
}