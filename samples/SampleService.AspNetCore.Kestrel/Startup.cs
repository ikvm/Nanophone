﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nanophone.AspNetCore.ApplicationServices;
using Nanophone.Core;
using Nanophone.RegistryHost.ConsulRegistry;
using Nanophone.RegistryTenant.WebApi;
using NLog.Extensions.Logging;

namespace SampleService.AspNetCore.Kestrel
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddNanophone(() => new ConsulRegistryHost());
            services.AddMvc();
            services.AddOptions();
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddNLog();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            var serviceRegistry = app.ApplicationServices.GetService<ServiceRegistry>();
            var uri = new Uri($"http://localhost:{Program.PORT}/");

            var registryInformation = serviceRegistry.AddTenant(new WebApiRegistryTenant(uri),
                "values", "1.7.0-pre", tags: new[] { "urlprefix-/values" })
                .Result;

            string checkId = serviceRegistry.AddHealthCheck(registryInformation.Name, registryInformation.Id,
                new Uri(uri, "metrics"), TimeSpan.FromSeconds(15), "metrics")
                .Result;

            app.ApplicationServices.GetService<IOptions<HealthCheckOptions>>().Value.HealthCheckId = checkId;
        }
    }
}
