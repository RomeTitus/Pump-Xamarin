using System;
using System.IO;
using Pump.Database;
using Pump.iOS.Database;
using SQLite;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLite_IOS))]

namespace Pump.iOS.Database
{
    class SQLite_IOS : ISQLite
    {
        public SQLiteConnection GetConnection()
        {
            var sqliteFileName = "farm.db3";
            string dockumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var librarypath = Path.Combine(dockumentsPath, "..", "Library");
            var path = Path.Combine(librarypath, sqliteFileName);
            var conn = new SQLiteConnection(path);

            return conn;
        }
    }
}