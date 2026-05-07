using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using B2BAdmin.ApiDocument.Infrastructure;
using B2BAdmin.ApiDocument.Application;

[assembly: HostingStartup(typeof(B2BAdmin.ApiDocument.API.HostingStartup))]
namespace B2BAdmin.ApiDocument.API
{
    /// <summary>
    /// Hosting startup
    /// </summary>
    public class HostingStartup : IHostingStartup
    {
        /// <summary>
        /// Configure services
        /// </summary>
        /// <param name="builder"></param>
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddSingleton<ApiDocumentDbContext>();

                services.AddMediatR(typeof(AddDocumentMenuCommand));
                
                services.Scan(scan =>
                    scan.FromAssembliesOf(typeof(AddDocumentMenuCommand))
                        .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)))
                        .AsImplementedInterfaces()
                        .WithTransientLifetime()
                );
                services.AddValidatorsFromAssemblyContaining(typeof(AddDocumentMenuCommand), ServiceLifetime.Transient);

            });

            builder.ConfigureAppConfiguration((context, config) =>
            {
            });
        }
    }
}
