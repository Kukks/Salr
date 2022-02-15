using System;
using System.Net.Http;
using System.Threading.Tasks;
using Salr.WebCommon;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MudBlazor.Services;
using NNostr.Client;
using Salr.Abstractions.Contracts;
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
            builder.RootComponents.Add<WasmAES>("wasmaes");
            builder.RootComponents.Add<HostedServices>("hostedservices");

            builder.Services.AddUIServices();
            builder.Services.AddSingleton<ILocalContentFetcher, HttpClientLocalContentFetcher>();
            builder.Services.AddSingleton<IConfigProvider, JsInteropConfigProvider>();
            builder.Services.AddSingleton<ISecureConfigProvider, PasswordEncryptedJsInteropSecureConfigProvider>();
            builder.Services.AddMudServices();
            builder.Services.AddSingleton(
                sp => new HttpClient {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)});

            await builder.Build().RunAsync();
        }
    }
}