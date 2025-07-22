using System;
using System.IO.Ports;
using EasyModbus;

class Meter
{
  private string name;
  private byte slaveId;
  private ModbusService modbusSvc;
  public string Name
  {
    get { return name; }
  }
  public int SlaveId
  {
    get { return slaveId; }
  }

  public Meter(string name, byte slaveId, ModbusService modbusService)
  {
    this.name = name;
    this.slaveId = slaveId;
    this.modbusSvc = modbusService;
  }

  public void ReadValueFromMeter(int amountValue)
  {
    int[] valueFromModbus = this.modbusSvc.ReadPairInput(this.slaveId, 0, amountValue);

    Console.WriteLine("\nReading Data...");

    for (int i = 0; i < valueFromModbus.Length/2; i++)
    {
      Console.Write($"index {i} : {valueFromModbus[i]}  ");

      Console.WriteLine($"Format value : {this.FormatValueDCBA([valueFromModbus[2*i], valueFromModbus[(2*i)+1]])}");
    }
    Console.WriteLine("Reading success.");
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