using System;
using Npgsql;
using System.IO.Ports;
using EasyModbus;

class Program
{


  // Global variables
  static NpgsqlConnection? conn;
  static ModbusService? modbusSvc;
  static ConfigService? configSvc;

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
        case "get-all":
          GetAllStudents();
          break;
        case "port-get":
          GetAvailablePort();
          break;
        case "modbus-connect":
          if (configSvc == null) return;
          string serialPort = configSvc.serialPort;
          int baudrate = configSvc.baudrate;
          string parity = configSvc.parity;
          string stopbits = configSvc.stopbits;
          int unitIden = configSvc.unitIden;
          modbusSvc = new ModbusService(serialPort, baudrate, parity, stopbits, unitIden);
          break;
        case "modbus-disconnect":
          modbusSvc?.Dispose();
          break;
        case "modbus-read":
          if (arguments.Length < 2) return;
          int startAddress = Int32.Parse(arguments[0]);
          int quantity = Int32.Parse(arguments[1]);
          modbusSvc?.ReadReg(startAddress, quantity);
          break;
        case "modbus-read-meter":
          if (modbusSvc == null) return;
          if (arguments.Length < 1) return;
          int lengthReg = Int32.Parse(arguments[0]);
          modbusSvc.GetValueFromMeter(lengthReg);
          break;
        case "config-init":
          configSvc?.InitConfig();
          break;
        case "config-display":
          configSvc?.DisplayConfig();
          break;
        case "exit":
          conn?.Close();
          conn?.Dispose();
          Console.WriteLine("Goodbye!");
          return;
      }
    }

  }

  private static void ConnectDB()
  {
    var builder = new Npgsql.NpgsqlConnectionStringBuilder()
    {
      Host = "localhost",
      Port = 5432,
      Username = "postgres",
      Password = "mysecretpassword",
      Database = "mydatabase"
    };
    string connectionString = builder.ConnectionString;

    conn = new NpgsqlConnection(connectionString);

    try
    {
      conn.Open();
      Console.WriteLine("Connected to PostgreSQL!");

    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error: {ex.Message}");
    }
  }

  private static void AddData(string id, string name, string email, string grade)
  {
    Console.WriteLine($"Id : {id}\nName : {name}\nEmail: {email}\nGrade : {grade}");
    string sql = "INSERT INTO students (id, name, email, grade) VALUES (@id, @name, @email, @grade)";

    int intId = Int32.Parse(id);

    using var cmd = new NpgsqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("id", intId);
    cmd.Parameters.AddWithValue("name", name);
    cmd.Parameters.AddWithValue("email", email);
    cmd.Parameters.AddWithValue("grade", grade);

    try
    {
      cmd.ExecuteScalar();
      Console.WriteLine($"Insert successfully!");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error: {ex}");
    }
  }

  private static void GetAllStudents()
  {
    string sql = "SELECT id, name, email, grade FROM students;";
    using var cmd = new NpgsqlCommand(sql, conn);
    using var reader = cmd.ExecuteReader();


    Console.WriteLine("\n----- Student List -----");
    try
    {
      while (reader.Read())
      {
        int id = reader.GetInt32(0);
        string name = reader.GetString(1);
        string email = reader.GetString(2);
        string grade = reader.GetString(3);

        Console.WriteLine($"ID: {id}, Name: {name}, Email: {email}, Grade: {grade}");
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error while reading data :---> {ex.Message}");
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