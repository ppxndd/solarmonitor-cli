using System;
using System.IO;
using System.IO.Ports;

class ConfigService : IDisposable
{

  const string path = "config.txt";
  public string serialPort;
  public int baudrate;
  public string parity;
  public string stopbits;
  public int unitIden;

  public ConfigService()
  {
    this.InitConfig();
  }


  public void Dispose()
  {
    Console.WriteLine("Exit File...");
  }

  public void InitConfig()
  {
    string[] data = File.ReadAllText(path).Split(',');
    for (int i = 0; i < data.Length; i++)
    {
      string[] config = data[i].Split('=');
      string configName = config[0];
      string configValue = config[1];
      if (configName == "serialPort")
      {
        this.serialPort = configValue;
      }
      if (configName == "baudrate")
      {
        this.baudrate = Int32.Parse(configValue);
      }
      if (configName == "parity")
      {
        this.parity = configValue;
      }
      if (configName == "stopbits")
      {
        this.stopbits = configValue;
      }
      if (configName == "unitIden")
      {
        this.unitIden = Int32.Parse(configValue);
      }
    }

    Console.WriteLine("Read config from text file successfully.\n");
  }

  public void DisplayConfig()
  {
    Console.WriteLine("\n*---- Config ----*");
    Console.WriteLine($"Serial port : {serialPort}");
    Console.WriteLine($"Baudrate : {baudrate}");
    Console.WriteLine($"Parity : {parity}");
    Console.WriteLine($"Stopbits : {stopbits}");
    Console.WriteLine($"UnitIdentifier : {unitIden}\n\n");
  }
}