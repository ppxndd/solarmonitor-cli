using System;
using System.IO.Ports;
using EasyModbus;

class ModbusService : IDisposable
{

  static ModbusClient? modbusClient;
  public ModbusService(string serialPort, int baudrate, string parity, string stopbits, int unitIden)
  {
    modbusClient = new ModbusClient(serialPort);

    modbusClient.Baudrate = baudrate;
    modbusClient.Parity = SetParity(parity);

    modbusClient.StopBits = SetStopbits(stopbits);
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

  static private System.IO.Ports.Parity SetParity(string parity)
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

  static private System.IO.Ports.StopBits SetStopbits(string stopbits)
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

  public void GetValueFromMeter(int lengthReg)
  {
    if (modbusClient == null) return;
    int[] rawDataFromModbus = modbusClient.ReadInputRegisters(0, lengthReg);
    for (int i = 0; i < rawDataFromModbus.Length / 2; i++)
    {
      int decHighBit = rawDataFromModbus[2 * i];
      int decLowBit = rawDataFromModbus[(2 * i) + 1];

      ushort highUShort = (ushort)(decHighBit & 0xFFFF);
      ushort lowUShort = (ushort)(decLowBit & 0xFFFF);

      // Pack as big-endian (most meters use big-endian)
      byte[] bytes = new byte[4];
      bytes[0] = (byte)(highUShort >> 8);    // high byte of high word
      bytes[1] = (byte)(highUShort & 0xFF);  // low byte of high word
      bytes[2] = (byte)(lowUShort >> 8);     // high byte of low word
      bytes[3] = (byte)(lowUShort & 0xFF);   // low byte of low word

      // If your device uses little-endian, reverse the byte array
      // Array.Reverse(bytes);

      float value = BitConverter.ToSingle(bytes, 0);

      Console.WriteLine($"Float[{i + 1}] = {value}  (Raw: {decHighBit}, {decLowBit})");
    }
  }

  public void Dispose()
  {
    modbusClient?.Disconnect();
  }
}