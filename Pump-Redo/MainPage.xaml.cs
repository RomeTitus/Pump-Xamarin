﻿using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Auth;
using Pump.Class;
using Pump.Database;
using Pump.Database.Table;
using Pump.IrrigationController;
using Pump.Layout;
using Pump.Layout.Views;
using Pump.SocketController;
using Pump.SocketController.Firebase;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Timers;
using Pump.Layout.Dashboard;

namespace Pump
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage
    {
        private readonly AuthenticationScreen _authenticationScreen;
        private readonly NotificationEvent _notificationEvent;
        private readonly Dictionary<IrrigationConfiguration, ObservableIrrigation> _observableDict;
        private readonly SocketPicker _socketPicker;
        private readonly DatabaseController _database;
        private Timer _timer;

        public MainPage(FirebaseAuthClient client)
        {
            InitializeComponent();
            _notificationEvent = new NotificationEvent();
            _observableDict = new Dictionary<IrrigationConfiguration, ObservableIrrigation>();
            _database = new DatabaseController();
            _socketPicker = new SocketPicker(new FirebaseManager(), _observableDict);
            _authenticationScreen = new AuthenticationScreen(client);
            
            client.AuthStateChanged += ClientOnAuthStateChanged;
            StartEvent();
        }

        private async void ClientOnAuthStateChanged(object sender, UserEventArgs e)
        {
            if (e.User == null)
            {
                await Navigation.PushModalAsync(_authenticationScreen);
                _authenticationScreen.IsDisplayed = true;
            }
            else
            {
                if (_authenticationScreen.IsDisplayed)
                    _authenticationScreen.ClosePage();
                _authenticationScreen.IsDisplayed = false;

                var configList = _database.GetControllerConfigurationList();
                if (!configList.Any())
                    Device.BeginInvokeOnMainThread(SetupNewController);
                else
                {
                    PopulateSavedIrrigation(configList);
                    await _socketPicker.Subscribe(configList, e.User);
                }
                    
            }
        }
        
        private void SetupNewController()
        {
            var connectionScreen = new ScanBluetooth( _notificationEvent, _socketPicker.BluetoothManager(), _database);
            if (Navigation.ModalStack.All(x => x.GetType() != typeof(ScanBluetooth)))
                Navigation.PushModalAsync(connectionScreen);
        }

        private void PopulateSavedIrrigation(List<IrrigationConfiguration> irrigationConfigurationList)
        {
            foreach (var configuration in irrigationConfigurationList)
            {
                if (_observableDict.Keys.Any())
                {
                    if(_observableDict.Keys.FirstOrDefault(x => x.Path == configuration.Path) == null)
                        _observableDict.Add(configuration, new ObservableIrrigation());
                }
                else
                    _observableDict.Add(configuration, new ObservableIrrigation());
                
                    

                var viewSite = ScrollViewSite.Children.FirstOrDefault(x =>
                    x.AutomationId == configuration.Id.ToString());
                
                if (viewSite == null)
                {
                    var viewSiteSummary = new ViewIrrigationConfigurationSummary(_observableDict.First(x => x.Key.Id == configuration.Id));
                    viewSiteSummary.GetTapGestureRecognizer().Tapped += OnTapped_HomeScreen;
                    ScrollViewSite.Children.Add(viewSiteSummary);
                }
            }
        }

        private void UpdateSavedIrrigation()
        {
            foreach (var view in ScrollViewSite.Children.Where(x => x is ViewIrrigationConfigurationSummary))
            {
                var viewConfig = (ViewIrrigationConfigurationSummary)view;
                viewConfig.Populate();
            }
        }

        private void OnTapped_HomeScreen(object sender, EventArgs e)
        {
            var stackLayoutGesture = (StackLayout) sender;
            var keyPairIrrigation = _observableDict.First(x => x.Key.Id.ToString() == stackLayoutGesture.AutomationId);
            
            _socketPicker.TargetedIrrigation = keyPairIrrigation.Key;

            var test = new ObservableFilteredIrrigation(keyPairIrrigation.Value, null);

            //var homeScreen = new HomeScreen(keyPairIrrigation.Value, _socketPicker);
            //Navigation.PushModalAsync(homeScreen);
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
            var configList = _database.GetControllerConfigurationList();
            if (configList.Any())
            {
                UpdateSavedIrrigation();
            }
            _timer.Enabled = true;
        }
        
        private void ButtonScanForControllers_OnPressed(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(SetupNewController);
        }
    }
}