using Npgsql;

namespace TinyUrlService.Data;

public class Postgres
{
    private readonly string _connectionString;

    public Postgres(IConfiguration config)
    {
        // Try environment variable first (recommended for Render)
        _connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")
                            ?? config.GetConnectionString("Postgres");

        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new Exception("Postgres connection string is missing. Set POSTGRES_CONNECTION or add it to appsettings.json.");
        }
    }

    public NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}