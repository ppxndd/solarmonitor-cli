using System;

class StatusDatabase
{
  private DatabaseService _dbSvc;

  public StatusDatabase(DatabaseService dbSvc)
  {
    _dbSvc = dbSvc;
  }

  public int GetTotalCountRow()
  {
    string query = @"
      SELECT COUNT(*) FROM meter_measurements
    ";

    return _dbSvc.ExecuteScalar<int>(query);
  }
}