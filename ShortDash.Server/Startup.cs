using Blazored.Modal;
using Blazored.Toast;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShortDash.Core.Interfaces;
using ShortDash.Core.Plugins;
using ShortDash.Core.Services;
using ShortDash.Server.Components;
using ShortDash.Server.Data;
using ShortDash.Server.Services;
using System.Linq;

namespace ShortDash.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env, ApplicationDbContext dbContext)
        {
            dbContext.Database.Migrate();

            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapHub<TargetsHub>(TargetsHub.HubUrl);
                endpoints.MapFallbackToPage("/_Host");
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection()
                .SetApplicationName("ShortDash.Server");

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
            });
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.EventsType = typeof(AuthenticationEvents);
                });

            services.AddAuthorization(config =>
            {
                // Administrator Actions
                config.AddPolicy(Policies.EditActions, Policies.IsAdminPolicy());
                config.AddPolicy(Policies.EditDashboards, Policies.IsAdminPolicy());
                config.AddPolicy(Policies.EditTargets, Policies.IsAdminPolicy());
                config.AddPolicy(Policies.EditDevices, Policies.IsAdminPolicy());
                // Specific Actions
                config.AddPolicy(Policies.ViewDashboards, Policies.IsUserPolicy());
            });

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSignalR();
            services.AddBlazoredModal();
            services.AddBlazoredToast();
            services.AddHttpContextAccessor();
            services.AddScoped<ApplicationDbContextFactory>();
            services.AddScoped<AdminAccessCodeService>();
            services.AddScoped<AuthenticationEvents>();
            services.AddScoped<ConfigurationService>();
            services.AddScoped<DashboardService>();
            services.AddScoped<DashboardActionService>();
            services.AddScoped<DataSignatureManager>();
            services.AddScoped<DeviceLinkService>();
            services.AddScoped<FormGeneratorPropertyMapper>();
            services.AddScoped<NavMenuManager>();
            services.AddScoped<TargetLinkService>();
            services.AddSingleton(typeof(IEncryptedChannelService), typeof(ServerEncryptedChannelService));
            services.AddSingleton<PluginService>();
            services.AddTransient(typeof(IDataProtectionService), typeof(DataProtectionService));
            services.AddTransient(typeof(IKeyStoreService), typeof(ConfigurationKeyStoreService));
            services.AddTransient(typeof(IShortDashPluginLogger<>), typeof(ShortDashPluginLogger<>));
            services.AddTransient(typeof(ISecureKeyStoreService), typeof(SecureKeyStoreService));
            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
            });
        }
    }
}