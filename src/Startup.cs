using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using fostering_service.Helpers;
using fostering_service.Services.Application;
using fostering_service.Services.Case;
using fostering_service.Services.HomeVisit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockportGovUK.AspNetCore.Middleware;
using StockportGovUK.AspNetCore.Availability;
using StockportGovUK.AspNetCore.Availability.Middleware;
using Swashbuckle.AspNetCore.Swagger;
using StockportGovUK.AspNetCore.Gateways;
using StockportGovUK.AspNetCore.Gateways.VerintServiceGateway;
using StockportGovUK.AspNetCore.Polly;

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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddHttpClients<IGateway, Gateway>(Configuration);
            services.AddSingleton<IVerintServiceGateway, VerintServiceGateway>();

            services.AddSingleton<ICaseHelper, CaseHelper>();

            services.AddTransient<IHomeVisitService, HomeVisitService>();
            services.AddTransient<ICaseService, CaseService>();
            services.AddTransient<IApplicationService, ApplicationService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "fostering_service API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "Authorization using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });

                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }},
                });
                
                c.CustomSchemaIds(x => x.FullName);
            });

            services.AddHttpClient();

            services.AddAvailability();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            
            app.UseMiddleware<Availability>();
            app.UseMiddleware<ExceptionHandling>();
            app.UseHttpsRedirection();
            app.UseSwagger();

            var swaggerPrefix = env.IsDevelopment() ? string.Empty : "/fosteringservice";

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"{swaggerPrefix}/swagger/v1/swagger.json", "fostering_service API");
            });
            app.UseMvc();
        }
    }
}
