using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Pump.Database.Table;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Pump.SocketController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EquipmentScreen : ContentPage
    {
        private readonly KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation>
            _observableFilterKeyValuePair;

        private readonly SocketPicker _socketPicker;

        public EquipmentScreen(
            KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation> observableFilterKeyValuePair,
            SocketPicker socketPicker)
        {
            InitializeComponent();
            _observableFilterKeyValuePair = observableFilterKeyValuePair;
            _socketPicker = socketPicker;
            _observableFilterKeyValuePair.Value.EquipmentList.CollectionChanged += PopulateEquipmentEvent;
            _observableFilterKeyValuePair.Value.SensorList.CollectionChanged += PopulateSensorEvent;
            PopulateEquipment();
            PopulateSensor();
        }

        private void PopulateEquipmentEvent(object sender, NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(PopulateEquipment);
        }

        private void PopulateEquipment()
        {
            ScreenCleanupForEquipment();
            try
            {
                if (_observableFilterKeyValuePair.Value.EquipmentList.Contains(null)) return;
                BtnAddEquipment.IsEnabled = true;
                if (_observableFilterKeyValuePair.Value.EquipmentList.Any())
                    foreach (var equipment in _observableFilterKeyValuePair.Value.EquipmentList
                                 .OrderBy(c => c.NAME.Length)
                                 .ThenBy(c => c.NAME))
                    {
                        var viewEquipment = ScrollViewEquipment.Children.FirstOrDefault(x =>
                            x.AutomationId == equipment.Id);
                        if (viewEquipment != null)
                        {
                            var viewScheduleStatus = (ViewEquipmentSummary)viewEquipment;
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
                    ScrollViewEquipment.Children.Add(new ViewEmptySchedule("No Equipments Here"));
            }
            catch (Exception e)
            {
                ScrollViewEquipment.Children.Add(new ViewException(e));
            }
        }

        private void ScreenCleanupForEquipment()
        {
            try
            {
                if (_observableFilterKeyValuePair.Value.LoadedAllData())
                {
                    var itemsThatAreOnDisplay =
                        _observableFilterKeyValuePair.Value.EquipmentList.Select(x => x?.Id).ToList();
                    if (itemsThatAreOnDisplay.Count == 0)
                        itemsThatAreOnDisplay.Add(new ViewEmptySchedule(string.Empty).AutomationId);

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
                    if (ScrollViewEquipment.Children.Count == 1 && ScrollViewEquipment.Children.First().AutomationId ==
                        "ActivityIndicatorSiteLoading") return;
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

        private void PopulateSensorEvent(object sender, NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(PopulateSensor);
        }

        private void PopulateSensor()
        {
            ScreenCleanupForSensor();

            try
            {
                if (_observableFilterKeyValuePair.Value.EquipmentList.Contains(null)) return;
                BtnAddSensor.IsEnabled = true;
                if (_observableFilterKeyValuePair.Value.SensorList.Any())
                    foreach (var sensor in _observableFilterKeyValuePair.Value.SensorList)
                    {
                        var viewSensorChild = ScrollViewSensor.Children.FirstOrDefault(x =>
                            x.AutomationId == sensor.Id);
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
                    ScrollViewSensor.Children.Add(new ViewEmptySchedule("No Sensor Here"));
            }
            catch (Exception e)
            {
                ScrollViewEquipment.Children.Add(new ViewException(e));
            }
        }

        private void ScreenCleanupForSensor()
        {
            try
            {
                if (_observableFilterKeyValuePair.Value.LoadedAllData())
                {
                    var itemsThatAreOnDisplay =
                        _observableFilterKeyValuePair.Value.SensorList.Select(x => x?.Id).ToList();
                    if (itemsThatAreOnDisplay.Count == 0)
                        itemsThatAreOnDisplay.Add(new ViewEmptySchedule(string.Empty).AutomationId);
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
                    if (ScrollViewSensor.Children.Count == 1 && ScrollViewSensor.Children.First().AutomationId ==
                        "ActivityIndicatorSiteLoading") return;

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


        private async void ViewEquipmentScreen_Tapped(object sender, EventArgs e)
        {
            var viewEquipment = (StackLayout)sender;
            var equipment =
                _observableFilterKeyValuePair.Value.EquipmentList.First(x => x?.Id == viewEquipment.AutomationId);

            var action = await DisplayActionSheet("You have selected " + equipment.NAME,
                "Cancel", null, "Update", "Delete");
            if (action == null) return;

            if (action == "Update")
                await Navigation.PushModalAsync(new EquipmentUpdate(_observableFilterKeyValuePair, _socketPicker,
                    equipment));
            else if (action == "Delete")
                if (await DisplayAlert("Are you sure?",
                        "Confirm to delete " + equipment.NAME, "Delete",
                        "Cancel"))
                {
                    equipment.DeleteAwaiting = true;
                    await _socketPicker.SendCommand(equipment, _observableFilterKeyValuePair.Key);
                }
        }

        private async void ViewSensorScreen_Tapped(object sender, EventArgs e)
        {
            var viewSensor = (StackLayout)sender;
            var sensor = _observableFilterKeyValuePair.Value.SensorList.First(x => x?.Id == viewSensor.AutomationId);

            var action = await DisplayActionSheet("You have selected " + sensor.NAME,
                "Cancel", null, "Update", "Delete");
            if (action == null) return;

            if (action == "Update")
            {
                await Navigation.PushModalAsync(new SensorUpdate(_observableFilterKeyValuePair, _socketPicker, sensor));
            }
            else
            {
                if (await DisplayAlert("Are you sure?",
                        "Confirm to delete " + sensor.NAME, "Delete",
                        "Cancel"))
                {
                    sensor.DeleteAwaiting = true;
                    await _socketPicker.SendCommand(sensor, _observableFilterKeyValuePair.Key);
                }
            }
        }

        private void BtnBack_OnPressed(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void BtnAddEquipment_OnPressed(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new EquipmentUpdate(_observableFilterKeyValuePair, _socketPicker));
        }

        private void BtnAddSensor_OnPressed(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new SensorUpdate(_observableFilterKeyValuePair, _socketPicker));
        }
    }
}