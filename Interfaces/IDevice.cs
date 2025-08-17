public interface IDevice
{
  static string type;
  public void StartBackgroundReading(TimeSpan interval);
  public void StopBackgroundReading(); 
}