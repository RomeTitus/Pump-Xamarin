using System;
using System.IO;
using Pump.Database;
using Pump.Droid.Database;
using SQLite;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLite_Android))]

namespace Pump.Droid.Database
{
    internal class SQLite_Android : ISQLite
    {
        public SQLiteConnection GetConnection()
        {
            var sqliteFileName = "farm.db3";
            var dockumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var path = Path.Combine(dockumentsPath, sqliteFileName);
            var conn = new SQLiteConnection(path);

            return conn;
        }
    }
}