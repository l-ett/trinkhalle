using Microsoft.Extensions.Hosting;
using Trinkhalle.CustomerManagement;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(Startup.ConfigureServices)
    .Build();


host.Run();