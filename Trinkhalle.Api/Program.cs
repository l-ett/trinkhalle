using Microsoft.Extensions.Hosting;
using Trinkhalle.Api;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(Startup.ConfigureServices)
    .Build();


host.Run();