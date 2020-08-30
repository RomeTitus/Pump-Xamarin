using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;
using Pump.Database;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Pump.SocketController;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ManualScheduleClass = Pump.Database.Table.ManualScheduleClass;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManualSchedule : ContentPage
    {
        private readonly List<IrrigationController.ManualSchedule> _manualScheduleList = new List<IrrigationController.ManualSchedule>();
        private readonly List<Equipment> _equipmentList = new List<Equipment>();

        private readonly SocketCommands _command = new SocketCommands();
        private readonly List<string> _queueManualSchedule = new List<string>();
        private readonly SocketMessage _socket = new SocketMessage();
        private List<string> _activeManualScheduleId = new List<string>();
        private bool _buttonEnabledStatus = true;
        private FloatingScreenScroll _floatingScreenScroll;
        private string _oldIrrigationPump;
        private string _oldIrrigationZone;
        private string _oldManualSchedule;
        private bool? _firebaseHasReplied = false;

        public ManualSchedule()
        {
            InitializeComponent();
            new Thread(ThreadController).Start();
        }

        private void GetManualScheduleFirebase()
        {
            var auth = new Authentication();
            //_manualScheduleList = Task.Run(() => auth.GetManualSchedule()).Result;

            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/ManualSchedule")
                .AsObservable<JObject>()
                .Subscribe(x =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (x.Object != null)
                        {
                            _buttonEnabledStatus = false;
                            var manualSchedule = auth.GetJsonManualSchedulesToObjectList(x.Object, x.Key);
                            _manualScheduleList.Clear();
                            _manualScheduleList.Add(manualSchedule);
                            PopulateManualElements();
                            SetButtonStatus();
                        }
                        else
                        {
                            _buttonEnabledStatus = true;
                            _manualScheduleList.Clear();
                            _activeManualScheduleId.Clear();
                            SetButtonStatus();
                        }

                    });
                    
                    
                });


            


            PopulateManualElements();
        }

        private void PopulateManualElements()
        {
            if (_manualScheduleList.Count <= 0) return;
            _activeManualScheduleId.Clear();
            foreach (var equipment in _manualScheduleList.SelectMany(manualSchedule => manualSchedule.equipmentIdList))
            {
                _activeManualScheduleId.Add(equipment.ID);
            }
            Device.BeginInvokeOnMainThread(() =>
            {
                SwitchRunWithSchedule.IsToggled = _manualScheduleList[0].RunWithSchedule;
                SwitchRunWithSchedule.IsEnabled = false;
                var timeLeft = new DateTime(_manualScheduleList[0].EndTime) - DateTime.Now;

                MaskedEntryTime.IsEnabled = false;
                MaskedEntryTime.Text = new ScheduleTime().convertDateTimeToString(timeLeft);
                ButtonStartManual.IsEnabled = false;

            });
            
            }

        private void GetEquipmentFirebase()
        {
            var auth = new Authentication();
            //_equipmentList = Task.Run(() => auth.GetAllEquipment()).Result;

            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Equipment")
                .AsObservable<JObject>()
                .Subscribe(x =>
                {
                    var equipment = auth.GetJsonEquipmentToObjectList(x.Object, x.Key);
                    _equipmentList.RemoveAll(y => y.ID == equipment.ID);
                    _equipmentList.Add(equipment);
                    PopulateEquipments();
                });
            //PopulateEquipments();
        }

        private void PopulateEquipments()
        {
            var pumps = _equipmentList.Where(equipment => equipment.isPump);
            var pumpsString = pumps.Aggregate("", (current, pump) => current + (pump.ID + ',' + pump.NAME + '#'));
            var zones = _equipmentList.Where(equipment => equipment.isPump == false);
            var zonesString = zones.Aggregate("", (current, zone) => current + (zone.ID + ',' + zone.NAME + '#'));

            var pumpObject = GetEquipmentObject(pumpsString);
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    _oldIrrigationPump = pumpsString;
                    ScrollViewManualPump.Children.Clear();
                    foreach (Button view in pumpObject)
                    {
                        if (_activeManualScheduleId.Count > 0)
                        {
                            if (_activeManualScheduleId.Contains(view.AutomationId))
                                view.BackgroundColor = Color.BlueViolet;
                            else
                                view.IsEnabled = false;
                        }

                        ScrollViewManualPump.Children.Add(view);
                    }
                }
                catch
                {
                    // ignored
                }

                var zoneObject = GetEquipmentObject(zonesString);
                try
                {
                    _oldIrrigationZone = zonesString;
                    ScrollViewManualZone.Children.Clear();
                    foreach (Button view in zoneObject)
                    {
                        if (_activeManualScheduleId.Count > 0)
                        {
                            if (_activeManualScheduleId.Contains(view.AutomationId))
                                view.BackgroundColor = Color.BlueViolet;
                            else
                                view.IsEnabled = false;
                        }

                        ScrollViewManualZone.Children.Add(view);
                    }
                }
                catch
                {
                    // ignored
                }
            });
        }

        private void ThreadController()
        {
            var started = false;
            var firebaseStarted = false;
            var databaseController = new DatabaseController();

            Thread manualSchedule = null;
            Thread pumps = null;
            Thread zones = null;

            while (true)
            {
                if (databaseController.IsRealtimeFirebaseSelected())
                {
                    if (firebaseStarted == false)
                    {
                        firebaseStarted = true;

                        GetManualScheduleFirebase();
                        GetEquipmentFirebase();
                        
                    }
                }
                else
                {
                    _equipmentList.Clear();
                    firebaseStarted = false;
                    if (started == false && databaseController.GetActivityStatus() != null &&
                        databaseController.GetActivityStatus().status)
                    {
                        //Start the threads
                        manualSchedule = new Thread(GetManualSchedule);
                        pumps = new Thread(GetPumps);
                        zones = new Thread(GetZones);

                        manualSchedule.Start();
                        pumps.Start();
                        zones.Start();
                        started = true;
                    }

                    if (manualSchedule != null)
                        if (started && databaseController.GetActivityStatus() != null &&
                            databaseController.GetActivityStatus().status == false)
                        {
                            manualSchedule.Abort();
                            pumps.Abort();
                            zones.Abort();
                            started = false;
                        }
                }
                

                Thread.Sleep(2000);
            }
        }

        private void GetManualSchedule()
        {
            var running = true;
            while (running)
            {
                try
                {
                    var manualSchedule = _socket.Message(_command.getManualSchedule());

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            if (_oldManualSchedule == manualSchedule)
                                return;
                            _oldManualSchedule = manualSchedule;

                            var manualScheduleClass = new ManualScheduleClass();
                            _buttonEnabledStatus = true;
                            if (manualSchedule != "No Data" && manualSchedule != "" && manualSchedule != "Data Empty")
                            {
                                _buttonEnabledStatus = false;
                                var manualScheduleDetail = manualSchedule.Split('$')
                                    .Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                                manualScheduleClass.RunWithSchedule =
                                    Convert.ToBoolean(Convert.ToInt32(manualScheduleDetail[0]));
                                manualScheduleClass.setEquipmentIDAndTime(manualScheduleDetail[1].Split('#')
                                    .Where(x => !string.IsNullOrWhiteSpace(x)).ToList());
                                _activeManualScheduleId = manualScheduleClass.getEquipmentID();
                                _queueManualSchedule.Clear();
                                SwitchRunWithSchedule.IsToggled = manualScheduleClass.RunWithSchedule;
                                SwitchRunWithSchedule.IsEnabled = _buttonEnabledStatus;
                                ButtonStartManual.IsEnabled = _buttonEnabledStatus;
                                var scheduleTime = new ScheduleTime();
                                MaskedEntryTime.Text = scheduleTime.TimeDiffNow(manualScheduleClass.ScheduleTime);
                                MaskedEntryTime.IsEnabled = _buttonEnabledStatus;
                                if (_floatingScreenScroll != null)
                                    try
                                    {
                                        PopupNavigation.Instance.PopAsync();
                                    }
                                    finally
                                    {
                                        _floatingScreenScroll = null;
                                    }
                            }
                            SetButtonStatus();
                        }
                        catch
                        {
                            // ignored
                        }
                    });
                }
                catch (ThreadAbortException)
                {
                    running = false;
                }
                catch
                {
                    // ignored
                }

                Thread.Sleep(15000);
            }
        }

        private void SetButtonStatus()
        {
            var buttonList = ScrollViewManualPump.Children.ToList();
            buttonList.AddRange(ScrollViewManualZone.Children.ToList());

            var isActivityIndicator = false;

            foreach (var button in buttonList.Where(button =>
                button.AutomationId == "ActivityIndicatorManualZone" ||
                button.AutomationId == "ActivityIndicatorManualPump")) isActivityIndicator = true;

            if (ScrollViewManualPump.Children.Count == 0 || ScrollViewManualZone.Children.Count == 0 ||
                isActivityIndicator)
                return;


            var buttons = buttonList.Cast<Button>().Cast<object>().ToList();
            SetButtonToDisabled(buttons);

            if (_buttonEnabledStatus)
                ResetAllActions();
        }

        private void ResetAllActions()
        {
            _activeManualScheduleId.Clear();
            MaskedEntryTime.Text = "";
            MaskedEntryTime.IsEnabled = true;
            ButtonStartManual.IsEnabled = true;
            SwitchRunWithSchedule.IsEnabled = true;
            SwitchRunWithSchedule.IsToggled = true;
        }

        private void DisableAllActions()
        {
            MaskedEntryTime.IsEnabled = false;
            ButtonStartManual.IsEnabled = false;
            SwitchRunWithSchedule.IsEnabled = false;
            var buttonList = ScrollViewManualPump.Children.ToList();
            buttonList.AddRange(ScrollViewManualZone.Children.ToList());

            var buttons = buttonList.Cast<Button>().Cast<object>().ToList();
            SetButtonToDisabled(buttons);
            _queueManualSchedule.Clear();
        }

        private bool SetButtonToDisabled(IEnumerable<object> buttonList)
        {
            foreach (Button button in buttonList)
                if (_buttonEnabledStatus == false)
                {
                    if (_activeManualScheduleId.Count > 0 && _activeManualScheduleId.Contains(button.AutomationId))
                    {
                        button.BackgroundColor = Color.BlueViolet;
                    }
                    else
                    {
                        button.IsEnabled = _buttonEnabledStatus;
                        button.BackgroundColor = Color.AliceBlue;
                    }
                }
                else
                {
                    button.IsEnabled = _buttonEnabledStatus;
                    button.BackgroundColor = Color.AliceBlue;
                }

            return _buttonEnabledStatus;
        }

        private void GetPumps()
        {
            var running = true;
            while (running)
            {
                try
                {
                    Thread.Sleep(10);
                    var irrigationPump = _socket.Message(_command.getPumps());

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (_oldIrrigationPump == irrigationPump)
                            return;
                        ScrollViewManualPump.Children.Clear();
                        _oldIrrigationPump = irrigationPump;

                        var irrigationPumpListObject = GetEquipmentObject(irrigationPump);
                        try
                        {
                            foreach (Button view in irrigationPumpListObject)
                            {
                                if (_activeManualScheduleId.Count > 0)
                                {
                                    if (_activeManualScheduleId.Contains(view.AutomationId))
                                        view.BackgroundColor = Color.BlueViolet;
                                    else
                                        view.IsEnabled = false;
                                }

                                ScrollViewManualPump.Children.Add(view);
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    });
                }
                catch (ThreadAbortException)
                {
                    running = false;
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ScrollViewManualPump.Children.Clear();
                        ScrollViewManualPump.Children.Add(new ViewNoConnection());
                    });
                }

                Thread.Sleep(30000);
            }
        }

        private List<object> GetEquipmentObject(string irrigationPump)
        {
            var equipmentListObject = new List<object>();
            try
            {
                if (irrigationPump == "No Data" || irrigationPump == "")
                {
                    equipmentListObject.Add(new ViewEmptySchedule("No Equipment Found Here"));
                    return equipmentListObject;
                }

                var pumpList = irrigationPump.Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                equipmentListObject.AddRange(pumpList.Select(pump => CreateButton(pump.Split(',').ToList())));

                return equipmentListObject;
            }
            catch
            {
                equipmentListObject.Clear();
                equipmentListObject.Add(new ViewNoConnection());
                return equipmentListObject;
            }
        }

        private void GetZones()
        {
            var running = true;
            while (running)
            {
                try
                {
                    Thread.Sleep(10);
                    var irrigationZone = _socket.Message(_command.getValves());

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (_oldIrrigationZone == irrigationZone)
                            return;
                        ScrollViewManualZone.Children.Clear();
                        _oldIrrigationZone = irrigationZone;

                        var irrigationZoneListObject = GetEquipmentObject(irrigationZone);
                        try
                        {
                            foreach (Button view in irrigationZoneListObject)
                            {
                                if (_activeManualScheduleId.Count > 0)
                                {
                                    if (_activeManualScheduleId.Contains(view.AutomationId))
                                        view.BackgroundColor = Color.BlueViolet;
                                    else
                                        view.IsEnabled = false;
                                }

                                ScrollViewManualZone.Children.Add(view);
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    });
                }
                catch (ThreadAbortException)
                {
                    running = false;
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ScrollViewManualZone.Children.Clear();
                        ScrollViewManualZone.Children.Add(new ViewNoConnection());
                    });
                }

                Thread.Sleep(30000);
            }
        }

        private Button CreateButton(IReadOnlyList<string> equipment)
        {
            var button = new Button
            {
                Text = equipment[1],
                AutomationId = equipment[0],
                HeightRequest = 40,
                Margin = new Thickness(10, 10, 10, 20),
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.Center,
                BackgroundColor = Color.AliceBlue,
                BorderColor = Color.BlueViolet
            };
            button.Clicked += Button_Tapped;

            return button;
        }

        private List<object> ButtonSelected(List<object> buttons)
        {
            foreach (Button button in buttons)
                button.BackgroundColor = _queueManualSchedule.Contains(button.AutomationId)
                    ? Color.CornflowerBlue
                    : Color.AliceBlue;
            return buttons;
        }

        private void ChangeButtonStatus(string id)
        {
            var buttonList = ScrollViewManualPump.Children.ToList();
            buttonList.AddRange(ScrollViewManualZone.Children.ToList());
            try
            {
                foreach (Button button in buttonList)
                    try
                    {
                        if (button.AutomationId == id) UpdateSelectedEquipment(button);
                    }
                    catch
                    {
                        // ignored
                    }
            }
            catch
            {
                // ignored
            }
        }

        private void UpdateSelectedEquipment(Button button)
        {
            button.BackgroundColor = _queueManualSchedule.Contains(button.AutomationId)
                ? Color.AliceBlue
                : Color.CornflowerBlue;
        }


        private void Button_Tapped(object sender, EventArgs e)
        {
            if (_activeManualScheduleId.Count > 0)
                return;
            var button = (Button) sender;

            ChangeButtonStatus(button.AutomationId);


            if (_queueManualSchedule.Contains(button.AutomationId))
            {
                _queueManualSchedule.Remove(button.AutomationId);
                button.BackgroundColor = Color.AliceBlue;
            }
            else
            {
                button.BackgroundColor = Color.CornflowerBlue;
                _queueManualSchedule.Add(button.AutomationId);
            }
        }

        private void ButtonStartManual_Clicked(object sender, EventArgs e)
        {
            new Thread(StartManualSchedule).Start();
        }

        private void ButtonStopManual_Clicked(object sender, EventArgs e)
        {
            new Thread(StopManualSchedule).Start();
        }

        private void ScrollViewManualZoneTap_Tapped(object sender, EventArgs e)
        {
            if (_oldIrrigationZone == null)
                return;

            ViewTapped(GetEquipmentObject(_oldIrrigationZone));
        }

        private void ScrollViewManualPump_Tapped(object sender, EventArgs e)
        {
            if (_oldIrrigationZone == null)
                return;

            ViewTapped(GetEquipmentObject(_oldIrrigationPump));
        }

        private void ViewTapped(List<object> equipment)
        {
            _floatingScreenScroll = new FloatingScreenScroll();
            if (SetButtonToDisabled(equipment))
                equipment = ButtonSelected(equipment);
            _floatingScreenScroll.setFloatingScreen(equipment);
            PopupNavigation.Instance.PushAsync(_floatingScreenScroll);
        }

        private void StopManualSchedule()
        {
            if (new DatabaseController().IsRealtimeFirebaseSelected())
            {
                _firebaseHasReplied = null;
                var auth = new Authentication();
               
                var key = Task.Run(() => auth.DeleteManualSchedule()).Result;
                
                auth._FirebaseClient
                    .Child(auth.getConnectedPi()).Child("Status")
                    .AsObservable<JObject>()
                    .Subscribe(x =>
                    {
                        if (x.Key != key || _firebaseHasReplied == false)
                            return;
                        _firebaseHasReplied = true;
                        
                        PopupNavigation.Instance.PopAsync();
                        var result = new Authentication().GetJsonStatusToObjectList(x.Object, x.Key);
                        if (result.Code == "success") return;
                        
                        Device.BeginInvokeOnMainThread(() =>{DisplayAlert(result.Operation, result.Code, "Understood");});

                    });

                var stopwatch = new Stopwatch();
                var floatingScreenScreen = new FloatingScreen {CloseWhenBackgroundIsClicked = false};
                PopupNavigation.Instance.PushAsync(floatingScreenScreen);
                stopwatch.Start();

                while (stopwatch.Elapsed < TimeSpan.FromSeconds(15))
                {
                    Thread.Sleep(500);
                }
                stopwatch.Stop();

                if (_firebaseHasReplied != null) return;
                PopupNavigation.Instance.PopAsync();
                _firebaseHasReplied = false;
                Device.BeginInvokeOnMainThread(() =>{DisplayAlert("Cleared!", "We never got a reply back", "Understood");});
                
            }
            else
            {
                try
                {
                    _socket.Message(_command.StopManualSchedule());

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        _queueManualSchedule.Clear();
                        _activeManualScheduleId.Clear();
                        _buttonEnabledStatus = true;
                        ResetAllActions();
                        var buttonList = ScrollViewManualPump.Children.ToList();
                        buttonList.AddRange(ScrollViewManualZone.Children.ToList());

                        _oldManualSchedule = "Data Empty";
                    });
                }
                catch
                {
                    // ignored
                }
            }
        }

        private void StartManualSchedule()
        {
            if (_queueManualSchedule.Count < 1 && MaskedEntryTime.Text.Count() < 4) return;

            Device.BeginInvokeOnMainThread(() => { ButtonStartManual.IsEnabled = false; });

            if (new DatabaseController().IsRealtimeFirebaseSelected())
            {
                var auth = new Authentication();
                var duration = MaskedEntryTime.Text.Split(':');
                _firebaseHasReplied = null;
                var manual = new IrrigationController.ManualSchedule
                {
                    DURATION = MaskedEntryTime.Text,
                    EndTime = DateTime.Now.AddHours(long.Parse(duration[0])).AddMinutes(long.Parse(duration[1])).Ticks,
                    RunWithSchedule = SwitchRunWithSchedule.IsToggled,
                    equipmentIdList = _queueManualSchedule.Select(queue => new ManualScheduleEquipment { ID = queue }).ToList()
                };

                var key = Task.Run(() => auth.SetManualSchedule(manual)).Result;
                auth._FirebaseClient
                    .Child(auth.getConnectedPi()).Child("Status")
                    .AsObservable<JObject>()
                    .Subscribe(x =>
                    {
                        if(x.Key != key || _firebaseHasReplied == false)
                            return;
                        _firebaseHasReplied = true;
                        
                        PopupNavigation.Instance.PopAsync();
                        var result = new Authentication().GetJsonStatusToObjectList(x.Object, x.Key);

                        Device.BeginInvokeOnMainThread(() =>{DisplayAlert(result.Operation, result.Code, "Understood");});
                        //StartManualScheduleScreen();
                    });
                        
                  

                var stopwatch = new Stopwatch();
                var floatingScreenScreen = new FloatingScreen { CloseWhenBackgroundIsClicked = false };
                PopupNavigation.Instance.PushAsync(floatingScreenScreen);
                stopwatch.Start();

                while (stopwatch.Elapsed < TimeSpan.FromSeconds(15))
                {
                    Thread.Sleep(500);
                }
                stopwatch.Stop();


                if (_firebaseHasReplied != null) return;
                {
                    PopupNavigation.Instance.PopAsync();
                    _firebaseHasReplied = false;
                    Task.Run(() => auth.DeleteManualSchedule());
                    Device.BeginInvokeOnMainThread(() =>{DisplayAlert("Reverted", "We never got a reply back", "Understood"); });
                    //StopManualScheduleScreen();

                }

            }
            else
            {
                var send = "";
                send += MaskedEntryTime.Text;


                if (SwitchRunWithSchedule.IsToggled)
                    send += ",1";
                else
                    send += ",0";

                send = _queueManualSchedule.Aggregate(send, (current, queue) => current + ("," + queue));

                send += "$MANUALSCHEDULE";

                var result = _socket.Message(send);
                    
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (result == "success")
                    {
                        _activeManualScheduleId = _queueManualSchedule;
                        DisableAllActions();
                        _buttonEnabledStatus = false;
                    }
                    else
                    {
                        DisplayAlert("Warning!!!", result, "Understood");
                    }
                });
            }
            
        }

    }
}