using System;
using System.IO;
using Pump.Database;
using Pump.Droid.Database;
using SQLite;
using Windows.Storage;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLite_UWP))]

namespace Pump.Droid.Database
{
    class SQLite_UWP : ISQLite
    {
        public SQLite_UWP() { }
        public SQLiteConnection GetConnection()
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "farm.db3");
            var conn = new SQLiteConnection(dbpath);

            return conn;
        }

    }
}