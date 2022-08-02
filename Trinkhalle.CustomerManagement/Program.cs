using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Trinkhalle.CustomerManagement;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(builder => { builder.AddUserSecrets(Assembly.GetExecutingAssembly()); })
    .ConfigureServices(Startup.ConfigureServices)
    .Build();

host.Run();