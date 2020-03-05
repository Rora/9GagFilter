using AspNetCore.Proxy.Builders;
using AspNetCore.Proxy.Extensions;
using AspNetCore.Proxy.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NineGagFilter.Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
            services.AddProxies();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBlazorDebugging();
            }

            app.UseStaticFiles();
            app.UseClientSideBlazorFiles<Client.Program>();

            app.UseRouting();

            var proxyCatchAllRoute = "NineGagCorsProxy/api";
            app.UseProxies(proxiesBuilder =>
            {
                proxiesBuilder.Map(proxyCatchAllRoute + "/{**_}", ConfigurePorxyFor(proxyCatchAllRoute, "https://api.9gag.com"));
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapFallbackToClientSideBlazor<Client.Program>("index.html");
            });
        }

        private Action<IProxyBuilder> ConfigurePorxyFor(string proxyCatchAllRoute, string endpoint)
        {
            return proxyBuilder =>
            {
                proxyBuilder.UseHttp((c, a) =>
                {
                    return endpoint.Trim('/') + c.Request.Path.Value.Substring(proxyCatchAllRoute.Length + 1, c.Request.Path.Value.Length - proxyCatchAllRoute.Length - 1);
                }, ConfigureCorsProxy);
            };
        }

        private void ConfigureCorsProxy(IHttpProxyOptionsBuilder proxyOptionsBuilder)
        {
            proxyOptionsBuilder
                .WithShouldAddForwardedHeaders(false)
                .WithBeforeSend((c, m) =>
                {
                    m.Headers.Remove("Origin");
                    return Task.CompletedTask;
                });
        }
    }
}
