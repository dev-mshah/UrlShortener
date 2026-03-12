using Npgsql;

namespace TinyUrlService.Data;

public class Postgres
{
    private readonly string _connectionString;

    public Postgres(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Postgres")!;
    }

    public NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}