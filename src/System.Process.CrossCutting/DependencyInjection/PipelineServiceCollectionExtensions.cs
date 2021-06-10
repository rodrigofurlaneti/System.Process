using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Phoenix.Pipeline.Orchestrator;
using System.Phoenix.Pipeline.Orchestrator.Config;
using System.Phoenix.Pipeline.Orchestrator.Factory;

namespace System.Process.CrossCutting.DependencyInjection
{
    public static class PipelineServiceCollectionExtensions
    {
        public static IServiceCollection AddPipeline<TId>(
           this IServiceCollection services,
           IConfiguration configuration) =>
               services
                   .Configure<PipelineConfig>(setting => configuration.GetSection("Pipeline").Bind(setting))
                   .AddTransient<IPipeline<TId>, Pipeline<TId>>()
                   .AddMongoClient<Snapshot<TId>, TId>(configuration)
                   .AddTransient<ITransformBlockFactory<TId>, TransformBlockFactory<TId>>();
    }
}
