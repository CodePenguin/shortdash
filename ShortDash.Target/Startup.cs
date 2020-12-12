using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShortDash.Core.Data;
using ShortDash.Core.Extensions;
using ShortDash.Core.Interfaces;
using ShortDash.Core.Plugins;
using ShortDash.Core.Services;
using ShortDash.Target.Data;
using ShortDash.Target.Services;
using ShortDash.Target.Shared;
using System.IO;

namespace ShortDash.Target
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDatabaseDeveloperPageExceptionFilter();

            var settings = new ApplicationSettings();
            Configuration.GetSection(ApplicationSettings.Key).Bind(settings);

            var configuredApplicationDataPath = !string.IsNullOrEmpty(settings.ApplicationDataPath) ? settings.ApplicationDataPath : null;
            var applicationDataPath = configuredApplicationDataPath ?? EnvironmentExtensions.GetLocalApplicationDataFolderPath("ShortDash.Target");
            if (!Directory.Exists(applicationDataPath))
            {
                throw new DirectoryNotFoundException("The ApplicationDataPath directory does not exist: " + applicationDataPath);
            }

            services.AddDataProtection()
                .SetApplicationName("ShortDash.Target");

            services.AddDbContext<TargetApplicationDbContext>(options =>
            {
                options.UseSqlite("Data Source=" + Path.Combine(applicationDataPath, "ShortDash.Target.db"));
            });
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddScoped<IApplicationDbContextFactory, TargetApplicationDbContextFactory>();
            services.AddScoped<TargetApplicationDbContextFactory>();
            services.AddTransient<IConfigurationService, ConfigurationService>();
            services.AddTransient(typeof(IDataProtectionService), typeof(DataProtectionService));
            services.AddTransient(typeof(IShortDashPluginLogger<>), typeof(ShortDashTargetPluginLogger<>));
            services.AddTransient(typeof(IKeyStoreService), typeof(ConfigurationKeyStoreService));
            services.AddSingleton(typeof(IEncryptedChannelService), typeof(TargetEncryptedChannelService));
            services.AddTransient(typeof(ISecureKeyStoreService), typeof(SecureKeyStoreService));
            services.AddSingleton<PluginService>();
            services.AddSingleton<ActionService>();
            services.AddTransient<IRetryPolicy, TargetHubRetryPolicy>();
            services.AddSingleton<TargetHubClient>();
            services.AddHostedService<TargetHubHostService>();
        }
    }
}
