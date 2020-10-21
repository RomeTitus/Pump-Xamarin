using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Database.Streaming;
using Pump.Class;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewEquipmentScreen : ContentPage
    {
        private ObservableCollection<Equipment> _equipmentList;
        private ObservableCollection<Sensor> _sensorList;
        private ObservableCollection<PiController> _piControllerList;

        public ViewEquipmentScreen()
        {
            InitializeComponent();
            new Thread(GetEquipmentFirebase).Start();
        }

        private void GetEquipmentFirebase()
        {
            var auth = new Authentication();
            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Equipment")
                .AsObservable<Equipment>()
                .Subscribe(x =>
                {
                    if (_equipmentList == null)
                        _equipmentList = new ObservableCollection<Equipment>();
                    var equipment = x.Object;
                    if (x.EventType == FirebaseEventType.Delete)
                    {
                        for (int i = 0; i < _equipmentList.Count; i++)
                        {
                            if (_equipmentList[i].ID == x.Key)
                                _equipmentList.RemoveAt(i);
                        }
                    }
                    else
                    {
                        var existingEquipment = _equipmentList.FirstOrDefault(y => y.ID == x.Key);
                        if (existingEquipment != null)
                        {
                            FirebaseMerger.CopyValues(existingEquipment, equipment);
                            Device.InvokeOnMainThreadAsync(PopulateEquipment);
                        }
                        else
                        {
                            equipment.ID = x.Key;
                            _equipmentList.Add(equipment);
                        }
                    }
                });

            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Sensor")
                .AsObservable<Sensor>()
                .Subscribe(x =>
                {

                    try
                    {
                        if (_sensorList == null)
                            _sensorList = new ObservableCollection<Sensor>();
                        var sensor = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (int i = 0; i < _sensorList.Count; i++)
                            {
                                if (_sensorList[i].ID == x.Key)
                                    _sensorList.RemoveAt(i);
                            }
                        }
                        else
                        {
                            var existingSensor = _sensorList.FirstOrDefault(y => y.ID == x.Key);
                            if (existingSensor != null)
                            {
                                FirebaseMerger.CopyValues(existingSensor, sensor);
                                Device.BeginInvokeOnMainThread(PopulateSensor);
                            }
                            else
                            {
                                sensor.ID = x.Key;
                                _sensorList.Add(sensor);
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                });

            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/PiController")
                .AsObservable<PiController>()
                .Subscribe(x =>
                {

                    try
                    {
                        if (_piControllerList == null)
                            _piControllerList = new ObservableCollection<PiController>();
                        var piController = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (int i = 0; i < _sensorList.Count; i++)
                            {
                                if (_piControllerList[i].ID == x.Key)
                                    _piControllerList.RemoveAt(i);
                            }
                        }
                        else
                        {
                            var existingPiController = _piControllerList.FirstOrDefault(y => y.ID == x.Key);
                            if (existingPiController != null)
                            {
                                FirebaseMerger.CopyValues(existingPiController, piController);
                                Device.BeginInvokeOnMainThread(PopulatePiController);
                            }
                            else
                            {
                                piController.ID = x.Key;
                                _piControllerList.Add(piController);
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                });

            LoadScreens();
        }


        private void LoadScreens()
        {
            var hasSubscribed = false;
            var equipmentHasRun = false;
            var sensorHasRun = false;
            var piControllerHasRun = false;
            while (!hasSubscribed)
            {
                try
                {
                    if (_equipmentList != null && _sensorList != null && _piControllerList != null)
                    {
                        Device.InvokeOnMainThreadAsync(() =>
                        {
                            BtnAddEquipment.IsEnabled = true;
                            BtnAddSensor.IsEnabled = true;
                            BtnAddSubController.IsEnabled = true;
                        });
                        hasSubscribed = true;
                    }
                    if (_equipmentList != null && !equipmentHasRun)
                    {
                        _equipmentList.CollectionChanged += PopulateEquipmentEvent;
                        Device.InvokeOnMainThreadAsync(PopulateEquipment);
                    }
                    if (_sensorList != null && !sensorHasRun)
                    {
                        _sensorList.CollectionChanged += PopulateSensorEvent;
                        Device.InvokeOnMainThreadAsync(PopulateSensor);
                    }
                    if (_piControllerList != null && !piControllerHasRun)
                    {
                        _piControllerList.CollectionChanged += PopulatePiControllerEvent;
                        Device.InvokeOnMainThreadAsync(PopulatePiController);
                    }
                }
                catch
                {
                    // ignored
                }
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
                if (_equipmentList.Any())
                    foreach (var equipment in _equipmentList.OrderBy(x => Convert.ToInt32(x.GPIO)).ToList())
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
                            viewScheduleStatus._equipment.AttachedPiController = equipment.AttachedPiController;
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
                if (_equipmentList != null)
                {

                    var itemsThatAreOnDisplay = _equipmentList.Select(x => x.ID).ToList();
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
                if (_sensorList.Any())
                    foreach (var sensor in _sensorList.OrderBy(x => Convert.ToInt32(x.GPIO)).ToList())
                    {
                        var viewSensorChild = ScrollViewSensor.Children.FirstOrDefault(x =>
                            x.AutomationId == sensor.ID);
                        if (viewSensorChild != null)
                        {
                            var viewSensor = (ViewSensorSummary)viewSensorChild;
                            viewSensor._sensor.NAME = sensor.NAME;
                            viewSensor._sensor.TYPE = sensor.TYPE;
                            viewSensor._sensor.AttachedPiController = sensor.AttachedPiController;
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
                    ScrollViewEquipment.Children.Add(new ViewEmptySchedule("No Sensor Here"));
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
                if (_sensorList != null)
                {
                    var itemsThatAreOnDisplay = _sensorList.Select(x => x.ID).ToList();
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
            }
            catch
            {
                // ignored
            }
        }


        private void PopulatePiControllerEvent(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(PopulatePiController);
        }

        private void PopulatePiController()
        {
            ScreenCleanupForPiController();

            try
            {
                if (_piControllerList.Any())
                    foreach (var piController in _piControllerList.ToList())
                    {
                        var viewPiControllerChild = ScrollViewSubController.Children.FirstOrDefault(x =>
                            x.AutomationId == piController.ID);
                        if (viewPiControllerChild != null)
                        {
                            var viewPiController = (ViewSubControllerSummary)viewPiControllerChild;
                            viewPiController._piController.NAME = piController.NAME;
                            viewPiController._piController.IpAdress = piController.IpAdress;
                            viewPiController._piController.BTmac = piController.IpAdress;
                            viewPiController._piController.Port = piController.Port;
                            viewPiController.Populate();
                        }
                        else
                        {
                            var viewPiControllerSummary = new ViewSubControllerSummary(piController);
                            ScrollViewSubController.Children.Add(viewPiControllerSummary);
                            viewPiControllerSummary.GetTapGestureRecognizer().Tapped += ViewPiControllerScreen_Tapped;
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

        private void ScreenCleanupForPiController()
        {
            try
            {
                if (_piControllerList != null)
                {
                    var itemsThatAreOnDisplay = _piControllerList.Select(x => x.ID).ToList();
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
            }
            catch
            {
                // ignored
            }
        }

        private void ViewEquipmentScreen_Tapped(object sender, EventArgs e)
        {
            var viewEquipment = (StackLayout)sender;
            var equipment = _equipmentList.First(x => x.ID == viewEquipment.AutomationId);
            Device.BeginInvokeOnMainThread(async () =>
            {
                var action = await DisplayActionSheet("You have selected " + equipment.NAME, 
                    "Cancel", null, "Update", "Delete");
                if(action == null) return;

                if (action == "Update")
                {
                    var availablePins = getAvailablePins();
                    availablePins.Add(long.Parse(equipment.GPIO));
                    if (equipment.DirectOnlineGPIO != null)
                        availablePins.Add(long.Parse(equipment.DirectOnlineGPIO));
                    await Navigation.PushModalAsync(new UpdateEquipment(availablePins, _piControllerList.ToList(), equipment));
                }
                else
                {
                    if (await DisplayAlert("Are you sure?",
                        "Confirm to delete " + equipment.NAME, "Delete",
                        "Cancel"))
                        await Task.Run(() => new Authentication().DeleteEquipment(equipment));
                }
            });
            //ViewScheduleSummary(scheduleSwitch.AutomationId);
        }

        private void ViewSensorScreen_Tapped(object sender, EventArgs e)
        {
            var viewSensor = (StackLayout)sender;
            var sensor = _sensorList.First(x => x.ID == viewSensor.AutomationId);
            Device.BeginInvokeOnMainThread(async () =>
            {
                var action = await DisplayActionSheet("You have selected " + sensor.NAME,
                    "Cancel", null, "Update", "Delete");
                if (action == null) return;

                if (action == "Update")
                {
                    
                }
                else
                {
                    if (await DisplayAlert("Are you sure?",
                        "Confirm to delete " + sensor.NAME, "Delete",
                        "Cancel")) return;
                }
            });
            //ViewScheduleSummary(scheduleSwitch.AutomationId);
        }

        private void ViewPiControllerScreen_Tapped(object sender, EventArgs e)
        {
            var viewPiController = (StackLayout)sender;
            var piController = _piControllerList.First(x => x.ID == viewPiController.AutomationId);
            Device.BeginInvokeOnMainThread(async () =>
            {
                var action = await DisplayActionSheet("You have selected " + piController.NAME,
                    "Cancel", null, "Update", "Delete");
                if (action == null) return;

                if (action == "Update")
                {

                }
                else
                {
                    if (await DisplayAlert("Are you sure?",
                        "Confirm to delete " + piController.NAME, "Delete",
                        "Cancel")) return;
                }
            });
            //ViewScheduleSummary(scheduleSwitch.AutomationId);
        }


        private List<long> getAvailablePins()
        {
            var availableList = new GpioPins().GetGpioList();

            var usedPins = _equipmentList.Select(x => long.Parse(x.GPIO)).ToList();
            usedPins.AddRange(_sensorList.Select(x => long.Parse(x.GPIO)).ToList());


            for (var i = 0; i < availableList.Count; i++)
            {
                var exist = false;

                if (usedPins.Contains(availableList[i]))
                {
                    availableList.RemoveAt(i);
                    i--;
                    continue;
                }

            }
            return availableList;
        }

        private void BtnBack_OnPressed(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void BtnAddEquipment_OnPressed(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new UpdateEquipment(getAvailablePins(), _piControllerList.ToList()));
        }
    }
}