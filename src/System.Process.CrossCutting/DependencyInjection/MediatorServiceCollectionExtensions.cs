using System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace System.Process.CrossCutting.DependencyInjection
{
    public static class MediatorServiceCollectionExtensions
    {
        public static IServiceCollection AddMediator(this IServiceCollection services)
        {
            var assembly = AppDomain.CurrentDomain.Load("System.Process.Application");
            services.AddMediatR(assembly);
            return services;
        }
    }
}