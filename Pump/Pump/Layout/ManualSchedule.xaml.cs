using Pump.Database.Table;
using Pump.Layout.Views;
using Pump.SocketController;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Pump.Database;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManualSchedule : ContentPage
    {
        readonly IrrigationCommands _command = new IrrigationCommands();
        readonly SocketMessage _socket = new SocketMessage();
        FloatingScreenScroll _floatingScreenScroll = null;
        List<string> _activeManualScheduleId = null;
        readonly List<string> _queueManualSchedule = new List<string>();
        private string _oldIrrigationPunp = null;
        private string _oldIrrigationZone = null;
        private string _oldManualSchedule = null;
        private bool _buttonEnabledStatus = true;
        public ManualSchedule()
        {
            InitializeComponent();
            //ScrollViewManualPump.Children.Clear();
            new Thread(ThreadController).Start();
        }

        private void ThreadController()
        {
            var started = false;
            var databaseController = new DatabaseController();

            Thread manualSchedule = null;
            Thread pumps = null;
            Thread zones = null;

            while (true)
            {

                if (started == false && databaseController.GetActivityStatus() != null && databaseController.GetActivityStatus().status)
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
                {
                    if (started && databaseController.GetActivityStatus() != null && databaseController.GetActivityStatus().status == false)
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
        { var running = true;
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

                            ManualScheduleClass manualScheduleClass = new ManualScheduleClass();
                            _buttonEnabledStatus = true;
                            if (manualSchedule != "No Data" && manualSchedule != "" && manualSchedule != "Data Empty")
                            {

                                _buttonEnabledStatus = false;
                                var manualScheduleDetail = manualSchedule.Split('$').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                                manualScheduleClass.RunWithSchedule = Convert.ToBoolean(Convert.ToInt32(manualScheduleDetail[0]));
                                manualScheduleClass.setEquipmentIDAndTime(manualScheduleDetail[1].Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList());
                                _activeManualScheduleId = manualScheduleClass.getEquipmentID();
                                _queueManualSchedule.Clear();
                                SwitchRunWithSchedule.IsToggled = manualScheduleClass.RunWithSchedule;
                                SwitchRunWithSchedule.IsEnabled = _buttonEnabledStatus;
                                ButtonStartManual.IsEnabled = _buttonEnabledStatus;
                                var scheduleTime = new ScheduleTime();
                                MaskedEntryTime.Text = scheduleTime.TimeDiffNow(manualScheduleClass.ScheduleTime);
                                MaskedEntryTime.IsEnabled = _buttonEnabledStatus;
                                if (_floatingScreenScroll != null)
                                {
                                    try
                                    {
                                        PopupNavigation.Instance.PopAsync();
                                    }
                                    finally
                                    {
                                        _floatingScreenScroll = null;
                                    }

                                }

                            }

                            var buttonList = ScrollViewManualPump.Children.ToList();
                            buttonList.AddRange(ScrollViewManualZone.Children.ToList());

                            var isActivityIndicator = false;

                            foreach (var button in buttonList.Where(button => button.AutomationId == "ActivityIndicatorManualZone" || button.AutomationId == "ActivityIndicatorManualPump"))
                            {
                                isActivityIndicator = true;
                            }

                            if (ScrollViewManualPump.Children.Count == 0 || ScrollViewManualZone.Children.Count == 0 || isActivityIndicator)
                                return;


                            var buttons = buttonList.Cast<Button>().Cast<object>().ToList();
                            SetButtonToDisabled(buttons);

                            if (_buttonEnabledStatus)
                                ResetAllActions();
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

        private void ResetAllActions()
        {
            _activeManualScheduleId = null;
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
            
        }

        private bool SetButtonToDisabled(IEnumerable<object> buttonList) {
            
            
            foreach (Button button in buttonList)
            {
                if (_buttonEnabledStatus == false)
                {
                    if (_activeManualScheduleId != null && _activeManualScheduleId.Contains(button.AutomationId))
                        button.BackgroundColor = Color.BlueViolet;
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

                        if (_oldIrrigationPunp == irrigationPump)
                            return;
                        ScrollViewManualPump.Children.Clear();
                        _oldIrrigationPunp = irrigationPump;

                        var irrigationPumpListObject = GetEquipmentObject(irrigationPump);
                        try
                        {
                            foreach (Button view in irrigationPumpListObject)
                            {
                                if (_activeManualScheduleId != null)
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
                                if (_activeManualScheduleId != null)
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
            Button button = new Button
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
            foreach(Button button in buttons)
            {
                button.BackgroundColor = _queueManualSchedule.Contains(button.AutomationId) ? Color.CornflowerBlue : Color.AliceBlue;
            }
            return buttons;
        }

        private void ChangeButtonStatus(string id)
        {
            var buttonList = ScrollViewManualPump.Children.ToList();
            buttonList.AddRange(ScrollViewManualZone.Children.ToList());
            try
            {

            
                foreach (Button button in buttonList)
                {
                    try
                    {
                        if (button.AutomationId == id)
                        {
                            UpdateSelectedEquipment(button);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        private void UpdateSelectedEquipment(Button button)
        {
            button.BackgroundColor = _queueManualSchedule.Contains(button.AutomationId) ? Color.AliceBlue : Color.CornflowerBlue;
        }
        
        

        private void Button_Tapped(object sender, EventArgs e)
        {
            if (_activeManualScheduleId != null)
                return;
            var button = (Button)sender;

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

            ViewTapped(GetEquipmentObject(_oldIrrigationPunp));
            
        }

        private void ViewTapped(List<Object> equipment)
        {
            _floatingScreenScroll = new FloatingScreenScroll();
            if (SetButtonToDisabled(equipment))
                equipment = ButtonSelected(equipment);
            _floatingScreenScroll.setFloatingScreen(equipment);
            PopupNavigation.Instance.PushAsync(_floatingScreenScroll);
        }

        private void StopManualSchedule()
        {
            try
            {
                _socket.Message(_command.StopManualSchedule());

                Device.BeginInvokeOnMainThread(() =>
                {

                    _queueManualSchedule.Clear();
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

        private void StartManualSchedule()
        {
            if(_queueManualSchedule.Count<1 && MaskedEntryTime.Text.Count() < 4)
            {
                return;
            }

            var send = "";
            send += MaskedEntryTime.Text;


            if (SwitchRunWithSchedule.IsToggled)
            {
                send += ",1";
            }
            else
            {
                send += ",0";
            }

            foreach(var queue in _queueManualSchedule)
            {
                send += "," + queue;
            }

            send += "$MANUALSCHEDULE";

            var result = _socket.Message(send);

            Device.BeginInvokeOnMainThread(() =>
            {
                if (result == "success")
                {
                    _activeManualScheduleId = _queueManualSchedule;
                    DisableAllActions();
                    _buttonEnabledStatus = false;

                    var buttonList = ScrollViewManualPump.Children.ToList();
                    buttonList.AddRange(ScrollViewManualZone.Children.ToList());

                    List<object> buttons = buttonList.Cast<Button>().Cast<object>().ToList();
                    if (SetButtonToDisabled(buttons))
                        buttons = ButtonSelected(buttons);
                    _queueManualSchedule.Clear();
                }
                else
                {
                    DisplayAlert("Warning!!!", result, "Understood");
                }


            });

        }
    }


    
}