using System;
using Pump.Database;
using Pump.Database.Table;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.Layout;
using Pump.SocketController;
using Xamarin.Forms;

namespace Pump
{
    public partial class App : Application
    {
        private readonly ObservableIrrigation _observableIrrigation = new ObservableIrrigation();
        private readonly DatabaseController _databaseController = new DatabaseController();
        private readonly SocketPicker _socketPicker;

        public App()
        {
            InitializeComponent();
            _socketPicker = new SocketPicker(_observableIrrigation);
            var siteScreen = new SiteScreen(_observableIrrigation);
            
            MainPage = siteScreen;
            siteScreen.GetControllerPicker().SelectedIndexChanged += ConnectionPicker_OnSelectedIndexChanged;
            siteScreen.GetControllerPicker().SelectedIndexChanged += ConnectionPicker_OnSelectedIndexChanged;
            _socketPicker.Subscribe();
        }

        private async void ConnectionPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {

            var picker = (Picker) sender;

            if (picker.SelectedIndex == -1)
                return;
            var controllerList = new DatabaseController().GetControllerConnectionList();
            var selectedConnection = controllerList[picker.SelectedIndex];
            _socketPicker.Disposable();
            new DatabaseController().SetSelectedController(selectedConnection);
            await _socketPicker.Subscribe();
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