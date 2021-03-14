using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pump.Class;
using Pump.Database;
using Pump.Droid.Database.Table;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Pump.SocketController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SiteScreen : ContentPage
    {
        private List<PumpConnection> _controllerList = new List<PumpConnection>();
        private readonly ObservableIrrigation _observableIrrigation;
        private HomeScreen _homeScreen;
        private readonly ControllerEvent _controllerEvent;
        private readonly NotificationEvent _notificationEvent;

        public SiteScreen()
        {
            InitializeComponent();
            _observableIrrigation = new ObservableIrrigation();
            SetEvents();
            
            _controllerEvent = new ControllerEvent();
            _controllerEvent.OnUpdateStatus += NewSelectedController;

            _notificationEvent = new NotificationEvent();
            _notificationEvent.OnNotificationUpdate += _notificationEvent_OnNotificationConnectionUpdate;

            
            var socketPicker = new SocketPicker(_observableIrrigation, _notificationEvent);
            ControllerPicker.SelectedIndexChanged += socketPicker.ConnectionPicker_OnSelectedIndexChanged;

            PopulateControllers();


            var dbController = new DatabaseController();
            if (!dbController.GetControllerConnectionList().Any())
                SetupNewController();
            else if (!string.IsNullOrEmpty(dbController.GetControllerConnectionSelection().SiteSelectedId))
                StartHomePage();
        }

        private async void _notificationEvent_OnNotificationConnectionUpdate(object sender, NotificationEventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () => {
                await DisplayAlert(e.Header, e.Main, e.ButtonText);
            });
            
        }

        private void SetEvents()
        {
            _observableIrrigation.SensorList.CollectionChanged += PopulateSensorAndEquipmentEvent;
            _observableIrrigation.EquipmentList.CollectionChanged += PopulateSensorAndEquipmentEvent;
            _observableIrrigation.SensorList.CollectionChanged += PopulateSiteEvent;
            _observableIrrigation.EquipmentList.CollectionChanged += PopulateSiteEvent;
            _observableIrrigation.ScheduleList.CollectionChanged += PopulateSiteEvent;
            _observableIrrigation.CustomScheduleList.CollectionChanged += PopulateSiteEvent;
            _observableIrrigation.ManualScheduleList.CollectionChanged += PopulateSiteEvent;
            _observableIrrigation.SiteList.CollectionChanged += PopulateSiteEvent;
            _observableIrrigation.SubControllerList.CollectionChanged += PopulateSubControllerEvent;
            ControllerPicker.SelectedIndexChanged += ConnectionPicker_OnSelectedIndexChanged;
        }

        private void PopulateControllers()
        {
            _controllerList = new DatabaseController().GetControllerConnectionList();
            ControllerPicker.Items.Clear();
            var selectedController = new DatabaseController().GetControllerConnectionSelection();
            for (var i = 0; i < _controllerList.Count; i++)
            {
                ControllerPicker.Items.Add(string.IsNullOrEmpty(_controllerList[i].Name) ? "Name is missing" : _controllerList[i].Name);
                if (_controllerList[i].ID == selectedController.ID)
                    ControllerPicker.SelectedIndex = i;
            }

            BtnDeleteController.IsEnabled = ControllerPicker.Items.Count > 0;
            BtnEditController.IsEnabled = ControllerPicker.Items.Count > 0;
        }

        private void BtnAddSite_OnPressed(object sender, EventArgs e)
        {
            var equipmentList = _observableIrrigation.EquipmentList.ToList();
            var equipments = _observableIrrigation.SiteList.Aggregate(equipmentList,
                (current, site) => current.Where(x => !site.Attachments.Contains(x.ID)).ToList());

            var sensorList = _observableIrrigation.SensorList.ToList();
            var sensors = _observableIrrigation.SiteList.Aggregate(sensorList,
                (current, site) => current.Where(x => !site.Attachments.Contains(x.ID)).ToList());


            Navigation.PushModalAsync(new SiteUpdate(sensors, equipments));
        }
        private void BtnAddSubController_OnPressed(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new SubControllerUpdate());
        }

        private void PopulateSensorAndEquipmentEvent(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (!_observableIrrigation.SensorList.Contains(null) && !_observableIrrigation.EquipmentList.Contains(null))
                    BtnAddSite.IsEnabled = true;
                else
                    BtnAddSite.IsEnabled = false;
            });
        }

        private void PopulateSiteEvent(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(PopulateSite);
        }

        private void PopulateSite()
        {
            ScreenCleanupForSite();
            try
            {
                if (!_observableIrrigation.SiteList.Contains(null) && !_observableIrrigation.SensorList.Contains(null) && !_observableIrrigation.ScheduleList.Contains(null) &&
                    !_observableIrrigation.CustomScheduleList.Contains(null) && !_observableIrrigation.ManualScheduleList.Contains(null) && _observableIrrigation.SiteList.Any())
                {
                    if (_observableIrrigation.SiteList.Any())
                        foreach (var site in _observableIrrigation.SiteList)
                        {
                            var viewSite = ScrollViewSite.Children.FirstOrDefault(x =>
                                x.AutomationId == site.ID);
                            if (viewSite != null)
                            {
                                var viewScheduleStatus = (ViewSiteSummary)viewSite;
                                viewScheduleStatus._site.NAME = site.NAME;
                                viewScheduleStatus._site.Description = site.Description;
                                viewScheduleStatus.Populate();
                            }
                            else
                            {
                                var viewSiteSummary = new ViewSiteSummary(site, _observableIrrigation.SensorList.FirstOrDefault(x => site.Attachments.Contains(x.ID)), _observableIrrigation.ScheduleList.ToList(), _observableIrrigation.CustomScheduleList.ToList(), _observableIrrigation.EquipmentList.ToList(), _observableIrrigation.ManualScheduleList.ToList());
                                ScrollViewSite.Children.Add(viewSiteSummary);
                                viewSiteSummary.GetTapGestureRecognizer().Tapped += ViewSiteScreen_Tapped;
                            }
                        }
                    else
                    {
                        ScrollViewSite.Children.Add(new ViewEmptySchedule("No Sites Here"));
                    }
                }
            }
            catch
            {
                // ignored
            }

            SetSelectedSite();
        }

        private void SetSelectedSite()
        {
            var connection = new DatabaseController().GetControllerConnectionSelection();

            try
            {
                if (_observableIrrigation.SiteList.Contains(null))
                    return;
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
            {
                try
                {
                    var siteView = (ViewSiteSummary)view;
                    siteView.SetBackgroundColor(connection.SiteSelectedId == siteView.AutomationId ? Color.Aquamarine : Color.White);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
            }
        }

        private void ScreenCleanupForSite()
        {
            try
            {
                if (!_observableIrrigation.SiteList.Contains(null))
                {

                    var itemsThatAreOnDisplay = _observableIrrigation.SiteList.Select(x => x.ID).ToList();
                    if (itemsThatAreOnDisplay.Count == 0)
                        itemsThatAreOnDisplay.Add(new ViewEmptySchedule(string.Empty).ID);

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

        private void ViewSiteScreen_Tapped(object sender, EventArgs e)
        {
            var viewSite = (StackLayout)sender;
            var site = _observableIrrigation.SiteList.First(x => x.ID == viewSite.AutomationId);
            Device.BeginInvokeOnMainThread(async () =>
            {
                var action = await DisplayActionSheet("You have selected " + site.NAME,
                    "Cancel", null, "Select", "Update", "Delete");
                if (action == null) return;

                if (action == "Update")
                {

                    var equipmentList = _observableIrrigation.EquipmentList.ToList();
                    var equipments = _observableIrrigation.SiteList.Aggregate(equipmentList,
                        (current, sites) => current.Where(x => !sites.Attachments.Contains(x.ID)).ToList());
                    equipments.AddRange(_observableIrrigation.EquipmentList.Where(x => site.Attachments.Contains(x.ID)));

                    var sensorList = _observableIrrigation.SensorList.ToList();
                    var sensors = _observableIrrigation.SiteList.Aggregate(sensorList,
                        (current, sites) => current.Where(x => !sites.Attachments.Contains(x.ID)).ToList());
                    sensors.AddRange(_observableIrrigation.SensorList.Where(x => site.Attachments.Contains(x.ID)));

                    await Navigation.PushModalAsync(new SiteUpdate(sensors, equipments, site));
                }
                else if(action == "Select")
                {
                    var controller = _controllerList[ControllerPicker.SelectedIndex];
                    controller.SiteSelectedId = site.ID;
                    new DatabaseController().UpdateControllerConnection(controller);
                    SetSelectedSite();
                    StartHomePage();
                    
                }
                else if (action == "Delete")
                {
                    if (await DisplayAlert("Are you sure?",
                        "Confirm to delete " + site.NAME, "Delete",
                        "Cancel"))
                        await Task.Run(() => new Authentication().DeleteSite(site));
                }
            });
        }

        private void SetupNewController()
        {
            var connectionScreen = new ExistingController(true, _controllerEvent);
            connectionScreen.GetUpdateButton().Clicked += BtnUpdateController_OnPressed;
            Navigation.PushModalAsync(connectionScreen);
        }

        private async void BtnAddController_OnPressed(object sender, EventArgs e)
        {
            var connectionScreen = new ExistingController(false, _controllerEvent);
            connectionScreen.GetUpdateButton().Clicked += BtnUpdateController_OnPressed;
            await Navigation.PushModalAsync(connectionScreen);
        }

        private void BtnEditController_OnPressed(object sender, EventArgs e)
        {
            var connectionScreen = new ExistingController(false, _controllerEvent, new DatabaseController().GetControllerConnectionSelection());
            connectionScreen.GetUpdateButton().Clicked += BtnUpdateController_OnPressed;
            Navigation.PushModalAsync(connectionScreen);
        }

        private async void BtnDeleteController_OnPressed_(object sender, EventArgs e)
        {
            var Result = await DisplayAlert("Are you Sure?", "Confirm to delete " + _controllerList[ControllerPicker.SelectedIndex].Name, "Delete", "Cancel");
            if (Result)
            {
                new DatabaseController().DeleteControllerConnection(_controllerList[ControllerPicker.SelectedIndex]);
                PopulateControllers();
            }
            
        }

        public Picker GetControllerPicker()
        {
            return ControllerPicker;
        }

        private void StartHomePage()
        {
            try
            {
                _homeScreen = new HomeScreen(_observableIrrigation);
                _homeScreen.GetSiteButton().Pressed += BtnHomeScreenSite_OnPressed;
                Navigation.PushModalAsync(_homeScreen);

            }
            catch
            {

            }
        }

        private void PopulateSubControllerEvent(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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
                {
                    ScrollViewSubController.Children.Add(new ViewEmptySchedule("No other controller Here"));
                }
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
                    var itemsThatAreOnDisplay = _observableIrrigation.SubControllerList.Select(x => x.ID).ToList();
                    if (itemsThatAreOnDisplay.Count == 0)
                        itemsThatAreOnDisplay.Add(new ViewEmptySchedule(string.Empty).ID);
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
            var subController = _observableIrrigation.SubControllerList.First(x => x.ID == viewSubController.AutomationId);
            Device.BeginInvokeOnMainThread(async () =>
            {
                var action = await DisplayActionSheet("You have selected " + subController.NAME,
                    "Cancel", null, "Update", "Delete");
                if (action == null) return;

                if (action == "Update")
                {
                    await Navigation.PushModalAsync(new SubControllerUpdate(subController));
                }
                else if(action == "Delete")
                {
                    if (await DisplayAlert("Are you sure?",
                        "Confirm to delete " + subController.NAME, "Delete",
                        "Cancel")) return;
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
            if(ControllerPicker.SelectedIndex != -1)
                _homeScreen = null;
        }

        private void NewSelectedController(object sender, ControllerEventArgs e)
        {
            PopulateControllers();
        }

    }
}