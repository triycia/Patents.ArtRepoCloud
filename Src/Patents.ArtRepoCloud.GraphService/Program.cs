using Autofac.Extensions.DependencyInjection;
using Patents.ArtRepoCloud.GraphService;
using Patents.ArtRepoCloud.GraphService.Extensions;
using HealthChecks.UI.Client;
using HotChocolate.AspNetCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.AddBasics();
builder.Services.AddAuthorization();
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseAuthorization();
app.UseGraphQLMultipartForm();
app.UseEndpoints(endpoints =>
{
    endpoints.MapGraphQL("/")
        .WithOptions(new GraphQLServerOptions
        {
            Tool = { Enable = true },
            EnableMultipartRequests = true
        });

    //The readiness check uses all registered checks.
    endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    endpoints.MapHealthChecks("/liveness", new HealthCheckOptions()
    {
        Predicate = r => r.Name.Contains("self")
    });
});

app.Run();