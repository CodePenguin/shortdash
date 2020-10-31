using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShortDash.Core.Interfaces;
using ShortDash.Core.Plugins;
using ShortDash.Core.Services;
using ShortDash.Target.Services;
using ShortDash.Target.Shared;

namespace ShortDash.Target
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
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

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection()
                .SetApplicationName("ShortDash.Target");

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddTransient(typeof(IDataProtectionService), typeof(DataProtectionService));
            services.AddTransient(typeof(IShortDashPluginLogger<>), typeof(ShortDashTargetPluginLogger<>));
            services.AddTransient(typeof(IKeyStoreService), typeof(FileKeyStoreService));
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
