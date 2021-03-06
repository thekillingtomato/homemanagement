using HomeManagement.API.WebApp.Data;
using HomeManagement.API.WebApp.Services;
using HomeManagement.API.WebApp.Services.Notification;
using HomeManagement.API.WebApp.Services.Rest;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HomeManagement.API.WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddLogging();
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddProtectedBrowserStorage();
            services.AddSingleton<WeatherForecastService>();
            services.AddSingleton<AuthenticationService>();
            services.AddSingleton<NotifierService>();
            services.AddSingleton<NewTransactionAddedNotification>();
            services.AddScoped<RestClient>();
            services.AddScoped<StorageRestClient>();
            
            services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
        }

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
    }
}
