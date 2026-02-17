using AZM.Abyan.Identity.API.Middleware;
using AZM.Abyan.Identity.Application.Common.Interfaces;
using AZM.Abyan.Identity.Application.Models;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using AZM.Abyan.Identity.Infrastructure.Security.Authorization;
using AZM.Abyan.Identity.Infrastructure.Services;
using AZM.Abyan.Identity.Persistence.DbContexts;
using AZM.Abyan.Identity.Persistence.Persistence.Repositories;
using AZM.Abyan.Identity.Persistence.Repositories.GenericRepository;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

#region Services

// Controllers
builder.Services.AddControllers();

// DbContext
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Localization
builder.Services.AddLocalization();

// MediatR (REGISTER ONCE)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(
        AZM.Abyan.Identity.Application.Commands.Client.Create.CreateClientCommand
    ).Assembly);

    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

// FluentValidation (from Application layer)
builder.Services.AddValidatorsFromAssembly(
    typeof(AZM.Abyan.Identity.Application.Commands.Client.Create.CreateClientCommand).Assembly,
    ServiceLifetime.Scoped);

// Shared Localizer
builder.Services.AddScoped(provider =>
{
    var factory = provider.GetRequiredService<IStringLocalizerFactory>();
    return factory.Create("SharedResources", typeof(SharedResource).Assembly.FullName!);
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
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
            },
            Array.Empty<string>()
        }
    });
});

// Authentication
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    var keycloakSettings = builder.Configuration.GetSection("Keycloak").Get<KeycloakConfiguration>();
//    var keycloakUrl = keycloakSettings?.BaseUrl ?? "http://localhost:8080";
//    var realm = keycloakSettings?.Realm ?? "Abyan";

//    options.Authority = $"{keycloakUrl}/realms/{realm}";
//    options.RequireHttpsMetadata = false;

//        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            //ValidIssuer= $"{keycloakUrl}/realms/{realm}",
//            ValidIssuers = new[]
//            {
//                $"{keycloakUrl}/realms/{realm}",
//                $"{keycloakUrl}/realms/master"
//            },
//            ValidateAudience = true,
//            AudienceValidator = (audiences, securityToken, validationParameters) =>
//            {
//                // allow any audience that contains "formbuilder"
//                return audiences.Contains("formbuilder");
//            },
//            ValidateLifetime = true,
//            NameClaimType = "preferred_username",
//            RoleClaimType = "roles"
//        };
//    });
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var keycloakSettings = builder.Configuration.GetSection("Keycloak")
            .Get<KeycloakConfiguration>();
        var keycloakUrl = keycloakSettings?.BaseUrl ?? "http://localhost:8080";
        var realm = keycloakSettings?.Realm ?? "Abyan";

        options.Authority = $"{keycloakUrl}/realms/{realm}";
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"{keycloakUrl}/realms/{realm}",
            ValidateAudience = true,
            ValidAudience = "formbuilder", // for single audience fallback
            ValidateLifetime = false,
            ClockSkew = TimeSpan.FromMinutes(5),
            NameClaimType = "preferred_username",
            RoleClaimType = "roles",
            AudienceValidator = (audiences, token, parameters) =>
            {
                return audiences.Contains("formbuilder");
            }
        };
    });

// Keycloak configuration - using KeycloakFormbuilder as default application config
builder.Services.Configure<KeycloakConfiguration>(
    builder.Configuration.GetSection("KeycloakConfigurations:Tenants:Abyan:KeycloakFormbuilder"));
builder.Services.AddHttpContextAccessor();

builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024;
    options.CompactionPercentage = 0.25;
});

// HttpClient
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
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IRealmResolverService, RealmResolverService>();

// Generic Repositories
builder.Services.AddScoped<IRepository<Tenant, Guid>, Repository<Tenant, Guid, IdentityDbContext>>();
builder.Services.AddScoped<IRepository<User, Guid>, Repository<User, Guid, IdentityDbContext>>();
builder.Services.AddScoped<IRepository<Client, Guid>, Repository<Client, Guid, IdentityDbContext>>();
builder.Services.AddScoped<IRepository<Role, Guid>, Repository<Role, Guid, IdentityDbContext>>();
builder.Services.AddScoped<IRepository<Scope, Guid>, Repository<Scope, Guid, IdentityDbContext>>();
builder.Services.AddScoped<IRepository<Resource, Guid>, Repository<Resource, Guid, IdentityDbContext>>();
builder.Services.AddScoped<IRepository<Policy, Guid>, Repository<Policy, Guid, IdentityDbContext>>();
builder.Services.AddScoped<IRepository<Permission, Guid>, Repository<Permission, Guid, IdentityDbContext>>();
builder.Services.AddScoped<IRepository<TenantUserRole, Guid>, Repository<TenantUserRole, Guid, IdentityDbContext>>();

// Sync Services
builder.Services.AddScoped<ITenantSyncService, TenantSyncService>();
builder.Services.AddScoped<IUserSyncService, UserSyncService>();
builder.Services.AddScoped<IClientSyncService, ClientSyncService>();
builder.Services.AddScoped<IRoleSyncService, RoleSyncService>();
builder.Services.AddScoped<IScopeSyncService, ScopeSyncService>();
builder.Services.AddScoped<IResourceSyncService, ResourceSyncService>();
builder.Services.AddScoped<IPolicySyncService, PolicySyncService>();
builder.Services.AddScoped<IPermissionKeycloakSyncService, PermissionKeycloakSyncService>();
builder.Services.AddScoped<ITenantUserRoleSyncService, TenantUserRoleSyncService>();
builder.Services.AddScoped<ISyncOrchestratorService, SyncOrchestratorService>();
builder.Services.AddScoped<ITenantProvider, JwtTenantProvider>();
builder.Services.Configure<KeycloakOptions>(
    builder.Configuration.GetSection("Keycloak"));

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IUmaAuthorizationService, KeycloakUmaAuthorizationService>();
#endregion

var app = builder.Build();

#region Middleware

// Global Exception Handler
app.UseExceptionHandler("/error");

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AZM Identity API v1");
        c.RoutePrefix = "swagger";
    });
}

// HTTPS
app.UseHttpsRedirection();

// Localization
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en")
    .AddSupportedCultures("en", "ar")
    .AddSupportedUICultures("en", "ar");

app.UseRequestLocalization(localizationOptions);

// Security
app.UseAuthentication();
app.UseMiddleware<UmaAuthorizationMiddleware>();
app.UseAuthorization();

// app.UseMiddleware<PermissionMiddleware>();


app.MapControllers();

#endregion

#region Startup Tasks

using (var scope = app.Services.CreateScope())
{
    var permissionSyncService = scope.ServiceProvider
        .GetRequiredService<IPermissionSyncService>();

    try
    {
        await permissionSyncService
            .SyncPermissionsAsync(Assembly.GetExecutingAssembly());
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Failed to sync permissions on startup.");
    }
}

#endregion

app.Run();
