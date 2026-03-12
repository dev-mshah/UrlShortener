
using TinyUrlService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Postgres>();
builder.Services.AddSingleton<Redis>();

var app = builder.Build();

app.MapGet("/", () => "TinyURL service running");


app.Run();
