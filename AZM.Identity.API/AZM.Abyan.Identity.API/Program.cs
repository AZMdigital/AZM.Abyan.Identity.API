using AZM.Identity.Application.Models;
using AZM.Identity.Application.Services;
using AZM.Identity.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace AZM.Identity.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Add Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "AZM Identity API",
                    Version = "v1",
                    Description = "Keycloak API Interface for Identity Management",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "AZM Identity Service"
                    }
                });

                // Add JWT Bearer token authentication
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.\n\nExample: \"Bearer 12345abcdef\"",
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
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // Configure Keycloak settings
            builder.Services.Configure<KeycloakConfiguration>(
                builder.Configuration.GetSection("Keycloak"));

            // Register HttpClient for KeycloakService
            builder.Services.AddHttpClient<IKeycloakService, KeycloakService>((serviceProvider, client) =>
            {
                var config = serviceProvider.GetRequiredService<IOptions<KeycloakConfiguration>>().Value;
                client.BaseAddress = new Uri(config.BaseUrl);
            });

            // Register Application Services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IClientService, ClientService>();
            builder.Services.AddScoped<IGroupService, GroupService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AZM Identity API v1");
                    c.RoutePrefix = "swagger"; // Swagger UI available at /swagger
                });
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
