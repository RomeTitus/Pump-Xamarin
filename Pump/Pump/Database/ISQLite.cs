using SQLite;

namespace Pump.Database
{
    public interface ISQLite
    {
        SQLiteConnection GetConnection();
    }
}