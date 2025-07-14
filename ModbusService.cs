using System;
using EasyModbus;

class ModbusService : IDisposable
{

  static ModbusClient? modbusClient;
  public ModbusService()
  {
    modbusClient = new ModbusClient("COM20");

    modbusClient.Baudrate = 9600;
    modbusClient.Parity = System.IO.Ports.Parity.None;
    modbusClient.StopBits = System.IO.Ports.StopBits.One;
    modbusClient.UnitIdentifier = 1;

    try
    {
      modbusClient.Connect();
      Console.WriteLine(modbusClient.Connected);
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

  public void DisplayVolt()
  {
    int[] volt = ReadVolt();
    string base16 = Convert.ToString(volt[0], 16);
    Console.WriteLine($"Base 16 Volt : {base16}");
    for (int i = 0; i < volt.Length; i++)
    {
      Console.WriteLine($"Volt : {volt[i]}");
    }
  }

  static public int[] ReadVolt()
  {
    if (modbusClient.Connected)
    {
      int[] holdingReg = modbusClient.ReadHoldingRegisters(0, 1);
      return holdingReg;
    }
    return null;
  }

  public void ReadReg(int startAddress, int quantity)
  {
    if (modbusClient.Connected)
    {
      int[] regValues = modbusClient.ReadHoldingRegisters(startAddress, quantity);
      for (int i = 0; i < regValues.Length; i++)
      {
        Console.WriteLine($"Register address {startAddress+i} :---> {regValues[i]}");
      }
    }
  }

  public void Dispose()
  {
    modbusClient?.Disconnect();
  }
}