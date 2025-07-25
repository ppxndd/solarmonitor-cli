using System;
using System.IO.Ports;
using EasyModbus;

class ModbusService : IDisposable
{
  private ModbusClient modbusClient;
  public ModbusService(string serialPort, int baudrate, string parity, string stopbits, int unitIden)
  {
    modbusClient = new ModbusClient(serialPort);

    modbusClient.Baudrate = baudrate;
    modbusClient.Parity = this.SetParity(parity);

    modbusClient.StopBits = this.SetStopbits(stopbits);
    modbusClient.UnitIdentifier = (byte)unitIden;
    try
    {
      modbusClient.Connect();
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
    if (modbusClient == null) return System.IO.Ports.Parity.None;

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
    if (modbusClient == null) return System.IO.Ports.StopBits.None;
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
    modbusClient.UnitIdentifier = slaveId;
    int[] rawValues = [];
    int loadTime = 0;
    quantity = quantity * 2;
    while (quantity > 40)
    {
      quantity -= 40;
      loadTime++;
    }
    Console.WriteLine($"\nLoad time = {loadTime}\nQuantity = {quantity}");

    for (int i = 0; i < loadTime; i++)
    {
      Console.WriteLine($"Chunk {i} quantity {quantity} start address {startAddress}");
      int[] readedValues = this.modbusClient.ReadInputRegisters(startAddress, 40);
      rawValues = rawValues.Concat(readedValues).ToArray();
      startAddress += 40;
    }

    if (quantity > 0)
    {
      Console.WriteLine($"quantity {quantity} start address {startAddress}");
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
        int[] values = this.modbusClient.ReadInputRegisters(0, 1);
        Console.WriteLine($"Found slave id {i}");
      }
      catch
      {
        Console.WriteLine($"Not found slave id {i}");
      }
    }

    Console.WriteLine("Scan finished...");

  }

  public void Dispose()
  {
    modbusClient?.Disconnect();
  }
}