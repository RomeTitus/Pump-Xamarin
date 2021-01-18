using System;
using Pump.Database;
using Pump.Database.Table;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.Layout;
using Xamarin.Forms;

namespace Pump
{
    public partial class App : Application
    {
        private readonly ObservableIrrigation _observableIrrigation = new ObservableIrrigation();
        private readonly DatabaseController _databaseController = new DatabaseController();
        private readonly InitializeFirebase _initializeFirebase;

        public App()
        {
            InitializeComponent();
            if (_databaseController.GetControllerConnectionSelection() == null)
            {
                var newConnectionScreen = new AddExistingController(true);
                MainPage = newConnectionScreen;
            }
            else
            {
                _initializeFirebase = new InitializeFirebase(_observableIrrigation);
                var siteScreen = new SiteScreen(_observableIrrigation);
                MainPage = siteScreen;

                siteScreen.GetControllerPicker().SelectedIndexChanged += ConnectionPicker_OnSelectedIndexChanged;

            }
        }

        private void ConnectionPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {

            var picker = (Picker) sender;

            if (picker.SelectedIndex == -1)
                return;
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