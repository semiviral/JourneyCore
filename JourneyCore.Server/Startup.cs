using JourneyCore.Server.Net.SignalR.Contexts;
using JourneyCore.Server.Net.SignalR.Hubs;
using JourneyCore.Server.Net.SignalR.Proxies;
using JourneyCore.Server.Net.SignalR.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;

namespace JourneyCore.Transmission
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = httpContext => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSignalR().AddJsonProtocol(options =>
            {
                options.PayloadSerializerSettings.ContractResolver = new DefaultContractResolver();
            });

            services.AddSingleton<GameClientContext>();
            services.AddSingleton<IGameClientContext>(provider => provider.GetRequiredService<GameClientContext>());

            services.AddSingleton<GameService>();
            services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<GameService>());
            services.AddSingleton<IGameService>(provider => provider.GetRequiredService<GameService>());

            services.AddSingleton<GameProxy>();
            services.AddSingleton<IGameProxy>(provider => provider.GetRequiredService<GameProxy>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseExceptionHandler("/Error");
            }

            app.UseCookiePolicy();
            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseSignalR(routes => routes.MapHub<GameClientHub>("/GameClient"));
        }
    }
}
