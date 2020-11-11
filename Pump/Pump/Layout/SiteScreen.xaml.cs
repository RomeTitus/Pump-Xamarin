using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private List<PumpConnection> ControllerList = new List<PumpConnection>();
        private readonly ObservableCollection<Equipment> _equipmentList;
        private readonly ObservableCollection<Alive> _aliveList;
        private readonly ObservableCollection<Sensor> _sensorList;
        private readonly ObservableCollection<ManualSchedule> _manualScheduleList;
        private readonly ObservableCollection<Schedule> _scheduleList;
        private readonly ObservableCollection<CustomSchedule> _customScheduleList;
        private readonly ObservableCollection<Site> _siteList;
        private readonly ObservableCollection<SubController> _subController;

        public SiteScreen(ObservableCollection<Equipment> equipmentList, ObservableCollection<Sensor> sensorList,
            ObservableCollection<ManualSchedule> manualScheduleList, ObservableCollection<Schedule> scheduleList,
            ObservableCollection<CustomSchedule> customScheduleList, ObservableCollection<Site> siteList, ObservableCollection<Alive> aliveList,
            ObservableCollection<SubController> subController)
        {
            _equipmentList = equipmentList;
            _sensorList = sensorList;
            _manualScheduleList = manualScheduleList;
            _scheduleList = scheduleList;
            _customScheduleList = customScheduleList;
            _siteList = siteList;
            _aliveList = aliveList;
            _subController = subController;
            InitializeComponent();
            PopulateControllers();
            _sensorList.CollectionChanged += PopulateSensorAndEquipmentEvent;
            _equipmentList.CollectionChanged += PopulateSensorAndEquipmentEvent;
            _siteList.CollectionChanged += PopulateSiteEvent;
            
            if (!string.IsNullOrEmpty(new DatabaseController().GetControllerConnectionSelection().SiteSelectedId))
                StartHomePage();
        }

        private void PopulateControllers()
        {
            ControllerList = new DatabaseController().GetControllerConnectionList();
            ControllerPicker.Items.Clear();
            var selectedController = new DatabaseController().GetControllerConnectionSelection();
            for (var i = 0; i < ControllerList.Count; i++)
            {
                ControllerPicker.Items.Add(string.IsNullOrEmpty(ControllerList[i].Name) ? "Name is missing" : ControllerList[i].Name);
                if (ControllerList[i].ID == selectedController.ID)
                    ControllerPicker.SelectedIndex = i;
            }

            BtnDeleteController.IsEnabled = ControllerPicker.Items.Count > 1;
            BtnEditController.IsEnabled = ControllerPicker.Items.Count > 1;
        }

        private void BtnAddSite_OnPressed(object sender, EventArgs e)
        {
            var equipmentList = _equipmentList.ToList();
            var equipments = _siteList.Aggregate(equipmentList,
                (current, site) => current.Where(x => site.Attachments.Contains(x.ID)).ToList());

            var sensorList = _sensorList.ToList();
            var sensors = _siteList.Aggregate(sensorList,
                (current, site) => current.Where(x => site.Attachments.Contains(x.ID)).ToList());


            Navigation.PushModalAsync(new SiteUpdate(sensors, equipments));
        }

        
        private void PopulateSensorAndEquipmentEvent(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (!_sensorList.Contains(null) && !_equipmentList.Contains(null))
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
                if (!_siteList.Contains(null) &&  _siteList.Any())
                    foreach (var site in _siteList)
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
                            var viewSiteSummary = new ViewSiteSummary(site);
                            ScrollViewSite.Children.Add(viewSiteSummary);
                            viewSiteSummary.GetTapGestureRecognizer().Tapped += ViewSiteScreen_Tapped;
                        }
                    }
                else
                {
                    ScrollViewSite.Children.Add(new ViewEmptySchedule("No Sites Here"));
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
            if(_siteList.Contains(null))
                return;
            var connection = new DatabaseController().GetControllerConnectionSelection();
            if (string.IsNullOrEmpty(connection.SiteSelectedId))
            {
                var site = _siteList.First();
                connection.SiteSelectedId = site.ID;
                new DatabaseController().UpdateControllerConnection(connection);
            }

            foreach (var view in ScrollViewSite.Children)
            {
                var siteView = (ViewSiteSummary) view;
                siteView.setBackgroundColor(connection.SiteSelectedId == siteView.AutomationId ? Color.Aquamarine : Color.White);
            }
        }

        private void ScreenCleanupForSite()
        {
            try
            {
                if (!_siteList.Contains(null))
                {

                    var itemsThatAreOnDisplay = _siteList.Select(x => x.ID).ToList();
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
            var site = _siteList.First(x => x.ID == viewSite.AutomationId);
            Device.BeginInvokeOnMainThread(async () =>
            {
                var action = await DisplayActionSheet("You have selected " + site.NAME,
                    "Cancel", null, "Select", "Update", "Delete");
                if (action == null) return;

                if (action == "Update")
                {
                    var equipmentList = _equipmentList.ToList();
                    var equipments = _siteList.Aggregate(equipmentList,
                        (current, sites) => current.Where(x => sites.Attachments.Contains(x.ID)).ToList());

                    var sensorList = _sensorList.ToList();
                    var sensors = _siteList.Aggregate(sensorList,
                        (current, sites) => current.Where(x => sites.Attachments.Contains(x.ID)).ToList());

                    await Navigation.PushModalAsync(new SiteUpdate(sensors, equipments, site));
                }
                else if(action == "Select")
                {
                    var controller = ControllerList[ControllerPicker.SelectedIndex];
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

        private void ControllerPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void BtnAddController_OnPressed(object sender, EventArgs e)
        {
        }

        private void BtnDeleteController_OnPressed_(object sender, EventArgs e)
        {
        }

        public Picker GetControllerPicker()
        {
            return ControllerPicker;
        }

        private void StartHomePage()
        {
            var homeScreen = new HomeScreen(_equipmentList, _sensorList, _manualScheduleList, _scheduleList, _customScheduleList,
                _siteList, _aliveList, _subController);
            Navigation.PushModalAsync(homeScreen);
           
        }
    }
}