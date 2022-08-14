using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Firebase.Auth;
using Pump.Class;
using Pump.Database;
using Pump.Database.Table;
using Pump.IrrigationController;
using Pump.Layout;
using Pump.Layout.Dashboard;
using Pump.Layout.Views;
using Pump.SocketController;
using Pump.SocketController.Firebase;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage
    {
        private readonly AuthenticationScreen _authenticationScreen;
        private readonly DatabaseController _database;
        private readonly NotificationEvent _notificationEvent;
        private readonly Dictionary<IrrigationConfiguration, ObservableIrrigation> _observableDict;
        private readonly SocketPicker _socketPicker;
        private readonly FirebaseAuthClient _client;
        private Timer _timer;

        public MainPage(FirebaseAuthClient client)
        {
            InitializeComponent();
            _notificationEvent = new NotificationEvent();
            _observableDict = new Dictionary<IrrigationConfiguration, ObservableIrrigation>();
            _database = new DatabaseController();
            _socketPicker = new SocketPicker(new FirebaseManager(), _observableDict);
            _authenticationScreen = new AuthenticationScreen(client);
            _client = client;
            client.AuthStateChanged += ClientOnAuthStateChanged;
        }

        private async void ClientOnAuthStateChanged(object sender, UserEventArgs e)
        {
            if (e.User == null)
            {
                ScrollViewSite.Children.Clear();
                _observableDict.Clear();
                await Navigation.PushModalAsync(_authenticationScreen);
                _authenticationScreen.IsDisplayed = true;
            }
            else
            {
                if (_authenticationScreen.IsDisplayed)
                    _authenticationScreen.ClosePage();
                _authenticationScreen.IsDisplayed = false;
                var configList = _database.GetIrrigationConfigurationList();
                if (!configList.Any())
                    configList = await GetIrrigationConfigFromFirebase(e.User);
                if (!configList.Any())
                    Device.BeginInvokeOnMainThread(SetupNewController);
                else
                    await Device.InvokeOnMainThreadAsync(async () =>
                    {
                        PopulateSavedIrrigation(configList);
                        await _socketPicker.Subscribe(configList, e.User);
                    });
                StartEvent();
            }
        }

        private async Task<List<IrrigationConfiguration>> GetIrrigationConfigFromFirebase(User user)
        {
            var loadingScreen = new PopupLoading("Retrieving");
            await PopupNavigation.Instance.PushAsync(loadingScreen);
            var configList = await _socketPicker.GetIrrigationConfigurations(user);
            configList.ForEach(x => _database.SaveIrrigationConfiguration(x));
            if (PopupNavigation.Instance.PopupStack.Any())
                await PopupNavigation.Instance.PopAllAsync();
            return configList;
        }

        private void SetupNewController()
        {
            if (Navigation.ModalStack.Any(x => x.GetType() == typeof(ScanBluetooth)))
                return;
            var connectionScreen = new ScanBluetooth(_observableDict.Keys.ToList(), _notificationEvent, _socketPicker.BluetoothManager(), _database, this);
            if (Navigation.ModalStack.All(x => x.GetType() != typeof(ScanBluetooth)))
                Navigation.PushModalAsync(connectionScreen);
        }

        public void PopulateSavedIrrigation(List<IrrigationConfiguration> irrigationConfigurationList)
        {
            foreach (var configuration in irrigationConfigurationList)
            {
                if (_observableDict.Keys.Any())
                {
                    if (_observableDict.Keys.FirstOrDefault(x => x.Path == configuration.Path) == null)
                        _observableDict.Add(configuration, new ObservableIrrigation());
                }
                else
                {
                    _observableDict.Add(configuration, new ObservableIrrigation());
                }


                var viewSite = ScrollViewSite.Children.FirstOrDefault(x =>
                    x.AutomationId == configuration.Id.ToString());

                if (viewSite == null)
                {
                    var viewSiteSummary =
                        new ViewIrrigationConfigurationSummary(_observableDict.First(x => x.Key.Id == configuration.Id),
                            _socketPicker);
                    viewSiteSummary.GetTapGestureRecognizerList().ForEach(x => x.Tapped += OnTapped_HomeScreen);
                    viewSiteSummary.GetTapGestureSettings().Tapped += OnTapped_Settings;
                    ScrollViewSite.Children.Add(viewSiteSummary);
                }
            }
        }

        public void UpdateSiteNames(IrrigationConfiguration irrigationConfiguration)
        {
            var viewSiteSummary = (ViewIrrigationConfigurationSummary) ScrollViewSite.Children.First(x =>
                x.AutomationId == irrigationConfiguration.Path);

            viewSiteSummary.ReLoadChildren();
            viewSiteSummary.GetTapGestureRecognizerList().ForEach(x => x.Tapped += OnTapped_HomeScreen);
        }

        private void UpdateSavedIrrigation()
        {
            foreach (var view in ScrollViewSite.Children.Where(x => x is ViewIrrigationConfigurationSummary))
            {
                var viewConfig = (ViewIrrigationConfigurationSummary)view;
                viewConfig.Populate();
            }
        }

        private void OnTapped_Settings(object sender, EventArgs e)
        {
            if (Navigation.ModalStack.Any(x => x.GetType() == typeof(IrrigationControllerSettings)))
                return;
            var imageGesture = (Image)sender;
            var configurationSummary =
                (ViewIrrigationConfigurationSummary)imageGesture.Parent.Parent.Parent.Parent.Parent.Parent;
            var settingsScreen =
                new IrrigationControllerSettings(this, configurationSummary.GetIrrigationConfigAndObservable(),
                    _socketPicker);
            Navigation.PushModalAsync(settingsScreen);
        }

        private void OnTapped_HomeScreen(object sender, EventArgs e)
        {
            if (Navigation.ModalStack.Any(x => x.GetType() == typeof(HomeScreen)))
                return;
            
            var stackLayoutGesture = (StackLayout)sender;
            var siteSummary = (ViewIrrigationSiteSummary)stackLayoutGesture.Parent.Parent.Parent;
            var configurationSummary = (ViewIrrigationConfigurationSummary) siteSummary.Parent.Parent.Parent.Parent.Parent;
            
            if (configurationSummary.GetIrrigationFilterConfigAndObservable(siteSummary.ObservableFiltered).Value == null)
                return;
            var homeScreen = new HomeScreen(configurationSummary.GetIrrigationFilterConfigAndObservable(siteSummary.ObservableFiltered),
                _socketPicker);
            Navigation.PushModalAsync(homeScreen);
        }

        private void StartEvent()
        {
            _timer = new Timer(800);
            _timer.Elapsed += timer_Elapsed;
            _timer.Enabled = true;
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Enabled = false;
            var configList = _database.GetIrrigationConfigurationList();
            if (configList.Any()) UpdateSavedIrrigation();
            _timer.Enabled = true;
        }

        private void ButtonScanForControllers_OnPressed(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(SetupNewController);
        }

        private async void TapGestureRecognizer_SignOut(object sender, EventArgs e)
        {
            if (await DisplayAlert("Sign out",
                    "Are you sure you want to sign out?", "Accept",
                    "Cancel"))
            {
                _database.DeleteAllIrrigationConfigurationConnection();
                await _client.SignOutAsync();
            }
        }
        public async void SubscribeToNewController(IrrigationConfiguration irrigationConfiguration)
        {
            await _socketPicker.Subscribe(new List<IrrigationConfiguration> {irrigationConfiguration}, _client.User);
        }
    }
}