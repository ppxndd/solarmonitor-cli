using System;

class SensorDatabase : IMeterRepository
{
  private DatabaseService _dbService;

  public SensorDatabase(DatabaseService dbSvc) {
    _dbService = dbSvc;
  }

  public void CreateOneDataMeter(float[] payload, byte slaveId, DateTime timestamp)
  {
    string query = @"
      INSERT INTO environment_data (
        created_at, temperature, humidity, pyranometer
      )
      VALUES (
        @created_at, @temperature, @humidity, @pyranometer
      )
    ";
    _dbService.ExecuteNonQuery(query, cmd =>
    {
      cmd.Parameters.AddWithValue("created_at", timestamp);
      cmd.Parameters.AddWithValue("temperature", payload[0]);
      cmd.Parameters.AddWithValue("humidity", payload[1]);
      cmd.Parameters.AddWithValue("pyranometer", payload[2]);
    });
  }
}