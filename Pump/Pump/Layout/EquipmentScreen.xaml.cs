using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pump.Class;
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
    public partial class EquipmentScreen : ContentPage
    {
        private readonly ObservableIrrigation _observableIrrigation;
        private readonly PumpConnection _pumpConnection;

        public EquipmentScreen(ObservableIrrigation observableIrrigation)
        {
            _observableIrrigation = observableIrrigation;
            InitializeComponent();
            _pumpConnection = new DatabaseController().GetControllerConnectionSelection();
            new Thread(LoadScreens).Start();
        }
        private void LoadScreens()
        {
            var hasSubscribed = false;
            var equipmentHasRun = false;
            var sensorHasRun = false;
            while (!hasSubscribed)
            {
                try
                {
                    if (!_observableIrrigation.EquipmentList.Contains(null) && !_observableIrrigation.SensorList.Contains(null) && !_observableIrrigation.SubControllerList.Contains(null))
                    {
                        Device.InvokeOnMainThreadAsync(() =>
                        {
                            BtnAddEquipment.IsEnabled = true;
                            BtnAddSensor.IsEnabled = true;
                        });
                        hasSubscribed = true;
                    }
                    if (!_observableIrrigation.EquipmentList.Contains(null) && !equipmentHasRun)
                    {
                        equipmentHasRun = true;
                        _observableIrrigation.EquipmentList.CollectionChanged += PopulateEquipmentEvent;
                        Device.InvokeOnMainThreadAsync(PopulateEquipment);
                    }
                    if (!_observableIrrigation.SensorList.Contains(null) && !sensorHasRun)
                    {
                        sensorHasRun = true;
                        _observableIrrigation.SensorList.CollectionChanged += PopulateSensorEvent;
                        Device.InvokeOnMainThreadAsync(PopulateSensor);
                    }
                }
                catch
                {
                    // ignored
                }
                Thread.Sleep(100);
            }
        }

        private void PopulateEquipmentEvent(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(PopulateEquipment);
        }

        private void PopulateEquipment()
        {
            ScreenCleanupForEquipment();
            try
            {
                if(_observableIrrigation.EquipmentList.Contains(null))return;
                if (_observableIrrigation.EquipmentList.Any())
                    foreach (var equipment in _observableIrrigation.EquipmentList.Where(x => _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.ID)))
                    {
                        var viewEquipment = ScrollViewEquipment.Children.FirstOrDefault(x =>
                            x.AutomationId == equipment.ID);
                        if (viewEquipment != null)
                        {
                            var viewScheduleStatus = (ViewEquipmentSummary)viewEquipment;
                            viewScheduleStatus._equipment.NAME = equipment.NAME;
                            viewScheduleStatus._equipment.DirectOnlineGPIO = equipment.DirectOnlineGPIO;
                            viewScheduleStatus._equipment.GPIO = equipment.GPIO;
                            viewScheduleStatus._equipment.isPump = equipment.isPump;
                            viewScheduleStatus._equipment.AttachedSubController = equipment.AttachedSubController;
                            viewScheduleStatus.Populate();
                        }
                        else
                        {
                            var viewEquipmentSummary = new ViewEquipmentSummary(equipment);
                            ScrollViewEquipment.Children.Add(viewEquipmentSummary);
                            viewEquipmentSummary.GetTapGestureRecognizer().Tapped += ViewEquipmentScreen_Tapped;
                        }
                    }
                else
                {
                    ScrollViewEquipment.Children.Add(new ViewEmptySchedule("No Equipments Here"));
                }
            }
            catch
            {
                // ignored
            }
        }

        private void ScreenCleanupForEquipment()
        {
            try
            {
                if (!_observableIrrigation.EquipmentList.Contains(null))
                {

                    var itemsThatAreOnDisplay = _observableIrrigation.EquipmentList.Select(x => x.ID).ToList();
                    if (itemsThatAreOnDisplay.Count == 0)
                        itemsThatAreOnDisplay.Add(new ViewEmptySchedule(string.Empty).ID);

                    for (var index = 0; index < ScrollViewEquipment.Children.Count; index++)
                    {
                        var existingItems = itemsThatAreOnDisplay.FirstOrDefault(x =>
                            x == ScrollViewEquipment.Children[index].AutomationId);
                        if (existingItems != null) continue;
                        ScrollViewEquipment.Children.RemoveAt(index);
                        index--;
                    }
                }
                else
                {
                    ScrollViewEquipment.Children.Clear();
                    var loadingIcon = new ActivityIndicator
                    {
                        AutomationId = "ActivityIndicatorSiteLoading",
                        HorizontalOptions = LayoutOptions.Center,
                        IsEnabled = true,
                        IsRunning = true,
                        IsVisible = true,
                        VerticalOptions = LayoutOptions.Center
                    };
                    ScrollViewEquipment.Children.Add(loadingIcon);
                }

            }
            catch
            {
                // ignored
            }
        }

        private void PopulateSensorEvent(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(PopulateSensor);
        }

        private void PopulateSensor()
        {
            ScreenCleanupForSensor();

            try
            {
                if(_observableIrrigation.SensorList.Contains(null))return;
                if (_observableIrrigation.SensorList.Any(x => _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.ID)))
                    foreach (var sensor in _observableIrrigation.SensorList.Where(x => _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.ID)))
                    {
                        var viewSensorChild = ScrollViewSensor.Children.FirstOrDefault(x =>
                            x.AutomationId == sensor.ID);
                        if (viewSensorChild != null)
                        {
                            var viewSensor = (ViewSensorSummary)viewSensorChild;
                            viewSensor._sensor.NAME = sensor.NAME;
                            viewSensor._sensor.TYPE = sensor.TYPE;
                            viewSensor._sensor.AttachedSubController = sensor.AttachedSubController;
                            viewSensor._sensor.GPIO = sensor.GPIO;
                            viewSensor.Populate();
                        }
                        else
                        {
                            var viewSensorSummary = new ViewSensorSummary(sensor);
                            ScrollViewSensor.Children.Add(viewSensorSummary);
                            viewSensorSummary.GetTapGestureRecognizer().Tapped += ViewSensorScreen_Tapped;
                        }
                    }
                else
                {
                    ScrollViewSensor.Children.Add(new ViewEmptySchedule("No Sensor Here"));
                }
            }
            catch
            {
                // ignored
            }
        }

        private void ScreenCleanupForSensor()
        {
            try
            {
                if (!_observableIrrigation.SensorList.Contains(null))
                {
                    var itemsThatAreOnDisplay = _observableIrrigation.SensorList.Select(x => x.ID).ToList();
                    if (itemsThatAreOnDisplay.Count == 0)
                        itemsThatAreOnDisplay.Add(new ViewEmptySchedule(string.Empty).ID);
                    for (var index = 0; index < ScrollViewSensor.Children.Count; index++)
                    {
                        var existingItems = itemsThatAreOnDisplay.FirstOrDefault(x =>
                            x == ScrollViewSensor.Children[index].AutomationId);
                        if (existingItems != null) continue;
                        ScrollViewSensor.Children.RemoveAt(index);
                        index--;
                    }
                }
                else
                {
                    ScrollViewSensor.Children.Clear();
                    var loadingIcon = new ActivityIndicator
                    {
                        AutomationId = "ActivityIndicatorSiteLoading",
                        HorizontalOptions = LayoutOptions.Center,
                        IsEnabled = true,
                        IsRunning = true,
                        IsVisible = true,
                        VerticalOptions = LayoutOptions.Center
                    };
                    ScrollViewSensor.Children.Add(loadingIcon);
                }
            }
            catch
            {
                // ignored
            }
        }


        

        private void ViewEquipmentScreen_Tapped(object sender, EventArgs e)
        {
            var viewEquipment = (StackLayout)sender;
            var equipment = _observableIrrigation.EquipmentList.First(x => x.ID == viewEquipment.AutomationId);
            Device.BeginInvokeOnMainThread(async () =>
            {
                var action = await DisplayActionSheet("You have selected " + equipment.NAME, 
                    "Cancel", null, "Update", "Delete");
                if(action == null) return;

                if (action == "Update")
                {
                    await Navigation.PushModalAsync(new EquipmentUpdate(_observableIrrigation.EquipmentList.ToList(), _observableIrrigation.SubControllerList.ToList(), _observableIrrigation.SiteList.ToList().First(x =>x.ID == _pumpConnection.SiteSelectedId), equipment));
                }
                else if(action == "Delete")
                {
                    if (await DisplayAlert("Are you sure?",
                        "Confirm to delete " + equipment.NAME, "Delete",
                        "Cancel"))
                        await Task.Run(() => new Authentication().DeleteEquipment(equipment));
                }
            });
        }

        private void ViewSensorScreen_Tapped(object sender, EventArgs e)
        {
            var viewSensor = (StackLayout)sender;
            var sensor = _observableIrrigation.SensorList.First(x => x.ID == viewSensor.AutomationId);
            Device.BeginInvokeOnMainThread(async () =>
            {
                var action = await DisplayActionSheet("You have selected " + sensor.NAME,
                    "Cancel", null, "Update", "Delete");
                if (action == null) return;

                if (action == "Update")
                {
                    await Navigation.PushModalAsync(new SensorUpdate(_observableIrrigation.SensorList.ToList(), _observableIrrigation.SubControllerList.ToList(), _observableIrrigation.EquipmentList.ToList(), _observableIrrigation.SiteList.ToList().First(x => x.ID == _pumpConnection.SiteSelectedId), sensor));
                }
                else
                {
                    if (await DisplayAlert("Are you sure?",
                        "Confirm to delete " + sensor.NAME, "Delete",
                        "Cancel")) return;
                }
            });
        }

        private void BtnBack_OnPressed(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void BtnAddEquipment_OnPressed(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new EquipmentUpdate(_observableIrrigation.EquipmentList.ToList(), _observableIrrigation.SubControllerList.ToList(), _observableIrrigation.SiteList.ToList().First(x => x.ID == _pumpConnection.SiteSelectedId)));
        }

        private void BtnAddSensor_OnPressed(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new SensorUpdate(_observableIrrigation.SensorList.ToList(), _observableIrrigation.SubControllerList.ToList(), _observableIrrigation.EquipmentList.ToList(), _observableIrrigation.SiteList.ToList().First(x => x.ID == _pumpConnection.SiteSelectedId)));
        }
    }
}