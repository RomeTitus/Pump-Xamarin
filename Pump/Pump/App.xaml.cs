using System;
using System.Collections.ObjectModel;
using Pump.Database;
using Pump.Database.Table;
using Pump.Droid.Database.Table;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.Layout;
using Xamarin.Forms;

namespace Pump
{
    public partial class App : Application
    {
        private readonly ObservableCollection<Equipment> _equipmentList = new ObservableCollection<Equipment>{ null };
        private readonly ObservableCollection<Sensor> _sensorList = new ObservableCollection<Sensor> { null };
        private readonly ObservableCollection<ManualSchedule> _manualScheduleList = new ObservableCollection<ManualSchedule> { null };
        private readonly ObservableCollection<Schedule> _scheduleList = new ObservableCollection<Schedule> { null };
        private readonly ObservableCollection<CustomSchedule> _customScheduleList = new ObservableCollection<CustomSchedule> { null };
        private readonly ObservableCollection<Site> _siteList = new ObservableCollection<Site> {null};
        private readonly ObservableCollection<Alive> _aliveList = new ObservableCollection<Alive> {null};
        private readonly ObservableCollection<SubController> _subController = new ObservableCollection<SubController>{null};
        private readonly DatabaseController _databaseController = new DatabaseController();
        private readonly InitializeFirebase _initializeFirebase;

        public App()
        {
            InitializeComponent();
            //Task.Run(InitializeFirebase);
            _initializeFirebase = new InitializeFirebase(_equipmentList, _sensorList, _manualScheduleList, _scheduleList, _customScheduleList,
                _siteList, _aliveList, _subController);
            var siteScreen = new SiteScreen(_equipmentList, _sensorList, _manualScheduleList, _scheduleList, _customScheduleList,
                _siteList, _aliveList, _subController);
            MainPage = siteScreen;

            siteScreen.GetControllerPicker().SelectedIndexChanged += ConnectionPicker_OnSelectedIndexChanged;
        }

        private void ConnectionPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = (Picker) sender;

            var controllerList = new DatabaseController().GetControllerConnectionList();
            var selectedConnection = controllerList[picker.SelectedIndex];
            _initializeFirebase.Disposable();
            new DatabaseController().SetSelectedController(selectedConnection);
            _initializeFirebase.SubscribeFirebase();


        }

        protected override void OnStart()
        {
            _databaseController.SetActivityStatus(new ActivityStatus(true));
        }

        protected override void OnSleep()
        {
            _databaseController.SetActivityStatus(new ActivityStatus(false));
        }

        protected override void OnResume()
        {
            _databaseController.SetActivityStatus(new ActivityStatus(true));
        }
    }
}