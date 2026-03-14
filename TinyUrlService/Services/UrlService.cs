using Npgsql;
using TinyUrlService.Data;
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
        var id = await _redis.Db.StringIncrementAsync("url_counter");

        var shortId = _sqids.Encode((long)id);

        using var conn = _postgres.GetConnection();
        await conn.OpenAsync();

        var cmd = new NpgsqlCommand(
            "INSERT INTO urls (id, short_id, long_url) VALUES (@id,@short,@long)",
            conn
        );

        cmd.Parameters.AddWithValue("id", (long)id);
        cmd.Parameters.AddWithValue("short", shortId);
        cmd.Parameters.AddWithValue("long", longUrl);

        await cmd.ExecuteNonQueryAsync();

        await _redis.Db.StringSetAsync(shortId, longUrl);

        return shortId;
    }
}