using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShortDash.Core.Extensions;
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
                .ConfigureAppConfiguration((hostBuilderContext, configBuilder) =>
                {
                    var environmentConfigPath = Environment.GetEnvironmentVariable("SHORTDASH_CONFIG_PATH");
                    var configPath = !string.IsNullOrEmpty(environmentConfigPath) ? environmentConfigPath : EnvironmentExtensions.GetLocalApplicationDataFolderPath("ShortDash.Target");
                    if (!string.IsNullOrEmpty(configPath))
                    {
                        if (!File.Exists(Path.Join(configPath, "appsettings.json")))
                        {
                            File.Copy(Path.Join(Path.GetFullPath(Path.GetDirectoryName(AppContext.BaseDirectory)), "appsettings.json"), Path.Join(configPath, "appsettings.json"));
                        }
                        configBuilder.AddJsonFile(Path.Join(configPath, "appsettings.json"), optional: true, reloadOnChange: true);
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
