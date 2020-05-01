using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Pump.Database;
using Pump.Droid.Database;
using SQLite;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLite_Android))]

namespace Pump.Droid.Database
{
    class SQLite_Android : ISQLite
    {
        public SQLite_Android() { }
        public SQLiteConnection GetConnection()
        {
          

            var sqliteFileName = "farm.db3";
            string dockumentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var path = Path.Combine(dockumentsPath, sqliteFileName);
            var conn = new SQLiteConnection(path);
            
            return conn;
        }


    }
}