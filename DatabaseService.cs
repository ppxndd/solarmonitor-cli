using System;
using Npgsql;
using dotenv.net;

class DatabaseService
{
  private NpgsqlConnection? conn;
  private LoggingService _logSvc;
  private readonly string _connectionString;

  public DatabaseService(LoggingService logSvc)
  {
    DotEnv.Load();
    var host = Environment.GetEnvironmentVariable("DB_HOST") ?? throw new InvalidOperationException("DB_HOST missing");
    var user = Environment.GetEnvironmentVariable("DB_USER") ?? throw new InvalidOperationException("DB_USER missing");
    var pass = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? throw new InvalidOperationException("DB_PASSWORD missing");
    var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? throw new InvalidOperationException("DB_NAME missing");
    var portStr = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";

    if (!int.TryParse(Environment.GetEnvironmentVariable("DB_PORT"), out var port))
    {
      throw new InvalidOperationException($"Invalid DB_PORT value: {portStr}");
    }

    NpgsqlConnectionStringBuilder builder = new Npgsql.NpgsqlConnectionStringBuilder()
    {
      Host = host,
      Port = port,
      Username = user,
      Password = pass,
      Database = dbName,
    };
    _connectionString = builder.ConnectionString;
    _logSvc = logSvc;
  }

  public NpgsqlConnection GetConnection()
  {
    var conn = new NpgsqlConnection(_connectionString);
    conn.Open();
    // _logSvc.LogDatabase("Opened Connection", "Successfully");
    return conn;
  }

  public void ExecuteNonQuery(string query, Action<NpgsqlCommand>? parameterSetter = null)
  {
    using var conn = GetConnection();
    using var cmd = new NpgsqlCommand(query, conn);

    parameterSetter?.Invoke(cmd);

    cmd.ExecuteNonQuery();
  }

  public T ExecuteScalar<T>(string query, Action<NpgsqlCommand>? parameterSetter = null)
  {
    using var conn = GetConnection();
    using var cmd = new NpgsqlCommand(query, conn);

    parameterSetter?.Invoke(cmd);

    object? result = cmd.ExecuteScalar();

    if (result == null || result is DBNull) return default!;

    return (T)Convert.ChangeType(result, typeof(T));
  }
}