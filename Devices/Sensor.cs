using System;
using System.IO.Ports;
using EasyModbus;

class Sensor : IDevice
{
  private byte slaveId;
  private ModbusService modbusSvc;
  private LoggingService logSvc;
  private CancellationTokenSource cts;
  
  public void StartBackgroundReading(TimeSpan interval)
  {

  }

  public void StopBackgroundReading()
  {
    
  }
}