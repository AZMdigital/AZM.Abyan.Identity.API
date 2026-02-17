using AZM.Abyan.Identity.Application.Common;
using AZM.Abyan.Identity.Infrastructure.Common;
using AZM.Abyan.Identity.Persistence.Common;

namespace AZM.Abyan.Identity.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            #region Controllers
            services.AddControllers();
            #endregion
            #region API Documentation
            // Swagger
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "AZM Identity API",
                    Version = "v1",
                    Description = "Keycloak API Interface for Identity Management"
                });

                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                  Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                  Id = "Bearer"
                            }
                        },Array.Empty<string>()
                    }
                 });
            });
            #endregion
            #region CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .SetPreflightMaxAge(TimeSpan.FromHours(24)));
            });
            #endregion
            #region Core Services
            services.AddHttpContextAccessor();
            services.AddLocalization();
            services.AddMemoryCache(options =>
            {
                options.SizeLimit = 1024;
                options.CompactionPercentage = 0.25;
            });
            #endregion

            #region Application Services
            services.AddApplicationServices(configuration);
            services.AddInfrastructureServices(configuration);
            services.AddPersistenceServices(configuration);
            #endregion
            return services;
        }

    }
}
