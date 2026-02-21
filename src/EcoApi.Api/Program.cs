using EcoApi.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);
builder.Host.AddApiSerilog();

var app = builder.Build();

await app.ApplyDatabaseMigrationsAsync();
app.UseApiPipeline();

await app.RunAsync();

public partial class Program { }
