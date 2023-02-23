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
    public partial class EquipmentScreen
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
                if (!_observableFilterKeyValuePair.Value.LoadedData) return;
                BtnAddEquipment.IsEnabled = true;
                foreach (var equipment in _observableFilterKeyValuePair.Value.EquipmentList.OrderByDescending(x => x.isPump))
                {
                    var viewSchedule = ScrollViewEquipment.Children.FirstOrDefault(x =>
                        x?.AutomationId == equipment.Id);
                    if (viewSchedule != null)
                    {
                        var viewScheduleStatus = (ViewEquipment)viewSchedule;
                        viewScheduleStatus.Populate(equipment);
                    }
                    else
                    {
                        var viewEquipmentSummary = new ViewEquipment(equipment);
                        ScrollViewEquipment.Children.Add(viewEquipmentSummary);
                        viewEquipmentSummary.GetTapGestureRecognizer().Tapped += ViewEquipmentScreen_Tapped;
                    }
                }
            
            
                if (ScrollViewEquipment.Children.Count == 0)
                    ScrollViewEquipment.Children.Add(new ViewEmptySchedule("No Equipments Here"));
            }
            catch (Exception e)
            {
                ScrollViewEquipment.Children.Add(new ViewException(e));
            }
        }

        private void ScreenCleanupForEquipment()
        {
            if (_observableFilterKeyValuePair.Value.LoadedData)
            {
                var itemsThatAreOnDisplay = _observableFilterKeyValuePair.Value.EquipmentList
                    .Select(x => x?.Id).ToList();
                
                if (itemsThatAreOnDisplay.Count == 0)
                    itemsThatAreOnDisplay.Add(new ViewEmptySchedule().AutomationId);
                ScrollViewEquipment.RemoveUnusedViews(itemsThatAreOnDisplay);
            }
            else
            {
                ScrollViewEquipment.DisplayActivityLoading();
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
                if (!_observableFilterKeyValuePair.Value.LoadedData) return;
                BtnAddSensor.IsEnabled = true;
                foreach (var sensor in _observableFilterKeyValuePair.Value.SensorList)
                {
                    var viewSchedule = ScrollViewSensor.Children.FirstOrDefault(x =>
                        x?.AutomationId == sensor.Id);
                    if (viewSchedule != null)
                    {
                        var viewScheduleStatus = (ViewSensor)viewSchedule;
                        viewScheduleStatus.Populate(sensor);
                    }
                    else
                    {
                        var viewEquipmentSummary = new ViewSensor(sensor);
                        ScrollViewSensor.Children.Add(viewEquipmentSummary);
                        viewEquipmentSummary.GetTapGestureRecognizer().Tapped += ViewSensorScreen_Tapped;
                    }
                }
                
                if (ScrollViewSensor.Children.Count == 0)
                    ScrollViewSensor.Children.Add(new ViewEmptySchedule("No Sensors Here"));
            }
            catch (Exception e)
            {
                ScrollViewSensor.Children.Add(new ViewException(e));
            }
        }

        private void ScreenCleanupForSensor()
        {
            if (_observableFilterKeyValuePair.Value.LoadedData)
            {
                var itemsThatAreOnDisplay = _observableFilterKeyValuePair.Value.SensorList
                    .Select(x => x?.Id).ToList();
                
                if (itemsThatAreOnDisplay.Count == 0)
                    itemsThatAreOnDisplay.Add(new ViewEmptySchedule().AutomationId);
                ScrollViewSensor.RemoveUnusedViews(itemsThatAreOnDisplay);
            }
            else
            {
                ScrollViewSensor.DisplayActivityLoading();
            }
        }


        private async void ViewEquipmentScreen_Tapped(object sender, EventArgs e)
        {
            var gridEquipment = ((Grid)sender).Parent;
            var equipment =
                _observableFilterKeyValuePair.Value.EquipmentList.First(x => x?.Id == gridEquipment.AutomationId);

            var action = await DisplayActionSheet("You have selected " + equipment.NAME,
                "Cancel", null, "Update", "Delete");
            if (action == null) return;

            if (action == "Update")
            {
                if (Navigation.ModalStack.Any(x => x.GetType() == typeof(EquipmentUpdate)))
                    return;
                await Navigation.PushModalAsync(new EquipmentUpdate(_observableFilterKeyValuePair, _socketPicker, equipment));
            }
                
            else if (action == "Delete")
                if (await DisplayAlert("Are you sure?",
                        "Confirm to delete " + equipment.NAME, "Delete",
                        "Cancel"))
                {
                    DeleteEquipment(equipment);
                }
        }

        private async void DeleteEquipment(Equipment equipment)
        {
            var notification = EquipmentScheduleValidate(equipment);
            if (!string.IsNullOrWhiteSpace(notification))
            {
                await DisplayAlert("Incomplete", notification, "Understood");
            }
            else
            {
                equipment.DeleteAwaiting = true;
                        
                var viewEquipment = (ViewEquipment)
                    ScrollViewEquipment.Children.FirstOrDefault(x => x.AutomationId == equipment.Id);
                viewEquipment?.AddStatusActivityIndicator();
                        
                await _socketPicker.SendCommand(equipment, _observableFilterKeyValuePair.Key);
            }
        }

        private async void ViewSensorScreen_Tapped(object sender, EventArgs e)
        {
            var viewSensor = ((Grid)sender).Parent;
            var sensor = _observableFilterKeyValuePair.Value.SensorList.First(x => x?.Id == viewSensor.AutomationId);

            var action = await DisplayActionSheet("You have selected " + sensor.NAME,
                "Cancel", null, "Update", "Delete");
            if (action == null) return;

            if (action == "Update")
            {
                if (Navigation.ModalStack.Any(x => x.GetType() == typeof(SensorUpdate)))
                    return;
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
        
        public void AddLoadingSensorScreenFromId(string id)
        {
            var viewSensor = (ViewSensor)
                ScrollViewSensor.Children.FirstOrDefault(x => x.AutomationId == id);
            viewSensor?.AddStatusActivityIndicator();
        }
        private void BtnBack_OnPressed(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void BtnAddEquipment_OnPressed(object sender, EventArgs e)
        {
            if (Navigation.ModalStack.Any(x => x.GetType() == typeof(EquipmentUpdate)))
                return;
            Navigation.PushModalAsync(new EquipmentUpdate(_observableFilterKeyValuePair, _socketPicker));
        }

        private void BtnAddSensor_OnPressed(object sender, EventArgs e)
        {
            if (Navigation.ModalStack.Any(x => x.GetType() == typeof(SensorUpdate)))
                return;
            Navigation.PushModalAsync(new SensorUpdate(_observableFilterKeyValuePair, _socketPicker));
        }

        private string EquipmentScheduleValidate(Equipment equipment)
        {
            var notification = _observableFilterKeyValuePair.Value.ScheduleList.Where(schedule => schedule.ScheduleDetails.Select(x => x.id_Equipment).Contains(equipment.Id) || schedule.id_Pump == equipment.Id).Aggregate("", (current, schedule) => current + ("\n\u2022 Schedule " + schedule.NAME + " requires this"));

            notification = _observableFilterKeyValuePair.Value.CustomScheduleList.Where(customSchedule => customSchedule.ScheduleDetails.Select(x => x.id_Equipment).Contains(equipment.Id) || customSchedule.id_Pump == equipment.Id).Aggregate(notification, (current, customSchedule) => current + ("\n\u2022 Custom Schedule " + customSchedule.NAME + " requires this"));

            return _observableFilterKeyValuePair.Value.ManualScheduleList.Where(manualSchedule => manualSchedule.ManualDetails.Select(x => x.id_Equipment).Contains(equipment.Id)).Aggregate(notification, (current, _) => current + "\n\u2022 The current Manual Schedule running requires this");
        }
    }
}