
using TinyUrlService.Data;
using TinyUrlService.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Postgres>();
builder.Services.AddSingleton<Redis>();
builder.Services.AddSingleton<SqidsGenerator>();

var app = builder.Build();

app.MapGet("/", () => "TinyURL service running");


app.Run();
