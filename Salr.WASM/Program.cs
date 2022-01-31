using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazor.Extensions;
using Salr.WebCommon;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Salr.Abstractions.Contracts;
using Salr.Abstractions.Services;
using Salr.UI;
using Salr.UI.Services;
using Salr.WASM.Services;

namespace Salr.WASM
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddUIServices();
            builder.Services.AddSingleton<ICounterState, CounterState>();
            builder.Services.AddSingleton<ILocalContentFetcher, HttpClientLocalContentFetcher>();
            builder.Services.AddSingleton<IConfigProvider, JsInteropConfigProvider>();
            builder.Services.AddSingleton<ISecureConfigProvider, PasswordEncryptedJsInteropSecureConfigProvider>();
            builder.Services.AddScoped<INotificationManager, WebNotificationManager>();
            builder.Services.AddMudServices();

            builder.Services.AddSingleton(
                sp => new HttpClient {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)});

            builder.Services.AddNotifications();
            await builder.Build().RunAsync();
        }
    }
}