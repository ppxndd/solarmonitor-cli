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

  public void LogMeterReading(float[] meterArrayValues, string meterName, byte slaveId)
  {
    if (meterArrayValues.Length >= 15)
    {
      Log.Information(
        "Meter: {MeterName} (ID: {SlaveId}) Readings:\n" +
        "Voltage 1: {V1}V, Voltage 2: {V2}V, Voltage 3: {V3}V\n" +
        "Current 1: {C1}A, Current 2: {C2}A, Current 3: {C3}A\n" +
        "Watt 1: {W1}W, Watt 2: {W2}W, Watt 3: {W3}W\n" +
        "VA 1: {VA1}, VA 2: {VA2}, VA 3: {VA3}\n" +
        "VAR 1: {VAR1}, VAR 2: {VAR2}, VAR 3: {VAR3}\n",
        meterName, slaveId,
        meterArrayValues[0], meterArrayValues[1], meterArrayValues[2],
        meterArrayValues[3], meterArrayValues[4], meterArrayValues[5],
        meterArrayValues[6], meterArrayValues[7], meterArrayValues[8],
        meterArrayValues[9], meterArrayValues[10], meterArrayValues[11],
        meterArrayValues[12], meterArrayValues[13], meterArrayValues[14]
      );
    }
    else
    {
      Log.Warning("Incomplete meter data for {MeterName} (ID: {SlaveId})", meterName, slaveId);
      return;
    }
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