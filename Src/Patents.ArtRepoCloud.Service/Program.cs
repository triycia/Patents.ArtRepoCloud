using Autofac;
using Autofac.Extensions.DependencyInjection;
using Vikcher.Framework.Azure.AppConfiguration;
using Patents.ArtRepoCloud.Service;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using NLog.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

    builder.Host.ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<Worker>();
        StartupHelpers.ConfigureServices(hostContext, services);
    });
    builder.Host.ConfigureLogging((hostContext, loggingBuilder) =>
    {
        loggingBuilder.SetMinimumLevel(LogLevel.Debug);
        loggingBuilder.AddNLog();
        if (hostContext.HostingEnvironment.IsDevelopment())
        {
            loggingBuilder.AddConsole();
        }
    });
    builder.Host.ConfigureAppConfiguration((hostContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                optional: true);
        if (hostContext.HostingEnvironment.IsDevelopment())
        {
            config.AddCommandLine(args);
        }

        var settings = config.Build();

        config.AddAzureAppConfiguration(options =>
        {
            if (hostContext.HostingEnvironment.IsDevelopment())
            {
                AzureAppConfigurationHelper.ConfigureAzureAppConfiguration(
                    settings["ConnectionStrings:AzureAppConfiguration"],
                    settings["AzureAppConfigurationLabel"],
                    settings["AzureAppConfigurationKeys"],
                    options,
                    hostContext.HostingEnvironment.IsDevelopment()
                );
            }
            else
            {
                AzureAppConfigurationHelper.ConfigureAzureAppConfigurationWithManagedIdentity(
                    settings["ConnectionStrings:AzureAppConfiguration"],
                    settings["AzureAppConfigurationLabel"],
                    settings["AzureAppConfigurationKeys"],
                    options,
                    hostContext.HostingEnvironment.IsDevelopment()
                    , true
                );
            }
        });
    });
    builder.Host.ConfigureContainer<ContainerBuilder>(StartupHelpers.ConfigureContainer);
var app = builder.Build();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    //The readiness check uses all registered checks.
    endpoints.MapHealthChecks("/hc", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
    {
        Predicate = r => r.Name.Contains("self")
    });
});
app.Run();