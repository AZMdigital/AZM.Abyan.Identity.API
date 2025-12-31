using LocalPolicy.Attributes;
using LocalPolicy.Handlers;
using LocalPolicy.Models;
using LocalPolicy.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LocalPolicy
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
                    Title = "Local Policy API",
                    Version = "v1",
                    Description = "Local Policy API with Keycloak Integration"
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
            var keycloakConfig = builder.Configuration.GetSection("Keycloak").Get<KeycloakConfiguration>() 
                ?? throw new InvalidOperationException("Keycloak configuration is missing");
            
            builder.Services.Configure<KeycloakConfiguration>(
                builder.Configuration.GetSection("Keycloak"));

            // Configure JWT Authentication
            var keycloakAuthority = $"{keycloakConfig.BaseUrl}/realms/{keycloakConfig.Realm}";
            
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = keycloakAuthority;
                options.Audience = keycloakConfig.ClientId;
                options.RequireHttpsMetadata = false; // Set to true in production
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero
                };
                
                // Map roles from token claims
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        // Extract roles from token and add them as role claims
                        if (context.Principal?.Identity is System.Security.Claims.ClaimsIdentity identity)
                        {
                            var roles = new List<string>();
                            
                            // Check in "roles" claim (if mapper uses "roles" as Token Claim Name)
                            var rolesClaims = context.Principal.FindAll("formbuilder");
                            foreach (var claim in rolesClaims)
                            {
                                if (!string.IsNullOrEmpty(claim.Value))
                                {
                                    // Handle JSON array format: ["admin", "employee"] or string format
                                    if (claim.Value.TrimStart().StartsWith("["))
                                    {
                                        // JSON array - parse it
                                        try
                                        {
                                            var roleArray = System.Text.Json.JsonSerializer.Deserialize<string[]>(claim.Value);
                                            if (roleArray != null)
                                            {
                                                roles.AddRange(roleArray);
                                            }
                                        }
                                        catch
                                        {
                                            // If parsing fails, try splitting by comma
                                            var roleValues = claim.Value.Trim('[', ']', '"', ' ')
                                                .Split(',', StringSplitOptions.RemoveEmptyEntries);
                                            roles.AddRange(roleValues.Select(r => r.Trim(' ', '"')));
                                        }
                                    }
                                    else
                                    {
                                        // Single role or space-separated roles
                                        var roleValues = claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                        roles.AddRange(roleValues);
                                    }
                                }
                            }
                            
                            // Check in "resource_access" claim (Keycloak default location for client roles)
                            var resourceAccessClaim = context.Principal.FindFirst("resource_access");
                            if (resourceAccessClaim != null)
                            {
                                try
                                {
                                    using var doc = System.Text.Json.JsonDocument.Parse(resourceAccessClaim.Value);
                                    if (doc.RootElement.TryGetProperty(keycloakConfig.ClientId, out var clientElement))
                                    {
                                        if (clientElement.TryGetProperty("roles", out var rolesElement))
                                        {
                                            foreach (var role in rolesElement.EnumerateArray())
                                            {
                                                roles.Add(role.GetString() ?? string.Empty);
                                            }
                                        }
                                    }
                                }
                                catch
                                {
                                    // Ignore JSON parsing errors
                                }
                            }
                            
                            // Add roles as role claims (remove duplicates)
                            foreach (var role in roles.Distinct())
                            {
                                if (!string.IsNullOrEmpty(role) && 
                                    !identity.HasClaim(System.Security.Claims.ClaimTypes.Role, role))
                                {
                                    identity.AddClaim(new System.Security.Claims.Claim(
                                        System.Security.Claims.ClaimTypes.Role, 
                                        role));
                                }
                            }
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            // Register Authorization handlers
            builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
            builder.Services.AddSingleton<IAuthorizationHandler, Action4AuthorizationHandler>();

            // Register permission policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Permission:action4", policy =>
                    policy.Requirements.Add(new PermissionRequirement("action4")));
                
                // Policy for action4: admin role OR action4 permission
                options.AddPolicy("Action4Access", policy =>
                    policy.Requirements.Add(new Action4Requirement()));
            });

            // Register HttpClient for KeycloakAuthService
            builder.Services.AddHttpClient<IKeycloakAuthService, KeycloakAuthService>((serviceProvider, client) =>
            {
                var config = serviceProvider.GetRequiredService<IOptions<KeycloakConfiguration>>().Value;
                client.BaseAddress = new Uri(config.BaseUrl);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Local Policy API v1");
                    c.RoutePrefix = "swagger"; // Swagger UI available at /swagger
                });
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
