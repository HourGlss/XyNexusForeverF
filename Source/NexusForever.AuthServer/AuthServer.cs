using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Systemd;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using NexusForever.AuthServer.Network;
using NexusForever.Database;
using NexusForever.Database.Telemetry;
using NexusForever.Game;
using NexusForever.Network.Configuration.Model;
using NexusForever.Shared;
using NexusForever.Shared.Configuration;
using NexusForever.Telemetry;
using NLog;
using NLog.Extensions.Logging;
using OpenTelemetry;

namespace NexusForever.AuthServer
{
    internal static class AuthServer
    {
        #if DEBUG
        private const string Title = "NexusForever: Authentication Server (DEBUG)";
        #else
        private const string Title = "NexusForever: Authentication Server (RELEASE)";
        #endif

        private static readonly NLog.ILogger log = LogManager.GetCurrentClassLogger();

        private static void Main()
        {
            string basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Directory.SetCurrentDirectory(basePath);

            IHostBuilder builder = new HostBuilder()
                .ConfigureLogging((hb, c) =>
                {
                    c.ClearProviders()
                        .AddConfiguration(hb.Configuration.GetSection("Logging"))
                        .AddNLog(new NLogProviderOptions
                        {
                            RemoveLoggerFactoryFilter = false
                        });
                })
                .ConfigureAppConfiguration(cb =>
                {
                    cb.SetBasePath(basePath)
                        .AddJsonFile("AuthServer.json", false)
                        .AddJsonFile("Logging.json", true)
                        .AddEnvironmentVariables();
                })
                .ConfigureServices((hb, sc) =>
                {
                    OpenTelemetryBuilder otb = sc.AddNexusForeverTelemetry(
                        hb.Configuration.GetSection("Telemetry"))?
                            .AddEntityFrameworkTracing();

                    sc.AddHostedService<HostedService>();

                    sc.AddOptions<NetworkConfig>()
                        .Bind(hb.Configuration.GetSection("Network"));

                    sc.AddSingletonLegacy<ISharedConfiguration, SharedConfiguration>();

                    sc.AddDatabase();
                    sc.AddGame();
                    sc.AddAuthNetwork();
                    sc.AddShared();
                })
                .UseWindowsService()
                .UseSystemd();

            if (!WindowsServiceHelpers.IsWindowsService() && !SystemdHelpers.IsSystemdService())
                Console.Title = Title;

            try
            {
                IHost host = builder.Build();
                host.Run();
            }
            catch (Exception e)
            {
                log.Fatal(e);
            }
        }
    }
}
