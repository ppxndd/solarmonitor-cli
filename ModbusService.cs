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
      // int decHighBit = rawDataFromModbus[2 * i];
      // int decLowBit = rawDataFromModbus[(2 * i) + 1];

      // string hexHighBit = decHighBit.ToString("X");
      // string hexLowBit = decLowBit.ToString("X");



      // string fullRegisterBit = hexHighBit + hexLowBit;

      // uint decValue = uint.Parse(fullRegisterBit, System.Globalization.NumberStyles.AllowHexSpecifier);

      // byte[] floatVals = BitConverter.GetBytes(decValue);
      // float f = BitConverter.ToSingle(floatVals, 0);

      ushort reg1 = (ushort)rawDataFromModbus[2 * i]; // high word?
      ushort reg2 = (ushort)rawDataFromModbus[(2 * i) + 1]; // low word?
      byte[] abcd = {
          (byte)(reg1 >> 8), (byte)(reg1 & 0xFF),
          (byte)(reg2 >> 8), (byte)(reg2 & 0xFF)
      };

      byte[] cdab = {
          (byte)(reg2 >> 8), (byte)(reg2 & 0xFF),
          (byte)(reg1 >> 8), (byte)(reg1 & 0xFF)
      };

      byte[] badc = {
          (byte)(reg1 & 0xFF), (byte)(reg1 >> 8),
          (byte)(reg2 & 0xFF), (byte)(reg2 >> 8)
      };

      byte[] dcba = {
          (byte)(reg2 & 0xFF), (byte)(reg2 >> 8),
          (byte)(reg1 & 0xFF), (byte)(reg1 >> 8)
      };

      float f_abcd = BitConverter.ToSingle(abcd, 0);
      float f_cdab = BitConverter.ToSingle(cdab, 0);
      float f_badc = BitConverter.ToSingle(badc, 0);
      float f_dcba = BitConverter.ToSingle(dcba, 0);

      Console.WriteLine($"[{i + 1}] Raw: {reg1}, {reg2}");
      Console.WriteLine($"  ABCD : {f_abcd}");
      Console.WriteLine($"  CDAB : {f_cdab}");
      Console.WriteLine($"  BADC : {f_badc}");
      Console.WriteLine($"  DCBA : {f_dcba}");
      Console.WriteLine();

      // Console.WriteLine($"\n-----* Meter Value *-----\nHigh : {hexHighBit}\nLow : {hexLowBit}\nDecimal value : {f}\n");
    }
  }

  public void Dispose()
  {
    modbusClient?.Disconnect();
  }
}