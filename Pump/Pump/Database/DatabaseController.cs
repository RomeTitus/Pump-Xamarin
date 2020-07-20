using System.Linq;
using Pump.Database.Table;
using Pump.Droid.Database.Table;
using SQLite;
using Xamarin.Forms;

namespace Pump.Database
{
    public class DatabaseController
    {
        //used to be thread safe
        public static readonly object Locker = new object();
        private readonly SQLiteConnection _database;

        public DatabaseController()
        {
            //database = DependencyService.Get<ISQLite>.GetConnection();

            _database = DependencyService.Get<ISQLite>().GetConnection();

            _database.CreateTable<PumpConnection>();

            _database.CreateTable<PumpSelection>();

            _database.CreateTable<ActivityStatus>();

            _database.CreateTable<NotificationToken>();
        }

        public PumpConnection GetPumpSelection()
        {
            lock (Locker)
            {
                if (_database.Table<PumpSelection>().Any())
                {
                    var pumpSelected = _database.Table<PumpSelection>().First();
                    return _database.Table<PumpConnection>().FirstOrDefault(x => x.ID == pumpSelected.PumpConnectionId);
                }

                return null;
            }
        }

        public ActivityStatus GetActivityStatus()
        {
            lock (Locker)
            {
                try
                {
                    return _database.Table<ActivityStatus>().Any() ? _database.Table<ActivityStatus>().First() : null;
                }
                catch
                {
                    return new ActivityStatus(true);
                }
            }
        }


        public void SetActivityStatus(ActivityStatus activityStatus)
        {
            lock (Locker)
            {
                try
                {
                    if (_database.Table<ActivityStatus>().Any())
                        _database.DeleteAll<ActivityStatus>();
                }
                finally
                {
                    _database.Insert(activityStatus);
                }
            }
        }

        public NotificationToken GetNotificationToken()
        {
            lock (Locker)
            {
                if (_database.Table<NotificationToken>().Any())
                    return _database.Table<NotificationToken>().First();
                return null;
            }
        }

        public void SetNotificationToken(NotificationToken notificationToken)
        {
            lock (Locker)
            {
                try
                {
                    if (_database.Table<NotificationToken>().Any())
                        _database.DeleteAll<NotificationToken>();
                }
                finally
                {
                    _database.Insert(notificationToken);
                }
            }
        }


        public void AddPumpConnection(PumpConnection pumpConnection)
        {
            lock (Locker)
            {
                if (_database.Table<PumpSelection>().Any())
                    _database.DeleteAll<PumpSelection>();

                _database.Insert(pumpConnection);
                _database.Insert(new PumpSelection(pumpConnection.ID));
            }
        }

        public void UpdatePump(PumpConnection pumpConnection)
        {
            lock (Locker)
            {
                _database.Update(pumpConnection);
            }
        }

        public PumpConnection GetControllerNameByMac(string bt)
        {
            lock (Locker)
            {
                var pumpConnection = _database.Table<PumpConnection>().FirstOrDefault(x => x.Mac.Equals(bt));
                return pumpConnection;
            }
        }

        public bool isRealtimeFirebaseSelected()
        {
            var selectedPump = GetPumpSelection();
            if (selectedPump?.RealTimeDatabase != null) return (bool) selectedPump.RealTimeDatabase;

            return false;
        }
    }
}