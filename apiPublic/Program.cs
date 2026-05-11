using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sentry;
using Sentry.Protocol;
using Serilog;
using Serilog.Events;
using System;
using System.Globalization;

namespace ApiPlugin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JHaF5cWWdCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdlWXpfcXRRRGJfUUJ2VkpWYEo=");
            CreateHostBuilder(args).Build().Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            string envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            string envConfigFile = $"appsettings.{envName}.json";

            var builtConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile(envConfigFile)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var sentryConfig = builtConfig.GetSection("Sentry").Get<SentryConfig>();

            var logConfig = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(path: $"AppData/Log/logging_{DateTime.Now.ToString("yyyy_MM_dd", CultureInfo.InvariantCulture)}")
                .WriteTo.File(path: $"AppData/Log/logging_Error_{DateTime.Now.ToString("yyyy_MM_dd", CultureInfo.InvariantCulture)}",
                    restrictedToMinimumLevel: LogEventLevel.Error);

            if (sentryConfig.Enable)
            {
                logConfig = logConfig.WriteTo.Sentry(option =>
                {
                    option.MinimumBreadcrumbLevel = LogEventLevel.Information; // It requires at least this level to store breadcrumb
                    option.MinimumEventLevel = LogEventLevel.Error; // This level or above will result in event sent to Sentry
                    option.DiagnosticsLevel = SentryLevel.Error;
                    option.AttachStacktrace = true;

                    option.SendDefaultPii = true;
                    option.Debug = sentryConfig.Environment == "dev";

                    option.Dsn = new Dsn(sentryConfig.Dsn);
                    option.SampleRate = sentryConfig.SampleRate;
                    option.Environment = sentryConfig.Environment;
                    option.ServerName = sentryConfig.Source;
                });
            }
            Log.Logger = logConfig.CreateLogger();

            return Host.CreateDefaultBuilder(args)
                 .ConfigureLogging(logging =>
                 {
                     logging.ClearProviders();

                     logging.AddSerilog();
                     logging.AddConsole();
                 })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var maxRequestBodySize = builtConfig.GetValue<long?>("Kestrel:Limits:MaxRequestBodySize")
                        ?? 30L * 1024 * 1024;

                    webBuilder.UseStartup<Startup>()
                    .UseKestrel(options =>
                    {
                        // Keep request size bounded to avoid untrusted large payloads exhausting memory.
                        options.Limits.MaxRequestBodySize = maxRequestBodySize;
                    })
                    .UseSetting(WebHostDefaults.HostingStartupAssembliesKey, "B2BAdmin.ApiDocument.API");
                });
        }
            
    }
}
