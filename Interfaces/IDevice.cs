public interface IDevice
{
  public void StartBackgroundReading(TimeSpan interval);
  public void StopBackgroundReading(); 
}