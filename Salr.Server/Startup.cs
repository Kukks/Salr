using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Salr.UI.Services;
using Salr.WebCommon;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MudBlazor.Services;
using NNostr.UI;
using Salr.Abstractions.Contracts;
using Salr.Abstractions.Services;
using Salr.Server.Services;

namespace Salr.Server
{
    public class HostedServiceWrapper : IHostedService
    {
        private readonly IEnumerable<ISimilarHostedService> _similarHostedServices;

        public HostedServiceWrapper(IEnumerable<ISimilarHostedService> similarHostedServices)
        {
            _similarHostedServices = similarHostedServices;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(_similarHostedServices.Select(service => service.StartAsync(cancellationToken)));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(_similarHostedServices.Select(service => service.StopAsync(cancellationToken)));
        }
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddUIServices();
            services.AddSingleton<ILocalContentFetcher, FileProviderLocalContentFetcher>();
            services.AddScoped<IConfigProvider, JsInteropConfigProvider>();
            services.AddScoped<ISecureConfigProvider, JsInteropSecureConfigProvider>();
            services.AddSingleton(provider =>
                provider.GetRequiredService<IWebHostEnvironment>().WebRootFileProvider);
            services.AddMudServices();

            services.AddDataProtection();
            services.AddHostedService<HostedServiceWrapper>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
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