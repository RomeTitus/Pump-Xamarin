using Pump.Database.Table;
using Pump.Layout.Views;
using Pump.SocketController;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManualSchedule : ContentPage
    {
        SocketCommands command = new SocketCommands();
        SocketMessage socket = new SocketMessage();
        List<string> ActiveManualScheduleID = null;
        List<string> QueueManualSchedule = new List<string>();
        string oldIrrigationPunp = null;
        string oldIrrigationZone = null;
        string oldManualSchedule = null;
        public ManualSchedule()
        {
            InitializeComponent();
            ScrollViewManualPump.Children.Clear();

            //new Thread(() => getManualSchedule()).Start();
            new Thread(() => getPumps()).Start();
            new Thread(() => getZones()).Start();
            
        }

        private void getManualSchedule()
        {
            while (true)
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
                        bool buttonEnabledStatus = true;
                        if (ManualSchedule != "No Data" || ManualSchedule != "")
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

                        foreach (Button button in buttonList)
                        {
                            if(buttonEnabledStatus == false && ActiveManualScheduleID != null && ActiveManualScheduleID.Contains(button.AutomationId))
                                button.BackgroundColor = Color.BlueViolet;
                            else
                            {
                                button.IsEnabled = buttonEnabledStatus;
                                button.BackgroundColor = Color.AliceBlue;
                            }
                        }
                        if(!buttonEnabledStatus)
                            ActiveManualScheduleID = null;
                            //LableTimeDuration.Text = "Duration: ";
                            //MaskedEntryTime.Text = "";


                    });
                }
                catch
                {
                    
                }
                Thread.Sleep(15000);
            }
        }

        private void getPumps()
        {
            while (true)
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
                    EquipmentListObject.Add(new ViewEmptySchedule());
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
            while (true)
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

            foreach (Button button in buttonList)
            {
                if(button.AutomationId == id)
                {
                    updateSelectedEquipment(button);
                }
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

        }

        private void ButtonStopManual_Clicked(object sender, EventArgs e)
        {

        }

        private void ScrollViewManualZoneTap_Tapped(object sender, EventArgs e)
        {
            if (oldIrrigationZone == null)
                return;
            var FloatingScreen = new FloatingScreen();
            var ZoneList = getEquipmentObject(oldIrrigationZone);
            ZoneList = ButtonSelected(ZoneList);
            FloatingScreen.setFloatingScreen(ZoneList);
            PopupNavigation.Instance.PushAsync(FloatingScreen);
        }

        private void ScrollViewManualPump_Tapped(object sender, EventArgs e)
        {
            if (oldIrrigationZone == null)
                return;
            var FloatingScreen = new FloatingScreen();
            var PumpList = getEquipmentObject(oldIrrigationPunp);
            PumpList = ButtonSelected(PumpList);
            FloatingScreen.setFloatingScreen(PumpList);
            PopupNavigation.Instance.PushAsync(FloatingScreen);
        }
    }


    
}