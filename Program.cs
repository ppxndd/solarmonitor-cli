using System;
using Npgsql;
using System.IO.Ports;
using EasyModbus;
using Serilog;

class Program
{

  static DatabaseService? dbSvc;
  static ModbusService? modbusSvc;
  static ConfigService? configSvc;
  static LoggingService? logSvc;
  static Meter[] meters = new Meter[]{};

  static void Main()
  {

    configSvc = new ConfigService();
    logSvc = new LoggingService();
    while (true)
    {
      string? rawInput = Console.ReadLine();
      if (string.IsNullOrWhiteSpace(rawInput)) return;

      string[] parts = rawInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);

      string command = parts[0];
      string[] arguments = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

      switch (command)
      {
        case "help":
          Console.WriteLine(
            "\n-- Help Commange --\n" +
            "-- port-get\n" +
            "-- modbus-connect\n" +
            "-- modbus-disconnect\n" +
            "-- modbus-read-meter [quantity register]\n" +
            "-- config-init\n" +
            "-- config-display\n" +
            "-- log-test\n" +
            "-- exit\n\n"
            );
          break;
        case "port-get":
          GetAvailablePort();
          break;
        case "modbus-connect":
          if (configSvc == null) return;
          string serialPort = configSvc.SerialPort;
          int baudrate = configSvc.Baudrate;
          string parity = configSvc.Parity;
          string stopbits = configSvc.StopBits;
          int unitIden = configSvc.UnitIden;
          modbusSvc = new ModbusService(serialPort, baudrate, parity, stopbits, unitIden);
          break;
        case "modbus-disconnect":
          modbusSvc?.Dispose();
          break;
        case "modbus-test":
          modbusSvc?.TestReadValue();
          break;
        case "modbus-slave":
          modbusSvc?.GetAvailableSlave();
          break;
        case "modbus-run":
          modbusSvc?.StartBackgroundReading(TimeSpan.FromMinutes(1), meters);
          break;
        case "config-init":
          configSvc?.InitConfig();
          break;
        case "config-display":
          configSvc?.DisplayConfig();
          break;
        case "meter-create":
          if (arguments.Length < 2) Console.WriteLine("meter-create {name meter} {slave id}");
          else if (modbusSvc == null) Console.WriteLine("Please connect modbus.");
          else
          {
            string meterName = arguments[0];
            byte slaveId;
            byte.TryParse(arguments[1], out slaveId);
            Console.WriteLine("\nAdding meter...");
            try
            {
              Meter meter = new Meter(meterName, slaveId, modbusSvc, logSvc, dbSvc);
              meter.ReadValueFromMeter(1);
              meters = meters.Concat([meter]).ToArray();
              Console.WriteLine("Add meter success.\n");
            }
            catch (Exception ex)
            {
              Console.WriteLine($"Add meter failed meter not response.\n {ex.Message}");

            }
          }
          break;
        case "meter-read":
          if (arguments.Length < 2) Console.WriteLine("meter-read {meter id} {quantity value}");
          else if (modbusSvc == null) Console.WriteLine("Please connect modbus");
          else
          {
            int slaveIdMeter;
            int quantity;
            int.TryParse(arguments[0], out slaveIdMeter);
            int.TryParse(arguments[1], out quantity);
            Meter meter = meters[slaveIdMeter];
            meter.ReadValueFromMeter(quantity);
          }
          break;
        case "meter-list":
          if (meters.Length < 1) Console.WriteLine("No meter in this system.");
          else
          {
            Console.WriteLine("\n-----* Meters detail *-----");
            for (int i=0; i<meters.Length; i++) 
            {
              try
              {
                Meter meter = meters[i];
                Console.WriteLine($"Index: {i} Slave Id : {meter.SlaveId} Name : {meter.Name}");
              }
              catch
              {
                Console.WriteLine("Error");
              }
            }
          }
          break;
        case "meter-run":
          if (meters.Length < 1) Console.WriteLine("No meter in this system.");
          else
          {
            for (int i = 0; i < meters.Length; i++)
            {
              meters[i].StartBackgroundReading(TimeSpan.FromMinutes(1));
              Console.WriteLine($"{meters[i].Name} start reading...");
            }
            Console.WriteLine($"Press Enter to stop...");
            Console.ReadLine();

            for (int i = 0; i < meters.Length; i++)
            {
              meters[i].StopBackgroundReading();
            }
          }
          break;
        case "db-connect":
          dbSvc = new DatabaseService(logSvc);
          break;
        case "exit":
          Console.WriteLine("Goodbye!");
          return;
      }
    }

  }

  private static void GetAvailablePort()
  {
    string[] ports = SerialPort.GetPortNames();
    foreach (string port in ports)
    {
      Console.WriteLine("Serial port: " + port);
    }
  }

}