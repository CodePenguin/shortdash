using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace ShortDash.Target
{
    public static class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .UseWindowsService()
                .ConfigureAppConfiguration((HostingAbstractionsHostExtensions, ConfigurationBinder) =>
                {
                    var configPath = Environment.GetEnvironmentVariable("SHORTDASH_CONFIG_PATH");
                    if (!string.IsNullOrEmpty(configPath))
                    {
                        ConfigurationBinder.AddJsonFile(Path.Join(configPath, "appsettings.json"), optional: true, reloadOnChange: true);
                    }
                })
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureHostConfiguration(hostBuilder =>
                {
                    hostBuilder.AddCommandLine(args);
                });
        }

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
    }
}
