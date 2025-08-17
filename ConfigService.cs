using System;
using System.IO;

class ConfigService : IDisposable
{
  const string path = "config.txt";
  private string serialPort = "COM1";
  private int baudrate = 9600;
  private string parity = "none";
  private string stopbits = "one";
  private int unitIden = 1;
  public string SerialPort
  {
    get { return serialPort; }
  }
  public int Baudrate
  {
    get { return baudrate; }
  }
  public string Parity
  {
    get { return parity; }
  }
  public string StopBits
  {
    get { return stopbits; }
  }
  public int UnitIden
  {
    get { return unitIden; }
  }

  public ConfigService()
  {
    this.LoadConfig();
  }

  public void Dispose()
  {
    Console.WriteLine("Exit File...");
  }

  public void LoadConfig()
  {
    try
    {
      if (!File.Exists(path))
      {
        Console.WriteLine($"Error: config file not found: {path}");
        return;
      }

      string[] pairs = File.ReadAllText(path).Split(',');
      foreach (string pair in pairs)
      {
        string[] config = pair.Split('=');
        if (config.Length != 2) continue;

        string name = config[0].Trim();
        string value = config[1].Trim();

        switch (name)
        {
          case "serialPort":
            serialPort = value;
            break;
          case "baudrate":
            int.TryParse(value, out baudrate);
            break;
          case "parity":
            parity = value.ToLower();
            break;
          case "stopbits":
            stopbits = value.ToLower();
            break;
          case "unitIden":
            int.TryParse(value, out unitIden);
            break;
        }
      }
      Console.WriteLine("Read config successfully.");
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error: can't read config...");
    }
  }

    public void DisplayConfig()
    {
        Console.WriteLine("\n*---- Config ----*");
        Console.WriteLine($"Serial port   : {serialPort}");
        Console.WriteLine($"Baudrate      : {baudrate}");
        Console.WriteLine($"Parity        : {parity}");
        Console.WriteLine($"Stopbits      : {stopbits}");
        Console.WriteLine($"Unit Identifier: {unitIden}\n");
    }
}