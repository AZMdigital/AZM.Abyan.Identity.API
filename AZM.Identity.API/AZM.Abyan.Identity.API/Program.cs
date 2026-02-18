using System;
using System.Reflection;
using AZM.Abyan.Identity.API.Extensions;
using AZM.Abyan.Identity.API.Middleware;
using AZM.Abyan.Identity.Application.Models;
using AZM.Abyan.Identity.Application.Resources;
using AZM.Abyan.Identity.Application.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting AZM Identity API");

    #region Service Configuration
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console().WriteTo.PostgreSQL(
        connectionString: context.Configuration.GetConnectionString("DefaultConnection"),
        tableName: "Logs",
        restrictedToMinimumLevel: LogEventLevel.Error,
        needAutoCreateTable: true,
        columnOptions: new Dictionary<string, ColumnWriterBase>
        {
            { "Message", new RenderedMessageColumnWriter() },
            { "MessageTemplate", new MessageTemplateColumnWriter() },
            { "Level", new LevelColumnWriter() },
            { "TimeStamp", new TimestampColumnWriter() },
            { "Exception", new ExceptionColumnWriter() },
            { "LogEvent", new LogEventSerializedColumnWriter() },
            { "Properties", new PropertiesColumnWriter() }
        }
    ));
    builder.Services.AddIdentityServices(builder.Configuration);
    #endregion

    var app = builder.Build();

    #region Middleware

    // Global Exception Handler
    app.UseExceptionHandler("/error");

    // Swagger
    //if (app.Environment.IsDevelopment())
    if (true)
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
    app.UseAuthorization();
    app.UseMiddleware<UmaAuthorizationMiddleware>();

    // app.UseMiddleware<PermissionMiddleware>();

    app.MapControllers();

    #endregion

    #region Startup Tasks

    //using (var scope = app.Services.CreateScope())
    //{
    //    var permissionSyncService = scope.ServiceProvider
    //        .GetRequiredService<IPermissionSyncService>();

    //    try
    //    {
    //        await permissionSyncService
    //            .SyncPermissionsAsync(Assembly.GetExecutingAssembly());
    //    }
    //    catch (Exception ex)
    //    {
    //        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    //        logger.LogError(ex, "Failed to sync permissions on startup.");
    //    }
    //}

    #endregion

    #region Application Startup
    Log.Information("AZM Identity API started successfully on {Environment}",
        app.Environment.EnvironmentName);

    await app.RunAsync();
    #endregion
}
catch (Exception ex)
{
    Log.Fatal(ex, "AZM Identity API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
