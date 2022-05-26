using System.IO;
using Windows.Storage;
using Pump.Database;
using Pump.Droid.Database;
using SQLite;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLite_UWP))]

namespace Pump.Droid.Database
{
    class SQLite_UWP : ISQLite
    {
        public SQLiteConnection GetConnection()
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "farm.db3");
            var conn = new SQLiteConnection(dbpath);

            return conn;
        }
    }
}