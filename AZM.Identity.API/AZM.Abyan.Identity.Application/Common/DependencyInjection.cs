using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AZM.Abyan.Identity.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace AZM.Abyan.Identity.Application.Common
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            #region MediatR Configuration
            // MediatR (REGISTER ONCE)
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(
                    AZM.Abyan.Identity.Application.Commands.Client.Create.CreateClientCommand
                ).Assembly);

                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            #endregion

            #region Validation Pipeline
            // FluentValidation (from Application layer)
             services.AddValidatorsFromAssembly(
                typeof(AZM.Abyan.Identity.Application.Commands.Client.Create.CreateClientCommand).Assembly,
                ServiceLifetime.Scoped);
            #endregion
            #region Localization
            // Shared Localizer
            services.AddScoped(provider =>
            {
                var factory = provider.GetRequiredService<IStringLocalizerFactory>();
                return factory.Create("SharedResources", typeof(SharedResource).Assembly.FullName!);
            });
            #endregion
            return services;
        }
    }
}
