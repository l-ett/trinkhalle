using Microsoft.Extensions.Hosting;
using Trinkhalle.DrinkManagement;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(Startup.ConfigureServices)
    .Build();


host.Run();