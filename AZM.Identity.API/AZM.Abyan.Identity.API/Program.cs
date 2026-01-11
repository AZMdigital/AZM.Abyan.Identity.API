using System;
using AZM.Abyan.Identity.Domain.Interfaces;
using AZM.Abyan.Identity.Infrastructure.Services;
using AZM.Abyan.Identity.Persistence.DbContexts;
using AZM.Abyan.Identity.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AZM.Abyan.Identity.Application.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// Swagger
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

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description =
            "JWT Authorization header using the Bearer scheme. " +
            "Enter 'Bearer' [space] and then your token.\n\n" +
            "Example: \"Bearer 12345abcdef\"",
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

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var keycloakSettings = builder.Configuration.GetSection("KeycloakConfigurations:Tenants:Abyan:KeycloakFormbuilder").Get<KeycloakConfiguration>();
    var keycloakUrl = keycloakSettings?.BaseUrl ?? "http://localhost:8080";
    var realm = keycloakSettings?.Realm ?? "Abyan";

    options.Authority = $"{keycloakUrl}/realms/{realm}";
    options.RequireHttpsMetadata = false; // For local development

    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuers = new[]
        {
            $"{keycloakUrl}/realms/{realm}",
            $"{keycloakUrl}/realms/master" // Allow master realm tokens for super admin
        },
        ValidateAudience = false, // Disabled for simplicity in admin tool
        ValidateLifetime = true
    };
});

// Keycloak configuration - using KeycloakFormbuilder as default application config
builder.Services.Configure<KeycloakConfiguration>(
    builder.Configuration.GetSection("KeycloakConfigurations:Tenants:Abyan:KeycloakFormbuilder"));
builder.Services.AddHttpContextAccessor();

// HttpClient for Keycloak
builder.Services.AddHttpClient<IKeycloakService, KeycloakService>((sp, client) =>
{
    var config = sp.GetRequiredService<IOptions<KeycloakConfiguration>>().Value;
    client.BaseAddress = new Uri(config.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});

// Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IRealmAdminService, RealmAdminService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IPermissionSyncService, PermissionSyncService>();

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AZM Identity API v1");
        c.RoutePrefix = "swagger";
    });

}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
//app.UseMiddleware<AZM.Abyan.Identity.API.Middleware.PermissionMiddleware>();

app.MapControllers();

// Sync Permissions on Startup
using (var scope = app.Services.CreateScope())
{
    var permissionSyncService = scope.ServiceProvider.GetRequiredService<IPermissionSyncService>();
    try 
    {
        await permissionSyncService.SyncPermissionsAsync(System.Reflection.Assembly.GetExecutingAssembly());
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Failed to sync permissions on startup.");
    }
}

app.Run();
