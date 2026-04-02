using StackExchange.Redis;

namespace TinyUrlService.Data;

public class Redis
{
    public IDatabase Db { get; }

    public Redis(IConfiguration config)
    {
        var redisUrl = Environment.GetEnvironmentVariable("Redis") ?? config.GetConnectionString("Redis");
        ;

        var options = ConfigurationOptions.Parse(redisUrl);
        options.Ssl = false;                 // Upstash requires TLS
        options.AbortOnConnectFail = false; // don't crash on startup

        var connection = ConnectionMultiplexer.Connect(options);

        Db = connection.GetDatabase();
    }
}