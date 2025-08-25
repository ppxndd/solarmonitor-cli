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
  static Meter[] meters = new Meter[] { };
  static Sensor _sensor;
  static ModbusService[] modbusList = new ModbusService[] { };
  static MeterDatabase? meterRepo;
  static SensorDatabase? _sensorRepo;

  static StatusDatabase? _statusRepo;

  static void Main()
  {

    configSvc = new ConfigService();
    logSvc = new LoggingService();
    RunConsole();
  }

  private static void RunConsole()
  {
    while (true)
    {
      string? rawInput = Console.ReadLine();
      if (string.IsNullOrWhiteSpace(rawInput)) return;

      string[] parts = rawInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);

      string command = parts[0];
      string[] arguments = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

      switch (command)
      {
        case "port-get":
          GetAvailablePort();
          break;
        case "modbus-connect":
          if (configSvc == null) return;
          else if (_statusRepo == null) Console.WriteLine("Please connect database before.");
          else
          {
            string serialPort = configSvc.SerialPort;
            int baudrate = configSvc.Baudrate;
            string parity = configSvc.Parity;
            string stopbits = configSvc.StopBits;
            int unitIden = configSvc.UnitIden;
            modbusSvc = new ModbusService(serialPort, baudrate, parity, stopbits, unitIden, _statusRepo);
          }
          break;
        case "modbus-create":
          if (arguments.Length < 3) Console.WriteLine("modbus-create <Name> <Serial Port> <Slave Id>");
          else if (_statusRepo == null) Console.WriteLine("Please connect database before.");
          else
          {
            string comPort = arguments[1];
            int.TryParse(arguments[2], out int slaveId);
            try
            {
              ModbusService mb = new ModbusService(comPort, 9600, "none", "one", (byte)slaveId, _statusRepo);
              modbusList = modbusList.Concat([mb]).ToArray();
            }
            catch (Exception error)
            {
              Console.WriteLine("Something error. Please try again.");
            }
          }
          break;
        case "modbus-disconnect":
          modbusSvc?.Dispose();
          break;
        case "modbus-list":
          if (modbusList.Length < 1) Console.WriteLine("No modbus in this system.");
          else
          {
            Console.WriteLine("\nID\tSerial Port");
            for (int i = 0; i < modbusList.Length; i++)
            {
              Console.WriteLine($"{i}\t{modbusList[i].SerialPort}");
            }
            Console.WriteLine();
          }
          break;
        case "modbus-slave":
          modbusSvc?.GetAvailableSlave();
          break;
        case "modbus-read":
          if (arguments.Length < 4) Console.WriteLine("modbus-read <COM Port> <Slave ID> <start address> <quantity>");
          else if (_statusRepo == null) Console.WriteLine("Please connect database before.");
          else
          {
            int.TryParse(arguments[1], out int slaveIdInt);
            int.TryParse(arguments[2], out int startAddress);
            int.TryParse(arguments[3], out int quantity);

            ModbusService modbusService = new ModbusService(arguments[0], 9600, "none", "one", 1, _statusRepo);
            int[] modbusResponse = modbusService.ReadOneValue((byte)slaveIdInt, startAddress, quantity);
            Console.WriteLine();
            for (int i = 0; i < modbusResponse.Length; i++)
            {
              Console.WriteLine($"address {i}: {modbusResponse[i]}");
            }
            Console.WriteLine();
            modbusService.Dispose();
          }
          break;
        case "modbus-run":
          modbusSvc?.StartBackgroundReading(TimeSpan.FromMinutes(1), meters);
          break;
        case "modbus-log":
          var lastTimeExecuted = modbusSvc?.LastTimeExecuted;
          Console.WriteLine($"Last executed is {lastTimeExecuted}");
          break;
        case "config-load":
          configSvc?.LoadConfig();
          break;
        case "config-display":
          configSvc?.DisplayConfig();
          break;
        case "meter-create":
          if (arguments.Length < 2) Console.WriteLine("meter-create {name meter} {slave id}");
          else if (meterRepo == null) Console.WriteLine("Please connect database");
          else if (logSvc == null) Console.WriteLine("...");
          else if (modbusSvc == null) Console.WriteLine("Please connect modbus.");
          else
          {
            string meterName = arguments[0];
            byte slaveId;
            byte.TryParse(arguments[1], out slaveId);
            Console.WriteLine("\nAdding meter...");
            try
            {
              Meter meter = new Meter(meterName, slaveId, modbusSvc, logSvc, meterRepo);
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
            for (int i = 0; i < meters.Length; i++)
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
          }
          break;
        case "meter-stop":
          for (int i = 0; i < meters.Length; i++)
          {
            meters[i].StopBackgroundReading();
            Console.WriteLine($"Stop reading meter {i}");
          }
          break;
        case "sensor-create":
          if (arguments.Length < 2) Console.WriteLine("sensor-create <index of modbus HT sensor> <index of modbus pyranometer sensor>");
          else if (modbusList.Length < 2) Console.WriteLine("Please create modbus before use this command.");
          else if (_sensorRepo == null) Console.WriteLine("Please connect database.");
          else if (logSvc == null) Console.WriteLine("Please create logging service");
          else
          {
            int.TryParse(arguments[0], out int indexModbusHT);
            int.TryParse(arguments[1], out int indexModbusPyranometer);
            LoggingService sensorLog = new LoggingService(type: "sensor");
            _sensor = new Sensor(modbusList[indexModbusHT], modbusList[indexModbusPyranometer], _sensorRepo, sensorLog);
          }
          break;
        case "sensor-run":
          if (_sensor == null) Console.WriteLine("Please create sensor before run");
          else
          {
            _sensor.StartBackgroundReading(TimeSpan.FromMinutes(1));
            Console.WriteLine("Sensor start reading...\n");
          }
          break;
        case "sensor-stop":
          _sensor.StopBackgroundReading();
          Console.WriteLine("Stop reading sensor.\n");
          break;
        case "sensor-list":
          break;
        case "db-connect":
          dbSvc = new DatabaseService(logSvc);
          meterRepo = new MeterDatabase(dbSvc);
          _statusRepo = new StatusDatabase(dbSvc);
          _sensorRepo = new SensorDatabase(dbSvc);
          Console.WriteLine($"\nConnected Database success.\n");
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