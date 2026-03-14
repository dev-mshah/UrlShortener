
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

var app = builder.Build();

app.MapGet("/", () => "TinyURL service running");

app.MapPost("/shorten", async (CreateUrlRequest req, UrlService urlService) =>
{
    var shortId = await urlService.CreateShortUrl(req.Url);

    return Results.Ok(new
    {
        shortUrl = $"http://localhost:5048/{shortId}"
    });
});

app.MapGet("/{shortId}", async (string shortId, Redis redis, Postgres postgres) =>
{
    // 1. Try to get it from Redis first (Cache)
    var longUrl = await redis.Db.StringGetAsync(shortId);

    if (string.IsNullOrEmpty(longUrl))
    {
        // 2. Fallback to Postgres if it's not in Redis
        using var conn = postgres.GetConnection();
        await conn.OpenAsync();

        using var cmd = new NpgsqlCommand("SELECT long_url FROM urls WHERE short_id = @short", conn);
        cmd.Parameters.AddWithValue("short", shortId);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null) return Results.NotFound("URL not found");

        longUrl = result.ToString();

        // 3. Optional: Backfill Redis for next time
        await redis.Db.StringSetAsync(shortId, longUrl);
    }

    return Results.Redirect(longUrl!);
});


app.Run();
