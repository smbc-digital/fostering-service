using System.Collections.Generic;
using fostering_service.Helpers;
using fostering_service.Services.Application;
using fostering_service.Services.Case;
using fostering_service.Services.HomeVisit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace fostering_service.Utils.ServiceCollectionExtensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterHelpers(this IServiceCollection services)
        {
            services.AddSingleton<ICaseHelper, CaseHelper>();
        }

        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddTransient<IHomeVisitService, HomeVisitService>();
            services.AddTransient<ICaseService, CaseService>();
            services.AddTransient<IApplicationService, ApplicationService>();
        }

        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fostering service API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    In = ParameterLocation.Header,
                    Description = "Authorization using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new List<string>()
                    }
                });
                c.CustomSchemaIds(x => x.FullName);
            });
        }
    }
}
