
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

    // Main Shortening Algorithm
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

    Console.WriteLine($"Received request for shortId: {shortId}");
    // Use Redis Cache To Get Long URL to reduce Latency  
    var longUrl = await redis.Db.StringGetAsync(shortId);

    // Cache Miss - Check Database Now
    if (string.IsNullOrEmpty(longUrl))
    {
        // Make a new connection to postgres  
        using var conn = postgres.GetConnection();
        await conn.OpenAsync();

        // Grab the Long URL from Database using Short URL
        using var cmd = new NpgsqlCommand(
            "SELECT long_url FROM urls WHERE short_id = @short", conn);

        cmd.Parameters.AddWithValue("short", shortId);

        var result = await cmd.ExecuteScalarAsync();

        // If no results showed up -- Error Handling
        if (result == null) return Results.NotFound("URL not found");

        longUrl = result.ToString();

        // Update Redis to have this URL for future quick retrieval 
        await redis.Db.StringSetAsync(shortId, longUrl);
    }

    return Results.Redirect(longUrl!);
});

app.MapRazorPages();

app.Run();