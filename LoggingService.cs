using Serilog;

class LoggingService
{
  public LoggingService()
  {
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel
        .Information()
        .WriteTo.File("logs/meter.log", rollingInterval: RollingInterval.Day)
        .CreateLogger();
  }

  public LoggingService(string type) {
    if (type == "environment")
    {
      Log.Logger = new LoggerConfiguration()
        .MinimumLevel
        .Information()
        .WriteTo.File("logs/environment/environment.log", rollingInterval: RollingInterval.Day)
        .CreateLogger();
    }
  }

  public void LogMeterReading(float[] meterArrayValues, string meterName, byte slaveId)
  {
    if (meterArrayValues.Length >= 15)
    {
      Log.Information(
        "Meter {MeterName} (ID: {SlaveId}) → V1:{V1}V, C1:{C1}A, W1:{W1}W, VA1:{VA1}, VAR1:{VAR1}",
        meterName, slaveId,
        meterArrayValues[0],  // V1
        meterArrayValues[3],  // C1
        meterArrayValues[6],  // W1
        meterArrayValues[9],  // VA1
        meterArrayValues[12]  // VAR1
      );
    }
    else
    {
      Log.Warning("Incomplete meter data for {MeterName} (ID: {SlaveId})", meterName, slaveId);
      return;
    }
  }

  public void LogEnvironment(float[] payload)
  {
    Log.Information(
      "{temperature} C, Humidity: {humidity}, Pyranometer: {pyranometer}",
       payload[0],
       payload[1],
       payload[2]
    );
  }

  public void LogDatabase(string status, string message)
  {
    Log.Information(status + " : " + message);
  }

  public void LogWarning(string message)
  {
    Log.Warning(message);
  }

  public void LogError(string message, Exception ex = null)
  {
    if (ex != null) Log.Error(ex, message);
    else Log.Error(message);
  }

  public void CloseLogger()
  {
    Log.CloseAndFlush();
  }
}