using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pump.Database
{
    public interface ISQLite
    {
        SQLiteConnection GetConnection();
    }
}
