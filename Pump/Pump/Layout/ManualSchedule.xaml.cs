using Pump.Database.Table;
using Pump.Layout.Views;
using Pump.SocketController;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pump.Database;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManualSchedule : ContentPage
    {
        SocketCommands command = new SocketCommands();
        SocketMessage socket = new SocketMessage();
        FloatingScreen FloatingScreen = null;
        List<string> ActiveManualScheduleID = null;
        List<string> QueueManualSchedule = new List<string>();
        string oldIrrigationPunp = null;
        string oldIrrigationZone = null;
        string oldManualSchedule = null;
        bool buttonEnabledStatus = true;
        public ManualSchedule()
        {
            InitializeComponent();
            //ScrollViewManualPump.Children.Clear();
            new Thread(() => ThreadController()).Start();
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
                    manualSchedule = new Thread(() => getManualSchedule());
                    pumps = new Thread(() => getPumps());
                    zones = new Thread(() => getZones());

                    manualSchedule.Start();
                    pumps.Start();
                    zones.Start();
                    started = true;
                }

                if (manualSchedule != null && pumps != null && zones != null)
                {
                    if (started == true && databaseController.GetActivityStatus() != null && databaseController.GetActivityStatus().status == false)
                    {
                        manualSchedule.Abort();
                        pumps.Abort();
                        zones.Abort();
                        started = false;
                        //Stop the threads
                    }

                }
                Thread.Sleep(2000);
            }
        }


        private void getManualSchedule()
        { var running = true;
            while (running)
            {
                try
                {
                    string ManualSchedule = socket.Message(command.getManualSchedule());
                    //Thread.Sleep(3000);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (oldManualSchedule == ManualSchedule)
                            return;
                        oldManualSchedule = ManualSchedule;

                        ManualScheduleClass manualScheduleClass = new ManualScheduleClass();
                        buttonEnabledStatus = true;
                        if (ManualSchedule != "No Data" && ManualSchedule != "" && ManualSchedule != "Data Empty")
                        {

                            buttonEnabledStatus = false;
                            var ManualScheduleDetail = ManualSchedule.Split('$').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                            manualScheduleClass.RunWithSchedule = Convert.ToBoolean(Convert.ToInt32(ManualScheduleDetail[0]));
                            manualScheduleClass.setEquipmentIDAndTime (ManualScheduleDetail[1].Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList());
                            ActiveManualScheduleID = manualScheduleClass.getEquipmentID();
                            QueueManualSchedule.Clear();
                            SwitchRunWithSchedule.IsToggled = manualScheduleClass.RunWithSchedule;
                            SwitchRunWithSchedule.IsEnabled = buttonEnabledStatus;
                            ButtonStartManual.IsEnabled = buttonEnabledStatus;
                            ScheduleTime scheduleTime = new ScheduleTime();
                            MaskedEntryTime.Text = scheduleTime.TimeDiffNow(manualScheduleClass.ScheduleTime);
                            MaskedEntryTime.IsEnabled = buttonEnabledStatus;
                            if(FloatingScreen != null)
                            {
                                try
                                {
                                    PopupNavigation.Instance.PopAsync();
                                }
                                finally
                                {
                                    FloatingScreen = null;
                                }
                                
                            }
                            
                        }
                        
                        var buttonList = ScrollViewManualPump.Children.ToList();
                        buttonList.AddRange(ScrollViewManualZone.Children.ToList());

                        bool isActivityIndicator = false;
                        
                        foreach (var button in buttonList)
                        {
                            
                            if(button.AutomationId == "ActivityIndicatorManualZone" || button.AutomationId == "ActivityIndicatorManualPump")
                            {
                                isActivityIndicator = true;
                            }
                        }

                        if (ScrollViewManualPump.Children.Count == 0 || ScrollViewManualZone.Children.Count == 0 || isActivityIndicator == true)
                            return;

                        
                        List<Object> buttons = new List<object>();
                        foreach (Button button in buttonList)
                        {
                            buttons.Add(button);
                        }
                        setButtonToDisabled(buttons);

                        if (buttonEnabledStatus)
                            resetAllActions();

                    });
                }
                catch (ThreadAbortException)
                {
                    running = false;
                }
                catch
                {
                    
                }
                Thread.Sleep(15000);
            }
        }

        private void resetAllActions()
        {
            ActiveManualScheduleID = null;
            MaskedEntryTime.Text = "";
            MaskedEntryTime.IsEnabled = true;
            ButtonStartManual.IsEnabled = true;
            SwitchRunWithSchedule.IsEnabled = true;
            SwitchRunWithSchedule.IsToggled = true;
        }

        private void disableAllActions()
        {
            
            MaskedEntryTime.IsEnabled = false;
            ButtonStartManual.IsEnabled = false;
            SwitchRunWithSchedule.IsEnabled = false;
            
        }

        private bool setButtonToDisabled(List<Object> buttonList) {
            
            
            foreach (Button button in buttonList)
            {
                if (buttonEnabledStatus == false)
                {
                    if (ActiveManualScheduleID != null && ActiveManualScheduleID.Contains(button.AutomationId))
                        button.BackgroundColor = Color.BlueViolet;
                    else
                    {
                        button.IsEnabled = buttonEnabledStatus;
                        button.BackgroundColor = Color.AliceBlue;
                    }

                }
                else
                {
                    button.IsEnabled = buttonEnabledStatus;
                    button.BackgroundColor = Color.AliceBlue;
                }
                
            }
            return buttonEnabledStatus;
        }

        private void getPumps()
        {
            var running = true;
            while (running)
            {
                try
                {
                    Thread.Sleep(10);
                    string IrrigationPunp = socket.Message(command.getPumps());
                    
                    Device.BeginInvokeOnMainThread(() =>
                    {

                        if (oldIrrigationPunp == IrrigationPunp)
                            return;
                        ScrollViewManualPump.Children.Clear();
                        oldIrrigationPunp = IrrigationPunp;

                        var IrrigationPunpListObject = getEquipmentObject(IrrigationPunp);
                        foreach (Button view in IrrigationPunpListObject)
                        {
                            if(ActiveManualScheduleID != null)
                            {
                                if (ActiveManualScheduleID.Contains(view.AutomationId))
                                    view.BackgroundColor = Color.BlueViolet;
                                else
                                    view.IsEnabled = false;


                            }
                            ScrollViewManualPump.Children.Add(view);
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
                        ScrollViewManualPump.Children.Add(new ViewNoConnection());
                    });

                }
                Thread.Sleep(30000);
            }
        }

        private List<object> getEquipmentObject(string IrrigationPunp)
        {
            List<object> EquipmentListObject = new List<object>();
            try
            {
                if (IrrigationPunp == "No Data" || IrrigationPunp == "")
                {
                    EquipmentListObject.Add(new ViewEmptySchedule("No Equipment Found Here"));
                    return EquipmentListObject;
                }

                var PumpList = IrrigationPunp.Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                foreach (var pump in PumpList)
                {
                    EquipmentListObject.Add(createButton(pump.Split(',').ToList()));
                }

                return EquipmentListObject;
            }
            catch
            {
                EquipmentListObject.Add(new ViewNoConnection());
                return EquipmentListObject;
            }

        }

        private void getZones()
        {
            var running = true;
            while (running)
            {
                try
                {
                    Thread.Sleep(10);
                    string IrrigationZone = socket.Message(command.getValves());

                    Device.BeginInvokeOnMainThread(() =>
                    {

                        if (oldIrrigationZone == IrrigationZone)
                            return;
                        ScrollViewManualZone.Children.Clear();
                        oldIrrigationZone = IrrigationZone;

                        var IrrigationZoneListObject = getEquipmentObject(IrrigationZone);
                        
                        foreach (Button view in IrrigationZoneListObject)
                        {
                            if (ActiveManualScheduleID != null)
                            {
                                if (ActiveManualScheduleID.Contains(view.AutomationId))
                                    view.BackgroundColor = Color.BlueViolet;
                                else
                                    view.IsEnabled = false;
                            }
                            ScrollViewManualZone.Children.Add(view);
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
                        ScrollViewManualZone.Children.Add(new ViewNoConnection());
                    });

                }
                Thread.Sleep(30000);
            }
        }

        private Button createButton(List<string> equipment)
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

        private List<Object> ButtonSelected(List<Object> buttons)
        {
            foreach(Button button in buttons)
            {
                if (QueueManualSchedule.Contains(button.AutomationId))
                    button.BackgroundColor = Color.CornflowerBlue;

                else
                    button.BackgroundColor = Color.AliceBlue;
            }
            return buttons;
        }

        private void changeButtonStatus(string id)
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
                        updateSelectedEquipment(button);
                    }
                }
                finally
                {

                }
                }
            }
            catch
            {

            }
        }

        private void updateSelectedEquipment(Button button)
        {
            
            if (QueueManualSchedule.Contains(button.AutomationId))
                button.BackgroundColor = Color.AliceBlue;
            
            else
                button.BackgroundColor = Color.CornflowerBlue;
            
        }
        
        

        private void Button_Tapped(object sender, EventArgs e)
        {
            if (ActiveManualScheduleID != null)
                return;
            var button = (Button)sender;

            changeButtonStatus(button.AutomationId);



            if (QueueManualSchedule.Contains(button.AutomationId))
            {
                QueueManualSchedule.Remove(button.AutomationId);
                button.BackgroundColor = Color.AliceBlue;
            }
            else
            {
                button.BackgroundColor = Color.CornflowerBlue;
                QueueManualSchedule.Add(button.AutomationId);
            }
            
        }

        private void ButtonStartManual_Clicked(object sender, EventArgs e)
        {
            new Thread(() => startManualSchedule()).Start();
        }

        private void ButtonStopManual_Clicked(object sender, EventArgs e)
        {
            new Thread(() => stopManualSchedule()).Start();
        }

        private void ScrollViewManualZoneTap_Tapped(object sender, EventArgs e)
        {
            if (oldIrrigationZone == null)
                return;

            viewTapped(getEquipmentObject(oldIrrigationZone));
        }

        private void ScrollViewManualPump_Tapped(object sender, EventArgs e)
        {
            if (oldIrrigationZone == null)
                return;

            viewTapped(getEquipmentObject(oldIrrigationPunp));
            
        }

        private void viewTapped(List<Object> Equipment)
        {
            FloatingScreen = new FloatingScreen();
            if (setButtonToDisabled(Equipment))
                Equipment = ButtonSelected(Equipment);
            FloatingScreen.setFloatingScreen(Equipment);
            PopupNavigation.Instance.PushAsync(FloatingScreen);
        }

        private void stopManualSchedule()
        {
            try
            {
                string StopeManual = socket.Message(command.StopManualSchedule());

                Device.BeginInvokeOnMainThread(() =>
                {

                    QueueManualSchedule.Clear();
                    buttonEnabledStatus = true;
                    resetAllActions();
                    var buttonList = ScrollViewManualPump.Children.ToList();
                    buttonList.AddRange(ScrollViewManualZone.Children.ToList());
                    
                    List<Object> buttons = new List<object>();
                    foreach (Button button in buttonList)
                    {
                        buttons.Add(button);
                    }
                    if (setButtonToDisabled(buttons))
                        buttons = ButtonSelected(buttons);
                    
                    oldManualSchedule = "Data Empty";
                });

            }
            catch
            {

            }            

        }

        private void startManualSchedule()
        {
            if(QueueManualSchedule.Count<1 && MaskedEntryTime.Text.Count() < 4)
            {
                return;
            }

            String send = "";
            send = send + MaskedEntryTime.Text.ToString();


            if (SwitchRunWithSchedule.IsToggled)
            {
                send = send + ",1";
            }
            else
            {
                send = send + ",0";
            }

            foreach(string queue in QueueManualSchedule)
            {
                send = send + "," + queue;
            }

            send = send + "$MANUALSCHEDULE";

            string result = socket.Message(send);

            Device.BeginInvokeOnMainThread(() =>
            {
                if (result == "success")
                {
                    ActiveManualScheduleID = QueueManualSchedule;
                    disableAllActions();
                    buttonEnabledStatus = false;

                    var buttonList = ScrollViewManualPump.Children.ToList();
                    buttonList.AddRange(ScrollViewManualZone.Children.ToList());

                    List<Object> buttons = new List<object>();
                    foreach (Button button in buttonList)
                    {
                        buttons.Add(button);
                    }
                    if (setButtonToDisabled(buttons))
                        buttons = ButtonSelected(buttons);
                    QueueManualSchedule.Clear();
                }
                else
                {
                    DisplayAlert("Warning!!!", result, "Understood");
                }


            });

        }
    }


    
}