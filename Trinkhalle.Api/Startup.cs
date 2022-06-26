using System.Reflection;
using MediatR;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Trinkhalle.Api.Infrastructure;

[assembly: FunctionsStartup(typeof(Trinkhalle.Api.Startup))]

namespace Trinkhalle.Api;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var configuration = builder.GetContext().Configuration;

        builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
        builder.Services.AddDbContext<TrinkhalleContext>(
            options => options.UseCosmos(
                connectionString: configuration.GetConnectionString("CosmosDb"),
                databaseName: "Trinkhalle"));
    }
}