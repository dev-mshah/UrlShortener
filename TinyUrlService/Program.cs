
using TinyUrlService.Data;
using TinyUrlService.Utils;
using TinyUrlService.Models;
using TinyUrlService.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Postgres>();
builder.Services.AddSingleton<Redis>();
builder.Services.AddSingleton<SqidsGenerator>();
builder.Services.AddSingleton<UrlService>();
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();



var app = builder.Build();

app.MapPost("/shorten", async (CreateUrlRequest req, UrlService urlService) =>
{
    var shortId = await urlService.CreateShortUrl(req.Url);

    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL")
                  ?? "http://localhost:5048";

    return Results.Ok(new
    {
        shortUrl = $"{baseUrl}/{shortId}"
    });
});

app.MapGet("/{shortId}", async (string shortId, Redis redis, Postgres postgres) =>
{
    var longUrl = await redis.Db.StringGetAsync(shortId);

    if (string.IsNullOrEmpty(longUrl))
    {
        using var conn = postgres.GetConnection();
        await conn.OpenAsync();

        using var cmd = new NpgsqlCommand(
            "SELECT long_url FROM urls WHERE short_id = @short", conn);

        cmd.Parameters.AddWithValue("short", shortId);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null) return Results.NotFound("URL not found");

        longUrl = result.ToString();

        await redis.Db.StringSetAsync(shortId, longUrl);
    }

    return Results.Redirect(longUrl!);
});

app.MapRazorPages();

app.Run();