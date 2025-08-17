using System;

class MeterDatabase: IMeterRepository
{
  private DatabaseService _dbService;
  public MeterDatabase(DatabaseService dbSvc) 
  {
    _dbService = dbSvc;
  }
  public void CreateOneDataMeter(float[] payload, byte slaveId, DateTime timestamp)
  {
    string query = @"
        INSERT INTO meter_measurements (
          meter_id, measurement_time, volts_avg, current_sum, watt_sum,
          voltage_1, voltage_2, voltage_3, current_1, current_2, current_3,
          va_1, va_2, va_3, var_1, var_2, var_3, pf_1, pf_2, pf_3,
          energy_im, energy_ex, freq, created_at
        )
        VALUES (
          @meter_id, @measurement_time, @volts_avg, @current_sum, @watt_sum,
          @volt1, @volt2, @volt3, @current1, @current2, @current3,
          @va1, @va2, @va3, @var1, @var2, @var3, @pf1, @pf2, @pf3,
          @energyIm, @energyEx, @freq, @created_at
        )
    ";
    _dbService.ExecuteNonQuery(query, cmd =>
    {
      cmd.Parameters.AddWithValue("meter_id", slaveId);
      cmd.Parameters.AddWithValue("measurement_time", timestamp);
      cmd.Parameters.AddWithValue("volts_avg", payload[21]);
      cmd.Parameters.AddWithValue("current_sum", payload[24]);
      cmd.Parameters.AddWithValue("watt_sum", payload[26]);
      cmd.Parameters.AddWithValue("volt1", payload[0]);
      cmd.Parameters.AddWithValue("volt2", payload[1]);
      cmd.Parameters.AddWithValue("volt3", payload[2]);
      cmd.Parameters.AddWithValue("current1", payload[3]);
      cmd.Parameters.AddWithValue("current2", payload[4]);
      cmd.Parameters.AddWithValue("current3", payload[5]);
      cmd.Parameters.AddWithValue("va1", payload[6]);
      cmd.Parameters.AddWithValue("va2", payload[7]);
      cmd.Parameters.AddWithValue("va3", payload[8]);
      cmd.Parameters.AddWithValue("var1", payload[9]);
      cmd.Parameters.AddWithValue("var2", payload[10]);
      cmd.Parameters.AddWithValue("var3", payload[11]);
      cmd.Parameters.AddWithValue("pf1", payload[12]);
      cmd.Parameters.AddWithValue("pf2", payload[13]);
      cmd.Parameters.AddWithValue("pf3", payload[14]);
      cmd.Parameters.AddWithValue("energyIm", payload[36]);
      cmd.Parameters.AddWithValue("energyEx", payload[37]);
      cmd.Parameters.AddWithValue("freq", payload[35]);
      cmd.Parameters.AddWithValue("created_at", timestamp);
    });

  }
}