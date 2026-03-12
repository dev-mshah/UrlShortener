
using TinyUrlService.Data;
using TinyUrlService.Utils;
using TinyUrlService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Postgres>();
builder.Services.AddSingleton<Redis>();
builder.Services.AddSingleton<SqidsGenerator>();
builder.Services.AddSingleton<UrlService>();

var app = builder.Build();

app.MapGet("/", () => "TinyURL service running");


app.Run();
