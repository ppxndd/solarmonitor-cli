﻿using System;
using Npgsql;
using System.IO.Ports;
using EasyModbus;

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
            Meter meter = new Meter(meterName, slaveId, modbusSvc);
            meters = meters.Concat([meter]).ToArray();
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
        case "meter-display":
          if (meters.Length < 1) Console.WriteLine("No meter in this system.");
          else
          {
            Console.WriteLine("\n-----* Meters detail *-----");
            foreach (Meter meter in meters)
            {
              try
              {
                Console.WriteLine($"Slave : {meter.SlaveId} Name : {meter.Name}");
              }
              catch
              {
                Console.WriteLine("Error");
              }
            }
          }
          break;
        case "db-connect":
          dbSvc = new DatabaseService();
          break;
        case "log-test":
          logSvc = new LoggingService();
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