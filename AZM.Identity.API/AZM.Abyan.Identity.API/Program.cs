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

// OpenAPI
builder.Services.AddOpenApi();

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

// Keycloak configuration
builder.Services.Configure<KeycloakConfiguration>(
    builder.Configuration.GetSection("Keycloak"));
builder.Services.AddHttpContextAccessor();

// HttpClient for Keycloak
builder.Services.AddHttpClient<IKeycloakService, KeycloakService>((sp, client) =>
{
    var config = sp.GetRequiredService<IOptions<KeycloakConfiguration>>().Value;
    client.BaseAddress = new Uri(config.BaseUrl);
});

// Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

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

    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
