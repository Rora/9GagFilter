using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Blazored.LocalStorage;
using System.Net.Http;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using NineGagFilter.Client.State;

namespace NineGagFilter.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            builder.Services.AddBlazoredLocalStorage();
            builder.Services
                .AddBlazorise(options =>
                {
                    options.ChangeTextOnKeyPress = true;
                })
                .AddBootstrapProviders()
                .AddFontAwesomeIcons();

            builder.Services.AddSingleton<AuthState>();
            builder.Services.AddSingleton<My9GAG.NineGagApiClient.IApiClient, My9GAG.NineGagApiClient.ApiClient>(sp => new My9GAG.NineGagApiClient.ApiClient(sp.GetService<HttpClient>(), o =>
            {
                var localhost = "/";
                o.ApiUrl = localhost + "NineGagCorsProxy/api";
            }));

            var host = builder.Build();
            host.Services
              .UseBootstrapProviders()
              .UseFontAwesomeIcons();
            
            await host.RunAsync();
        }
    }
}
