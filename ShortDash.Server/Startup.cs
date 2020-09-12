using Blazored.Modal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ApplicationDbContext dbContext)
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapHub<TargetsHub>("/targetshub");
                endpoints.MapFallbackToPage("/_Host");
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
            });
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSignalR();
            services.AddBlazoredModal();
            services.AddScoped<DashboardService>();
            services.AddScoped<DashboardActionService>();
            services.AddSingleton<FormGeneratorPropertyMapper>();
            services.AddSingleton<PluginService>();
            services.AddTransient(typeof(IShortDashPluginLogger<>), typeof(ShortDashPluginLogger<>));
            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
        }
    }
}