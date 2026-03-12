using StackExchange.Redis;

namespace TinyUrlService.Data;

public class Redis
{
    public IDatabase Db { get; }

    public Redis(IConfiguration config)
    {
        var connection = ConnectionMultiplexer.Connect(
            config.GetConnectionString("Redis")!
        );

        Db = connection.GetDatabase();
    }
}