using Play.Catalog.Service.Entities;
using Play.Common.MongDB;
using Play.Common.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

builder.Services.AddMongo().AddMongoRepository<Item>("items");

var app = builder.Build();

app.MapControllers();
app.UseHttpsRedirection();

app.Run();

