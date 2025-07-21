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

    Log.Information("Starting meter add...");

    Log.Information("Voltage: {Voltage}V", 230.1);
    Log.Information("Energy: {Energy}kWh", 12340.4);

    Log.CloseAndFlush();
  }
}