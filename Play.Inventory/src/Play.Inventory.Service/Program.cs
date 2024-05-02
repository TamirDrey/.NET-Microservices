using Play.Common.MongDB;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

Random jitterer = new Random();

builder.Services.AddMongo().AddMongoRepository<InventoryItem>("Inventoryitems");

builder.Services.AddHttpClient<CatalogClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5186");
})
.AddTransientHttpErrorPolicy(builderB => builderB.Or<TimeoutRejectedException>().WaitAndRetryAsync(
    5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
    + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
    onRetry: (outcome, timespan, retryAttempt) =>
    {
        var serviceProvider = builder.Services.BuildServiceProvider();
        serviceProvider.GetService<ILogger<CatalogClient>>().LogWarning($"Dalaying for {timespan.TotalSeconds} seconds, then marking retry {retryAttempt} ");
    }
))
.AddTransientHttpErrorPolicy(builderB => builderB.Or<TimeoutRejectedException>().CircuitBreakerAsync(3, TimeSpan.FromSeconds(15),
    onBreak: (outcome, timespan) =>
    {
        var serviceProvider = builder.Services.BuildServiceProvider();
        serviceProvider.GetService<ILogger<CatalogClient>>().LogWarning($"Opening the circuit for {timespan.TotalSeconds} seconds.....");
    },
    onReset: () =>
    {
        var serviceProvider = builder.Services.BuildServiceProvider();
        serviceProvider.GetService<ILogger<CatalogClient>>().LogWarning("Closing the circuit....");
    }
))
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapControllers();
app.UseHttpsRedirection();

app.Run();
