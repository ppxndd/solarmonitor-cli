using System;
using System.IO.Ports;
using EasyModbus;

class Sensor : IDevice
{
  private LoggingService logSvc;
  private CancellationTokenSource cts;
  ModbusService modbusHT, modbusPyranometer;
  private readonly IMeterRepository _repo;

  public Sensor(ModbusService modbusHT, ModbusService modbusPyranometer, IMeterRepository repo, LoggingService log)
  {
    this.modbusHT = modbusHT;
    this.modbusPyranometer = modbusPyranometer;
    logSvc = log;
    _repo = repo;
    Console.WriteLine("Create modbusService for sensor success.");
  }

  public void StartBackgroundReading(TimeSpan interval)
  {
    cts = new CancellationTokenSource();
    Task.Run(() => ReadLoop(interval, cts.Token));
  }

  public void StopBackgroundReading()
  {
    cts?.Cancel();
    logSvc.CloseLogger();
  }

  private async Task ReadLoop(TimeSpan interval, CancellationToken token)
  {
    while (!token.IsCancellationRequested)
    {
      try
      {
        ReadValue();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error reading sensor data: {ex.Message}");
      }

      await Task.Delay(interval, token);
    }
  }

  public void ReadValue()
  {
    int[] rawHTData = this.modbusHT.ReadOneValue(0, 2);
    int[] rawPyranometerData = this.modbusPyranometer.ReadOneValue(0, 2);

    var formatedValues = new List<float>();

    float realTemp = rawHTData[0] / 10;
    float realHumidity = rawHTData[1] / 10;
    float realPyrano = convertHT([rawPyranometerData[0], rawPyranometerData[1]]) * 2;
    formatedValues.Add(realTemp);
    formatedValues.Add(realHumidity);
    formatedValues.Add(realPyrano);
    if (formatedValues.ToArray().Length > 1)
    {
      SaveAndLog(formatedValues.ToArray());
    }
  }

  private void SaveAndLog(float[] payload)
  {
    var currentTime = DateTime.Now;
    _repo.CreateOneDataMeter(payload, 0, currentTime);
    logSvc.LogEnvironment(payload);
  }

  public float convertHT(int[] input_registers_)
  {
    string strHighByte = input_registers_[0].ToString("X2");
    string strLowByte = input_registers_[1].ToString("X2");

    //Console.WriteLine("strHighByte = {0}", strHighByte);
    //Console.WriteLine("strLowByte = {0}", strLowByte); 


    strHighByte = "0000" + strHighByte;
    strHighByte = strHighByte.Substring(strHighByte.Length - 4, 4);

    strLowByte = "0000" + strLowByte;
    strLowByte = strLowByte.Substring(strLowByte.Length - 4, 4);

    //Console.WriteLine("strHighByte = {0}", strHighByte);
    //Console.WriteLine("strLowByte = {0}", strLowByte);

    string strHighAll = strLowByte + strHighByte;

    uint num_Pyranometer = uint.Parse(strHighAll, System.Globalization.NumberStyles.AllowHexSpecifier);


    byte[] floatVals_Pyranometer = BitConverter.GetBytes(num_Pyranometer);
    UInt16 f_Pyranometer = BitConverter.ToUInt16(floatVals_Pyranometer, 0);
    return f_Pyranometer;

    // Str_watt_mm = (f_Pyranometer *2).ToString("0.00");

    // Console.WriteLine("Str_watt_mm = {0}", Str_watt_mm);
  }

}