using System;
using System.IO.Ports;
using EasyModbus;

class ModbusService : IDisposable
{
  private ModbusClient modbusClient;
  private CancellationTokenSource cts;
  public ModbusService(string serialPort, int baudrate, string parity, string stopbits, int unitIden)
  {
    this.modbusClient = new ModbusClient(serialPort);

    this.modbusClient.Baudrate = baudrate;
    this.modbusClient.Parity = this.SetParity(parity);

    this.modbusClient.StopBits = this.SetStopbits(stopbits);
    this.modbusClient.UnitIdentifier = (byte)unitIden;
    try
    {
      this.modbusClient.Connect();
      Console.WriteLine($"Connect modbus serial port {serialPort} successfully.");
    }
    catch (EasyModbus.Exceptions.ModbusException mex)
    {
      Console.WriteLine("Modbus Exception: " + mex.Message);
    }
    catch (TimeoutException)
    {
      Console.WriteLine("Timeout: No response from Modbus slave.");
    }
    catch (Exception ex)
    {
      Console.WriteLine("General error: " + ex.Message);
    }
  }

  private System.IO.Ports.Parity SetParity(string parity)
  {
    if (this.modbusClient == null) return System.IO.Ports.Parity.None;

    if (parity == "none")
    {
      return System.IO.Ports.Parity.None;
    }
    else if (parity == "odd")
    {
      return System.IO.Ports.Parity.Odd;
    }
    else if (parity == "even")
    {
      return System.IO.Ports.Parity.Even;
    }
    else
    {
      return System.IO.Ports.Parity.None;
    }
  }

  private System.IO.Ports.StopBits SetStopbits(string stopbits)
  {
    if (this.modbusClient == null) return System.IO.Ports.StopBits.None;
    if (stopbits == "one")
    {
      return System.IO.Ports.StopBits.One;
    }
    else if (stopbits == "two")
    {
      return System.IO.Ports.StopBits.Two;
    }
    else
    {
      return System.IO.Ports.StopBits.None;
    }
  }

  public int[] ReadPairInput(byte slaveId, int startAddress, int quantity)
  {
    this.modbusClient.UnitIdentifier = slaveId;
    int[] rawValues = [];
    int loadTime = 0;
    quantity = quantity * 2;
    while (quantity > 40)
    {
      quantity -= 40;
      loadTime++;
    }

    for (int i = 0; i < loadTime; i++)
    {
      int[] readedValues = this.modbusClient.ReadInputRegisters(startAddress, 40);
      rawValues = rawValues.Concat(readedValues).ToArray();
      startAddress += 40;
    }

    if (quantity > 0)
    {
      int[] readedValues = this.modbusClient.ReadInputRegisters(startAddress, quantity);
      rawValues = rawValues.Concat(readedValues).ToArray();
      startAddress += quantity;
    }

    return rawValues;
  }

  public void TestReadValue()
  {
    int[] values = this.modbusClient.ReadInputRegisters(10, 10);
    foreach (int value in values) {
      Console.WriteLine(value);
    }
  }

  public void GetAvailableSlave()
  {
    Console.WriteLine("\nScanning Modbus slaves on...");

    for (int i = 1; i < 247; i++)
    {
      try
      {
        this.modbusClient.UnitIdentifier = (byte)i;
        int[] values = this.modbusClient.ReadInputRegisters(0, 10);
        Console.WriteLine($"Found slave id {i}");
      }
      catch
      {
        Console.WriteLine($"Not found slave id {i}");
      }
    }

    Console.WriteLine("Scan finished...");
  }

  public void StartBackgroundReading(TimeSpan interval, Meter[] meters)
  {
    cts = new CancellationTokenSource();
    Task.Run(() => ReadLoop(interval, cts.Token, meters));
  }

  public void StopBackgroundReading()
  {
    cts?.Cancel();
  }

  private async Task ReadLoop(TimeSpan interval, CancellationToken token, Meter[] meters)
  {
    while (!token.IsCancellationRequested)
    {
      try
      {
        for (int i = 0; i < meters.Length; i++)
        {
          if (meters[i] == null) Console.WriteLine($"Meter at index {i} is null");
          else
          {
            meters[i].ReadValueFromMeter(41);
          }
          Console.WriteLine($"{meters[i].Name} start reading...");
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error reading Modbus data: {ex.Message}");
      }

      await Task.Delay(interval, token);
    }
  }

  public void Dispose()
  {
    modbusClient?.Disconnect();
  }
}