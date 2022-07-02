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

        public MainPage(FirebaseAuthClient client)
        {
            InitializeComponent();
            _notificationEvent = new NotificationEvent();
            _observableDict = new Dictionary<IrrigationConfiguration, ObservableIrrigation>();
            
            //_observableIrrigation = new ObservableIrrigation();
            
            _database = new DatabaseController();
            _socketPicker = new SocketPicker(new FirebaseManager(), _observableDict);
            _authenticationScreen = new AuthenticationScreen(client);
            
            client.AuthStateChanged += ClientOnAuthStateChanged;
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
                    PopulateSavedControllers(configList);
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

        private void PopulateSavedControllers(List<IrrigationConfiguration> irrigationConfigurationList)
        {
            foreach (var configuration in irrigationConfigurationList)
            {
                _observableDict.Add(configuration, new ObservableIrrigation());
                
                var viewSite = ScrollViewSite.Children.FirstOrDefault(x =>
                    x.AutomationId == configuration.Id.ToString());
                if (viewSite != null)
                {
                    var viewScheduleStatus = (ViewIrrigationConfigurationSummary)viewSite;
                    viewScheduleStatus.IrrigationConfiguration.Path = configuration.Path;
                }
                else
                {
                    var viewSiteSummary = new ViewIrrigationConfigurationSummary(configuration);
                    viewSiteSummary.GetTapGestureRecognizer().Tapped += OnTapped_HomeScreen;
                    ScrollViewSite.Children.Add(viewSiteSummary);
                }
            }
        }

        private void OnTapped_HomeScreen(object sender, EventArgs e)
        {
            var stackLayoutGesture = (StackLayout) sender;
            var configList = _database.GetControllerConfigurationList();
            var config = configList.First(x => x.Id.ToString() == stackLayoutGesture.AutomationId);
            _socketPicker.TargetedIrrigation = config;
           // var observableSiteIrrigation = new ObservableSiteFilteredIrrigation(_observableIrrigation, config.ControllerPairs.First().Value.ToList()); //Edit to have selectible sites from diffrent controllers
           // var homeScreen = new HomeScreen(observableSiteIrrigation, _socketPicker);
           // Navigation.PushModalAsync(homeScreen);
        }

        /*
        
        private void SetEvents()
        {
            _observableIrrigation.SiteList.CollectionChanged += PopulateSiteEvent;
            StartEvent();
        }

        private void PopulateControllers()
        {
            _controllerList = new DatabaseController().GetControllerConnectionList();
            ControllerPicker.Items.Clear();
            var selectedController = new DatabaseController().GetControllerConnectionSelection();
            for (var i = 0; i < _controllerList.Count; i++)
            {
                ControllerPicker.Items.Add(string.IsNullOrEmpty(_controllerList[i].Name)
                    ? "Name is missing"
                    : _controllerList[i].Name);
                if (_controllerList[i].ID == selectedController.ID)
                    ControllerPicker.SelectedIndex = i;
            }

            BtnDeleteController.IsEnabled = ControllerPicker.Items.Count > 0;
            BtnEditController.IsEnabled = ControllerPicker.Items.Count > 0;
        }

        

        private void BtnAddSubController_OnPressed(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new SubControllerUpdate(_socketPicker));
        }

        private void PopulateSiteEvent(object sender, NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(PopulateSite);
        }

        private void PopulateSite()
        {
            ScreenCleanupForSite();
            try
            {
                if (_observableIrrigation.SiteList.Contains(null))
                {
                    BtnAddSite.IsEnabled = false;
                    return;
                }

                _timer.Interval = 10000; //refresh every 1 seconds instead of every 0.3 sec
                BtnAddSite.IsEnabled = true;

                if (_observableIrrigation.SiteList.Any())
                {
                    foreach (var site in _observableIrrigation.SiteList)
                    {
                        var viewSite = ScrollViewSite.Children.FirstOrDefault(x =>
                            x.AutomationId == site.ID);
                        if (viewSite != null)
                        {
                            var viewScheduleStatus = (ViewSiteSummary)viewSite;
                            viewScheduleStatus.Site.NAME = site.NAME;
                            viewScheduleStatus.Site.Description = site.Description;
                        }
                        else
                        {
                            var viewSiteSummary = new ViewSiteSummary(site);
                            viewSiteSummary.GetTapGestureRecognizer().Tapped += ViewMainPage_Tapped;
                            ScrollViewSite.Children.Add(viewSiteSummary);
                        }
                    }

                    SetSelectedSite();
                }
                else
                {
                    if (ScrollViewSite.Children.Count == 0)
                        ScrollViewSite.Children.Add(new ViewEmptySchedule("No Sites Here"));
                }

                PopulateSiteDetail();
            }

            catch (Exception e)
            {
                ScrollViewSite.Children.Add(new ViewException(e));
            }
        }

        private void PopulateSiteDetail()
        {
            if (_observableIrrigation.LoadedAllData())
            {
                foreach (var viewSite in ScrollViewSite.Children)
                {
                    var viewScheduleStatus = (ViewSiteSummary)viewSite;

                    viewScheduleStatus.sensor = _observableIrrigation.SensorList.FirstOrDefault(x =>
                        viewScheduleStatus.Site.Attachments.Contains(x?.ID) && x?.TYPE == "Pressure Sensor");


                    viewScheduleStatus.Schedules = _observableIrrigation.ScheduleList.ToList();
                    viewScheduleStatus.CustomSchedules =
                        _observableIrrigation.CustomScheduleList.ToList();
                    viewScheduleStatus.Equipments = _observableIrrigation.EquipmentList.ToList();
                    viewScheduleStatus.ManualSchedules =
                        _observableIrrigation.ManualScheduleList.ToList();
                    viewScheduleStatus.Populate();
                }

                var selectedSiteId = new DatabaseController().GetControllerConnectionSelection().SiteSelectedId;
                if (!string.IsNullOrEmpty(selectedSiteId) && _loadedHomeScreen == false &&
                    _observableIrrigation.SiteList.Count == 1)
                {
                    _loadedHomeScreen = true;
                    StartHomePage(selectedSiteId);
                }
            }
        }

        private string SetSelectedSite()
        {
            var connection = new DatabaseController().GetControllerConnectionSelection();

            try
            {
                if (_observableIrrigation.SiteList.Contains(null))
                    return null;
                if (string.IsNullOrEmpty(connection.SiteSelectedId))
                {
                    var site = _observableIrrigation.SiteList.First();
                    connection.SiteSelectedId = site.ID;
                    new DatabaseController().UpdateControllerConnection(connection);
                }
            }
            catch
            {
                //check this
            }

            foreach (var view in ScrollViewSite.Children)
                try
                {
                    var siteView = (ViewSiteSummary)view;
                    siteView.SetBackgroundColor(connection.SiteSelectedId == siteView.AutomationId
                        ? Color.Aquamarine
                        : Color.White);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            return connection.SiteSelectedId;
        }

        private void ScreenCleanupForSite()
        {
            try
            {
                if (!_observableIrrigation.SiteList.Contains(null))
                {
                    var itemsThatAreOnDisplay = _observableIrrigation.SiteList.Select(x => x?.ID).ToList();

                    if (itemsThatAreOnDisplay.Count == 0)
                        itemsThatAreOnDisplay.Add(new ViewEmptySchedule(string.Empty).AutomationId);

                    for (var index = 0; index < ScrollViewSite.Children.Count; index++)
                    {
                        var existingItems = itemsThatAreOnDisplay.FirstOrDefault(x =>
                            x == ScrollViewSite.Children[index].AutomationId);
                        if (existingItems != null) continue;
                        ScrollViewSite.Children.RemoveAt(index);
                        index--;
                    }
                }
                else
                {
                    if (ScrollViewSite.Children.Count == 1 && ScrollViewSite.Children.First().AutomationId ==
                        "ActivityIndicatorSiteLoading")
                        return;
                    ScrollViewSite.Children.Clear();
                    var loadingIcon = new ActivityIndicator
                    {
                        AutomationId = "ActivityIndicatorSiteLoading",
                        HorizontalOptions = LayoutOptions.Center,
                        IsEnabled = true,
                        IsRunning = true,
                        IsVisible = true,
                        VerticalOptions = LayoutOptions.Center
                    };
                    ScrollViewSite.Children.Add(loadingIcon);
                }
            }
            catch
            {
                // ignored
            }
        }

        private void ViewMainPage_Tapped(object sender, EventArgs e)
        {
            var viewSite = (StackLayout)sender;
            var site = _observableIrrigation.SiteList.First(x => x?.ID == viewSite.AutomationId);
            Device.BeginInvokeOnMainThread(async () =>
            {
                var action = await DisplayActionSheet("You have selected " + site.NAME,
                    "Cancel", null, "Select", "Update", "Delete");
                if (action == null) return;

                if (action == "Update")
                {
                    var equipmentList = _observableIrrigation.EquipmentList.ToList();
                    var equipments = _observableIrrigation.SiteList.Aggregate(equipmentList,
                        (current, sites) => current.Where(x => !sites.Attachments.Contains(x?.ID)).ToList());
                    equipments.AddRange(
                        _observableIrrigation.EquipmentList.Where(x => site.Attachments.Contains(x?.ID)));

                    var sensorList = _observableIrrigation.SensorList.ToList();
                    var sensors = _observableIrrigation.SiteList.Aggregate(sensorList,
                        (current, sites) => current.Where(x => !sites.Attachments.Contains(x?.ID)).ToList());
                    sensors.AddRange(_observableIrrigation.SensorList.Where(x => site.Attachments.Contains(x?.ID)));

                    await Navigation.PushModalAsync(new SiteUpdate(sensors, equipments, _socketPicker, site));
                }
                else if (action == "Select")
                {
                    var controller = _controllerList[ControllerPicker.SelectedIndex];
                    controller.SiteSelectedId = site.ID;
                    new DatabaseController().UpdateControllerConnection(controller);
                    StartHomePage(SetSelectedSite());
                }
                else if (action == "Delete")
                {
                    if (await DisplayAlert("Are you sure?",
                            "Confirm to delete " + site.NAME, "Delete",
                            "Cancel"))
                    {
                        site.DeleteAwaiting = true;
                        await _socketPicker.SendCommand(site);
                    }
                }
            });
        }

        

        private async void BtnAddController_OnPressed(object sender, EventArgs e)
        {
            var connectionScreen = new ExistingController(false, _notificationEvent, _socketPicker.BluetoothManager());
            connectionScreen.GetUpdateButton().Tapped += BtnUpdateController_OnPressed;
            await Navigation.PushModalAsync(connectionScreen);
        }

        private void BtnEditController_OnPressed(object sender, EventArgs e)
        {
            var connectionScreen = new ExistingController(false, _notificationEvent, _socketPicker.BluetoothManager(),
                new DatabaseController().GetControllerConnectionSelection());
            connectionScreen.GetUpdateButton().Tapped += BtnUpdateController_OnPressed;
            Navigation.PushModalAsync(connectionScreen);
        }

        private async void BtnDeleteController_OnPressed_(object sender, EventArgs e)
        {
            var result = await DisplayAlert("Are you Sure?",
                "Confirm to delete " + _controllerList[ControllerPicker.SelectedIndex].Name, "Delete", "Cancel");
            if (!result) return;
            new DatabaseController().DeleteControllerConnection(_controllerList[ControllerPicker.SelectedIndex]);
            PopulateControllers();
        }

        private void StartHomePage(string siteId)
        {
            try
            {
                var observableSiteIrrigation = new ObservableSiteIrrigation(_observableIrrigation,
                    _observableIrrigation.SiteList.First(x => x.ID == siteId));
                _homeScreen = new HomeScreen(_observableIrrigation, observableSiteIrrigation, _socketPicker);
                _homeScreen.GetSiteButton().Pressed += BtnHomeScreenSite_OnPressed;
                Navigation.PushModalAsync(_homeScreen);
            }
            catch
            {
                // ignored
            }
        }

        private void PopulateSubControllerEvent(object sender, NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(PopulateSubController);
        }

        private void PopulateSubController()
        {
            ScreenCleanupForSubController();

            try
            {
                if (_observableIrrigation.SubControllerList.Contains(null)) return;
                BtnAddSubController.IsEnabled = true;
                if (_observableIrrigation.SubControllerList.Any())
                    foreach (var subController in _observableIrrigation.SubControllerList.ToList())
                    {
                        var viewSubControllerChild = ScrollViewSubController.Children.FirstOrDefault(x =>
                            x.AutomationId == subController.ID);
                        if (viewSubControllerChild != null)
                        {
                            var viewSubController = (ViewSubControllerSummary)viewSubControllerChild;
                            viewSubController.SubController.NAME = subController.NAME;
                            viewSubController.SubController.IpAdress = subController.IpAdress;
                            viewSubController.SubController.BTmac = subController.IpAdress;
                            viewSubController.SubController.Port = subController.Port;
                            viewSubController.Populate();
                        }
                        else
                        {
                            var viewSubControllerSummary = new ViewSubControllerSummary(subController);
                            ScrollViewSubController.Children.Add(viewSubControllerSummary);
                            viewSubControllerSummary.GetTapGestureRecognizer().Tapped += ViewSubControllerScreen_Tapped;
                        }
                    }
                else
                    ScrollViewSubController.Children.Add(new ViewEmptySchedule("No other controller Here"));
            }
            catch
            {
                // ignored
            }
        }

        private void ScreenCleanupForSubController()
        {
            try
            {
                if (!_observableIrrigation.SubControllerList.Contains(null))
                {
                    var itemsThatAreOnDisplay = _observableIrrigation.SubControllerList.Select(x => x?.ID).ToList();

                    if (itemsThatAreOnDisplay.Count == 0)
                        itemsThatAreOnDisplay.Add(new ViewEmptySchedule(string.Empty).AutomationId);

                    for (var index = 0; index < ScrollViewSubController.Children.Count; index++)
                    {
                        var existingItems = itemsThatAreOnDisplay.FirstOrDefault(x =>
                            x == ScrollViewSubController.Children[index].AutomationId);
                        if (existingItems != null) continue;
                        ScrollViewSubController.Children.RemoveAt(index);
                        index--;
                    }
                }
                else
                {
                    if (ScrollViewSubController.Children.Count == 1 &&
                        ScrollViewSubController.Children.First().AutomationId ==
                        "ActivityIndicatorSiteLoading")
                        return;
                    ScrollViewSubController.Children.Clear();
                    var loadingIcon = new ActivityIndicator
                    {
                        AutomationId = "ActivityIndicatorSiteLoading",
                        HorizontalOptions = LayoutOptions.Center,
                        IsEnabled = true,
                        IsRunning = true,
                        IsVisible = true,
                        VerticalOptions = LayoutOptions.Center
                    };
                    ScrollViewSubController.Children.Add(loadingIcon);
                }
            }
            catch
            {
                // ignored
            }
        }


        private void ViewSubControllerScreen_Tapped(object sender, EventArgs e)
        {
            var viewSubController = (StackLayout)sender;
            var subController =
                _observableIrrigation.SubControllerList.First(x => x?.ID == viewSubController.AutomationId);
            Device.BeginInvokeOnMainThread(async () =>
            {
                var action = await DisplayActionSheet("You have selected " + subController.NAME,
                    "Cancel", null, "Update", "Delete");
                if (action == null) return;

                if (action == "Update")
                    await Navigation.PushModalAsync(new SubControllerUpdate(_socketPicker, subController));
                else if (action == "Delete")
                    if (await DisplayAlert("Are you sure?",
                            "Confirm to delete " + subController.NAME, "Delete",
                            "Cancel"))
                    {
                        subController.DeleteAwaiting = true;
                        await _socketPicker.SendCommand(subController);
                    }
            });
        }


        private async void BtnHomeScreenSite_OnPressed(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private void BtnUpdateController_OnPressed(object sender, EventArgs e)
        {
            PopulateControllers();
        }

        private void ConnectionPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (ControllerPicker.SelectedIndex == -1) return;
            _homeScreen = null;
            _timer.Interval = 300;
        }


        private void StartEvent()
        {
            _timer.Elapsed += timer_Elapsed;
            _timer.Enabled = true;
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PopulateSiteEvent(null, null);
            PopulateSubControllerEvent(null, null);
        }
    */
        private void ButtonScanForControllers_OnPressed(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(SetupNewController);
        }
    }
}