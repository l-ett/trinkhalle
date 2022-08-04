// See https://aka.ms/new-console-template for more information

using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Plugins.Http.CSharp;
using Newtonsoft.Json;
using Trinkhalle.DrinkManagement.Features;

var httpFactory = HttpClientFactory.Create();

var buyDrink = Step.Create("buy_drink",
    clientFactory: httpFactory,
    timeout:TimeSpan.FromSeconds(30),
    execute: async context =>
    {
        var url = $"https://api-trinkhalle-we.azure-api.net/DrinkManagement/PurchaseDrink";
        var purchaseDrink = new CreateDrinkPurchaseCommand()
        {
            BeverageId = Guid.Parse("817c8151-b863-47b4-9d62-fb0202279f89"),
            UserId = Guid.NewGuid()
        };
        var request = Http.CreateRequest("POST", url)
            .WithHeader("Ocp-Apim-Subscription-Key", "")
            .WithHeader("Content-Type", "application/json")
            .WithBody(new StringContent(JsonConvert.SerializeObject(purchaseDrink)));

        var response = await Http.Send(request, context);
        return response;
    });

var scenario = ScenarioBuilder
    .CreateScenario("rest_api", buyDrink)
    .WithWarmUpDuration(TimeSpan.FromSeconds(15))
    .WithLoadSimulations(LoadSimulation.NewKeepConstant(50, TimeSpan.FromMinutes(1)));

NBomberRunner
    .RegisterScenarios(scenario)
    .Run();