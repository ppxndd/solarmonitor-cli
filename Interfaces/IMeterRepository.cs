public interface IMeterRepository
{
  void CreateOneDataMeter(float[] payload, byte slaveId, DateTime timestamp);
}