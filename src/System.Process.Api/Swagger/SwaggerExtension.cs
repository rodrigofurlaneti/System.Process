using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Phoenix.Web.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.IO;

namespace System.Pricing.Api.Swagger
{
    public static class SwaggerExtensions
    {
        private const string HeaderVersion = "x-api-version";
        private const string Format = "'v'VVV";

        public static void UseSystemSwagger(this IApplicationBuilder app, IApiVersionDescriptionProvider provider, string basePath)
        {
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swagger, httpReq) =>
                {
                    swagger.Servers = new List<OpenApiServer> {
                        new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}{httpReq.PathBase.ToUriComponent()}" }
                };
                OpenApiPaths paths = new OpenApiPaths();
                foreach (var path in swagger.Paths)
                {
                    paths.Add(string.Concat(basePath, path.Key), path.Value);
                }
                swagger.Paths = paths;
                });
            });

            app.UseSwaggerUI(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                    $"./swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
                    options.RoutePrefix = string.Empty;
                }

                options.DocExpansion(DocExpansion.List);
            });
        }

        public static IServiceCollection AddSystemSwagger(this IServiceCollection services, IConfiguration configuration, string xmlFile)
        {
            services.AddApiVersioning(options =>
            {
                options.UseApiBehavior = false;
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.RegisterMiddleware = false;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new HeaderApiVersionReader(HeaderVersion),
                    new QueryStringApiVersionReader(),
                    new UrlSegmentApiVersionReader());
            });

            services.AddVersionedApiExplorer(p =>
            {
                p.GroupNameFormat = Format;
                p.SubstituteApiVersionInUrl = true;
            });

            services.Configure<SwaggerConfig>(configuration);

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwagger>();
            services.AddSwaggerGen(options =>
            {
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
                options.AddSecurityDefinition("SystemT",
                    new OpenApiSecurityScheme
                    {
                        Description = @"System Token",
                        Name = "SystemT",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "SystemT"
                    });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "SystemT"
                            },
                            Scheme = "SystemT",
                            Name = "SystemT",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });

            return services;
        }
    }
}