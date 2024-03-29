﻿using System.Diagnostics.CodeAnalysis;
using fostering_service.Utils.ServiceCollectionExtensions;
using fostering_service.Utils.StorageProvider;
using fostering_service.Utils.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StockportGovUK.AspNetCore.Middleware;
using StockportGovUK.NetStandard.Gateways;
using StockportGovUK.NetStandard.Gateways.Extensions;
using StockportGovUK.NetStandard.Gateways.VerintService;

namespace fostering_service
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                    .AddNewtonsoftJson();
            services.AddStorageProvider(Configuration);
            services.AddHttpClient<IVerintServiceGateway, VerintServiceGateway>(Configuration);          
            services.AddSwagger();
            services.AddHealthChecks()
                    .AddCheck<TestHealthCheck>("TestHealthCheck");

            services.RegisterHelpers();
            services.RegisterServices();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsEnvironment("local"))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());

            app.UseMiddleware<ApiExceptionHandling>();
            
            app.UseHealthChecks("/healthcheck", HealthCheckConfig.Options);

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"{(env.IsEnvironment("local") ? string.Empty : "/fosteringservice")}/swagger/v1/swagger.json", "Fostering service API");
            });
        }
    }
}
