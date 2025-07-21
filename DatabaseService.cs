using System;
using Npgsql;

class DatabaseService : IDisposable
{
  private NpgsqlConnection? conn;

  public DatabaseService()
  {
    NpgsqlConnectionStringBuilder builder = new Npgsql.NpgsqlConnectionStringBuilder()
    {
      Host = "localhost",
      Port = 5432,
      Username = "postgres",
      Password = "mysecretpassword",
      Database = "mydatabase",
    };
    string connectionString = builder.ConnectionString;
    conn = new NpgsqlConnection(connectionString);

    try
    {
      conn.Open();
      Console.WriteLine("Connected to PostgresSQL!");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error Database connection : {ex.Message}");
    }

  }

  public void CreateOneDataMeter(int id, float[] payload)
  {
    
  }
  public void Dispose()
  {
    if (conn == null) return;
    conn.Close();
  }
}