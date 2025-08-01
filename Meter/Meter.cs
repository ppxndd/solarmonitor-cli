using System;
using System.IO.Ports;
using EasyModbus;
using Serilog;

class Meter
{
  private string name;
  private byte slaveId;
  private ModbusService modbusSvc;
  private LoggingService logSvc;
  private CancellationTokenSource cts;
  private DatabaseService dbSvc;
  public string Name
  {
    get { return name; }
  }
  public int SlaveId
  {
    get { return slaveId; }
  }

  public Meter(string name, byte slaveId, ModbusService modbusService, LoggingService logSvc, DatabaseService dbSvc)
  {
    this.name = name;
    this.slaveId = slaveId;
    this.modbusSvc = modbusService;
    this.logSvc = logSvc;
    this.dbSvc = dbSvc;
  }

  public void StartBackgroundReading(TimeSpan interval)
  {
    cts = new CancellationTokenSource();
    Task.Run(() => ReadLoop(interval, cts.Token));
  }

  public void StopBackgroundReading()
  {
    logSvc.CloseLogger();
    cts?.Cancel();
  }

  private async Task ReadLoop(TimeSpan interval, CancellationToken token)
  {
    while (!token.IsCancellationRequested)
    {
      try
      {
        this.ReadValueFromMeter(41);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error reading Modbus data: {ex.Message}");
      }

      await Task.Delay(interval, token);
    }
  }
  public void ReadValueFromMeter(int amountValue)
  {
    int[] valueFromModbus = this.modbusSvc.ReadPairInput(this.slaveId, 0, amountValue);
    var formatedValues = new List<float>();

    for (int i = 0; i < valueFromModbus.Length / 2; i++)
    {
      float formatedValue = this.FormatValueDCBA([valueFromModbus[2 * i], valueFromModbus[(2 * i) + 1]]);
      formatedValues.Add(formatedValue);
    }
    if (formatedValues.ToArray().Length > 1)
    {
      this.dbSvc.CreateOneDataMeter(this.slaveId, formatedValues.ToArray(), this.name, this.slaveId);
      this.logSvc.LogMeterReading(formatedValues.ToArray(), this.name, this.slaveId);
    }
  }

  private float FormatValueDCBA(int[] dataFromReg)
  {

    ushort highBit = (ushort)dataFromReg[0];
    ushort lowBit = (ushort)dataFromReg[1];

    byte[] formattedValue = {
      (byte)(lowBit & 0xFF), (byte)(lowBit >> 8),
      (byte)(highBit & 0xFF), (byte)(highBit >> 8)
    };

    float returnValue = BitConverter.ToSingle(formattedValue, 0);
    return returnValue;
  }
}