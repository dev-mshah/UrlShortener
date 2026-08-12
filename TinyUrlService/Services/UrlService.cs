using Npgsql;
using TinyUrlService.Data;
using TinyUrlService.Models;
using TinyUrlService.Utils;

namespace TinyUrlService.Services;

public class UrlService
{
    private readonly Postgres _postgres;
    private readonly Redis _redis;
    private readonly SqidsGenerator _sqids;

    public UrlService(Postgres postgres, Redis redis, SqidsGenerator sqids)
    {
        _postgres = postgres;
        _redis = redis;
        _sqids = sqids;
    }

    public async Task<string> CreateShortUrl(string longUrl)
    {
        // For each URL we get, we will keep a counter and increment each time
        // Each counter value is unique, eliminating the risk of collisions without the need for additional checks.
        var id = await _redis.Db.StringIncrementAsync("url_counter");


        // Use SQIDS to encode global counter 
        var shortId = _sqids.Encode((long)id);

        // Connet to Postgres to save new short ID
        using var conn = _postgres.GetConnection();
        await conn.OpenAsync();

        // Add the new URL with both short & long url columns for easy retrieval
        var cmd = new NpgsqlCommand(
            "INSERT INTO urls (id, short_id, long_url) VALUES (@id,@short,@long)",
            conn
        );

        cmd.Parameters.AddWithValue("id", (long)id);
        cmd.Parameters.AddWithValue("short", shortId);
        cmd.Parameters.AddWithValue("long", longUrl);

        await cmd.ExecuteNonQueryAsync();


        // Add this new URL into the Cache for Low Latency Retrieval
        await _redis.Db.StringSetAsync(shortId, longUrl);

        return shortId;
    }
}
