using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pump.Database;
using Pump.Droid.Database.Table;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.Layout.Views;
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

        public SiteScreen(ObservableIrrigation observableIrrigation)
        {
            _observableIrrigation = observableIrrigation;
            InitializeComponent();
            PopulateControllers();
            _observableIrrigation.SensorList.CollectionChanged += PopulateSensorAndEquipmentEvent;
            _observableIrrigation.EquipmentList.CollectionChanged += PopulateSensorAndEquipmentEvent;
            _observableIrrigation.SensorList.CollectionChanged += PopulateSiteEvent;
            _observableIrrigation.EquipmentList.CollectionChanged += PopulateSiteEvent;
            _observableIrrigation.ScheduleList.CollectionChanged += PopulateSiteEvent;
            _observableIrrigation.CustomScheduleList.CollectionChanged += PopulateSiteEvent;
            _observableIrrigation.ManualScheduleList.CollectionChanged += PopulateSiteEvent;
            _observableIrrigation.SiteList.CollectionChanged += PopulateSiteEvent;
            ControllerPicker.SelectedIndexChanged += ConnectionPicker_OnSelectedIndexChanged;
            if (!string.IsNullOrEmpty(new DatabaseController().GetControllerConnectionSelection().SiteSelectedId))
                StartHomePage();
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
                else
                {
                    if (await DisplayAlert("Are you sure?",
                        "Confirm to delete " + site.NAME, "Delete",
                        "Cancel"))
                        await Task.Run(() => new Authentication().DeleteSite(site));
                }
            });
        }

        private void BtnAddController_OnPressed(object sender, EventArgs e)
        {
            var connectionScreen = new AddExistingController(false);
            connectionScreen.GetUpdateButton().Clicked += BtnUpdateController_OnPressed;
            Navigation.PushModalAsync(connectionScreen);
        }

        private void BtnEditController_OnPressed(object sender, EventArgs e)
        {
            var connectionScreen = new AddExistingController(false, new DatabaseController().GetControllerConnectionSelection());
            connectionScreen.GetUpdateButton().Clicked += BtnUpdateController_OnPressed;
            Navigation.PushModalAsync(connectionScreen);
        }

        private void BtnDeleteController_OnPressed_(object sender, EventArgs e)
        {
            new DatabaseController().DeleteControllerConnection(_controllerList[ControllerPicker.SelectedIndex]);
            PopulateControllers();
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

        
    }
}