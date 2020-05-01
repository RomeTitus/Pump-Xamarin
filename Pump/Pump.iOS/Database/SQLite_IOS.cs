using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Foundation;
using Pump.Database;
using Pump.iOS.Database;
using SQLite;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLite_IOS))]
namespace Pump.iOS.Database
{
    class SQLite_IOS: ISQLite
    {
        public SQLite_IOS() { }

        public SQLiteConnection GetConnection()
        {
            var sqliteFileName = "farm.db3";
            string dockumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var librarypath = Path.Combine(dockumentsPath, ".." ,  "Library");
            var path = Path.Combine(librarypath, sqliteFileName);
            var conn = new SQLite.SQLiteConnection(path);

            return conn;
        }
    }
}