using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Pump.Class;
using Pump.Database;
using Pump.Droid.Database.Table;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Pump.SocketController;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Dashboard
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManualScheduleHomeScreen : ContentPage
    {
        private readonly ObservableIrrigation _observableIrrigation;
        private readonly PumpConnection _pumpConnection;
        private FloatingScreenScroll _floatingScreenScroll;
        private readonly SocketPicker _socketPicker;

        public ManualScheduleHomeScreen(ObservableIrrigation observableIrrigation, SocketPicker socketPicker)
        {
            InitializeComponent();
            _observableIrrigation = observableIrrigation;
            _socketPicker = socketPicker;
            _pumpConnection = new DatabaseController().GetControllerConnectionSelection();
            new Thread(GetManualElementsReady).Start();
        }

        private void GetManualElementsReady()
        {
            var hasSubscribed = false;
            var scheduleHasRun = false;
            while (!hasSubscribed)
            {
                try
                {
                    if (!_observableIrrigation.EquipmentList.Contains(null) && !_observableIrrigation.ManualScheduleList.Contains(null))
                    {
                        hasSubscribed = true;
                        _observableIrrigation.ManualScheduleList.CollectionChanged += PopulateManualElementsEvent;
                        Device.InvokeOnMainThreadAsync(PopulateManualElements);
                    }

                    if (_observableIrrigation.EquipmentList.Contains(null) && scheduleHasRun == false)
                    {
                        scheduleHasRun = true;
                        _observableIrrigation.EquipmentList.CollectionChanged += PopulateManualElementsEvent;
                    }
                }
                catch
                {
                    // ignored
                }
                Thread.Sleep(100);
            }
        }

        private void PopulateManualElementsEvent(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(PopulateManualElements);
        }

        private void PopulateManualElements()
        {
            ScreenCleanupForManualScreen();
            try
            {
                if (_observableIrrigation.ManualScheduleList.Contains(null) || _observableIrrigation.EquipmentList.Contains(null)) return;
                if (_observableIrrigation.EquipmentList.Any())
                {
                    foreach (var pump in _observableIrrigation.EquipmentList.Where(x => x.isPump && _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.ID)))
                    {
                        var existingButton =
                            ScrollViewManualPump.Children.FirstOrDefault(view => view.AutomationId == pump.ID);
                        if (existingButton == null)
                            ScrollViewManualPump.Children.Add(CreateEquipmentButton(pump));
                        else
                            ((Button)existingButton).Text = pump.NAME;
                    }
                    if (_observableIrrigation.EquipmentList.Count(x => x.isPump) == 0)
                    {
                        ScrollViewManualPump.Children.Add(new ViewEmptySchedule("No Pump Found Here"));
                    }
                    foreach (var zone in _observableIrrigation.EquipmentList.Where(x => !x.isPump && _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.ID)))
                    {
                        var existingButton =
                            ScrollViewManualZone.Children.FirstOrDefault(view => view.AutomationId == zone.ID);
                        if (existingButton == null)
                            ScrollViewManualZone.Children.Add(CreateEquipmentButton(zone));
                        else
                            ((Button)existingButton).Text = zone.NAME;
                    }
                    if (_observableIrrigation.EquipmentList.Count(x => x.isPump == false) == 0)
                    {
                        ScrollViewManualZone.Children.Add(new ViewEmptySchedule("No Zone Found Here"));
                    }
                }
            }
            catch
            {
                ScrollViewManualPump.Children.Add(new ViewException());
                ScrollViewManualZone.Children.Add(new ViewException());
            }

            try
            {
                //Sorts Out all the button color stuff
                SetActiveEquipmentButton();
                if(_observableIrrigation.ManualScheduleList.Contains(null))return;
                
                if (_observableIrrigation.ManualScheduleList.Count == 0)
                {
                    //Clear Manual
                    MaskedEntryTime.Text = "";
                    MaskedEntryTime.IsEnabled = true;
                    ButtonStartManual.IsEnabled = true;
                    ButtonStartManual.Text = "START";
                    SwitchRunWithSchedule.IsEnabled = true;
                    SwitchRunWithSchedule.IsToggled = true;
                }
                else
                {
                    //Populate
                    ButtonStopManual.IsEnabled = true;
                    ButtonStartManual.Text = "UPDATE";
                    MaskedEntryTime.Text = ScheduleTime.ConvertTimeSpanToString(ScheduleTime.FromUnixTimeStampUtc(GetSelectedManualSchedule().EndTime) - DateTime.UtcNow);
                    
                }
            }
            catch
            {
                // ignored
            }
        }

        private void ScreenCleanupForManualScreen()
        {
            try
            {
                if (!_observableIrrigation.EquipmentList.Contains(null))
                {

                    var pumpsThatAreOnDisplay = _observableIrrigation.EquipmentList.Where(x => x.isPump && _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.ID)).Select(x => x.ID).ToList();
                    if (pumpsThatAreOnDisplay.Count == 0)
                        pumpsThatAreOnDisplay.Add(new ViewEmptySchedule(string.Empty).ID);


                    for (var index = 0; index < ScrollViewManualPump.Children.Count; index++)
                    {
                        var existingItems = pumpsThatAreOnDisplay.FirstOrDefault(x =>
                            x == ScrollViewManualPump.Children[index].AutomationId);
                        if (existingItems != null) continue;
                        ScrollViewManualPump.Children.RemoveAt(index);
                        index--;
                    }

                    var zonesThatAreOnDisplay = _observableIrrigation.EquipmentList.Where(x => !x.isPump && _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.ID)).Select(x => x.ID).ToList();
                    if (zonesThatAreOnDisplay.Count == 0)
                        zonesThatAreOnDisplay.Add(new ViewEmptySchedule(string.Empty).ID);


                    for (var index = 0; index < ScrollViewManualZone.Children.Count; index++)
                    {
                        var existingItems = zonesThatAreOnDisplay.FirstOrDefault(x =>
                            x == ScrollViewManualZone.Children[index].AutomationId);
                        if (existingItems != null) continue;
                        ScrollViewManualZone.Children.RemoveAt(index);
                        index--;
                    }
                }
                else
                {
                    ScrollViewManualZone.Children.Clear();
                    var loadingIcon = new ActivityIndicator
                    {
                        AutomationId = "ActivityIndicatorSiteLoading",
                        HorizontalOptions = LayoutOptions.Center,
                        IsEnabled = true,
                        IsRunning = true,
                        IsVisible = true,
                        VerticalOptions = LayoutOptions.Center
                    };
                    ScrollViewManualZone.Children.Add(loadingIcon);
                }

            }
            catch
            {
                // ignored
            }
        }

        private Button CreateEquipmentButton(Equipment equipment)
        {
            var button = new Button
            {
                Text = equipment.NAME,
                AutomationId = equipment.ID,
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

        private void SetActiveEquipmentButton()
        {
            var equipmentList = ScrollViewManualPump.Children.ToList().ToList();
            equipmentList.AddRange(ScrollViewManualZone.Children.ToList());
            try
            {
                foreach (var button in equipmentList.Cast<Button>())
                {
                    if (_observableIrrigation.ManualScheduleList.Count == 0)
                    {
                        button.BackgroundColor = Color.AliceBlue;
                        continue;
                    }

                    var existing = _observableIrrigation.ManualScheduleList.FirstOrDefault(x =>
                        x.ManualDetails.Select(y => y.id_Equipment).Contains(button.AutomationId));

                    if (existing == null)
                    {
                        button.BackgroundColor = Color.AliceBlue;
                    }
                    else
                        button.BackgroundColor = Color.BlueViolet;
                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void Button_Tapped(object sender, EventArgs e)
        {
            var button = (Button) sender;
            var equipmentViewList = ScrollViewManualPump.Children.ToList().ToList();
            equipmentViewList.AddRange(ScrollViewManualZone.Children.ToList());
            var equipmentOnScreen = (Button)equipmentViewList.FirstOrDefault(x => x.AutomationId == button.AutomationId);
            if (_observableIrrigation.ManualScheduleList.Any(x => x.ManualDetails.Select(y => y.id_Equipment).Contains(button.AutomationId)))
            {
                if (equipmentOnScreen != null)
                    equipmentOnScreen.BackgroundColor =
                        button.BackgroundColor == Color.AliceBlue ? Color.BlueViolet : Color.AliceBlue;
            }
            else
            {
                if (equipmentOnScreen != null)
                    equipmentOnScreen.BackgroundColor =
                        button.BackgroundColor == Color.AliceBlue ? Color.CadetBlue : Color.AliceBlue;
            
                if (button != equipmentOnScreen)
                    button.BackgroundColor = button.BackgroundColor == Color.AliceBlue ? Color.CadetBlue : Color.AliceBlue;
            }
        }

        private async void ButtonStartManual_Clicked(object sender, EventArgs e)
        {
            var equipmentList = ScrollViewManualPump.Children.ToList().ToList();
            equipmentList.AddRange(ScrollViewManualZone.Children.ToList());

            var selectedEquipment = (from button in equipmentList.Cast<Button>() where button.BackgroundColor == Color.BlueViolet || button.BackgroundColor == Color.CadetBlue select button.AutomationId).ToList();
            if (selectedEquipment.Count < 1 || MaskedEntryTime.Text.Count() != 5)
            {
                Device.BeginInvokeOnMainThread(() => { DisplayAlert("Incomplete Operation", "Please Make sure that the Time and Equipment are selected correctly", "Understood"); });
                return;
            }
            
            var duration = MaskedEntryTime.Text.Split(':');
            var manualSchedule = _observableIrrigation.ManualScheduleList.FirstOrDefault(x =>
                x.ManualDetails.Any(y => selectedEquipment.Contains(y.id_Equipment)));

            if (manualSchedule == null)
            {
                manualSchedule = new ManualSchedule
                {
                    EndTime = ScheduleTime.GetUnixTimeStampUtcNow(TimeSpan.FromHours(long.Parse(duration[0])), TimeSpan.FromMinutes(long.Parse(duration[1]))),
                    RunWithSchedule = SwitchRunWithSchedule.IsToggled,
                    ManualDetails = selectedEquipment.Select(queue => new ManualScheduleEquipment { id_Equipment = queue }).ToList()
                };
            }
            else
            {
                manualSchedule.EndTime = ScheduleTime.GetUnixTimeStampUtcNow(
                    TimeSpan.FromHours(long.Parse(duration[0])), TimeSpan.FromMinutes(long.Parse(duration[1])));
                manualSchedule.RunWithSchedule = SwitchRunWithSchedule.IsToggled;
                manualSchedule.ManualDetails = selectedEquipment
                    .Select(queue => new ManualScheduleEquipment { id_Equipment = queue }).ToList();
            }

            var floatingScreenScreen = new FloatingScreen { CloseWhenBackgroundIsClicked = false };
            await PopupNavigation.Instance.PushAsync(floatingScreenScreen);

            var notification = await _socketPicker.SendCommand(manualSchedule);
            await PopupNavigation.Instance.PopAllAsync();
            var controllerStatus = new ControllerStatus(notification);
            
            if(controllerStatus.Accept == null)
                await DisplayAlert(controllerStatus.Operation, controllerStatus.Code, "Understood");
            else
            {
                if (!await DisplayAlert(controllerStatus.Operation, controllerStatus.Code, controllerStatus.Accept,
                    "Understood")) return;
                manualSchedule.DeleteAwaiting = true;
                await _socketPicker.SendCommand(manualSchedule);
            }
                
        }

        private async void ButtonStopManual_Clicked(object sender, EventArgs e)
        {
            var manualSchedule = GetSelectedManualSchedule();
            manualSchedule.DeleteAwaiting = true;
            var floatingScreenScreen = new FloatingScreen { CloseWhenBackgroundIsClicked = false };
            await PopupNavigation.Instance.PushAsync(floatingScreenScreen);

            var notification = await _socketPicker.SendCommand(manualSchedule);
            await PopupNavigation.Instance.PopAllAsync();
            var controllerStatus = new ControllerStatus(notification);

            if (controllerStatus.Accept == null)
                await DisplayAlert(controllerStatus.Operation, controllerStatus.Code, "Understood");
        }


        private void ScrollViewManualZoneTap_Tapped(object sender, EventArgs e)
        {
            if (_observableIrrigation.EquipmentList == null)
                return;
            var buttonList = new List<Button>();
            foreach (var zone in _observableIrrigation.EquipmentList.Where(x => x.isPump == false && _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.ID)))
            {
                var button =  CreateEquipmentButton(zone);
                button.Scale = 1.25;

                var zoneButton = ScrollViewManualZone.Children.First(x => x.AutomationId == button.AutomationId);
                button.BackgroundColor = zoneButton.BackgroundColor;
                buttonList.Add(button);

            }
            ViewTapped(buttonList);
        }

        private void ScrollViewManualPump_Tapped(object sender, EventArgs e)
        {
            if (_observableIrrigation.EquipmentList.Contains(null))
                return;
            var buttonList = new List<Button>();
            foreach (var pump in _observableIrrigation.EquipmentList.Where(x => x.isPump && _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.ID)))
            {
                var button = CreateEquipmentButton(pump);
                button.Scale = 1.25;
                var pumpButton = ScrollViewManualPump.Children.First(x => x.AutomationId == button.AutomationId);
                button.BackgroundColor = pumpButton.BackgroundColor;
                buttonList.Add(button);
            }
            ViewTapped(buttonList);
        }
        private void ViewTapped(IEnumerable<Button> equipment)
        {
            _floatingScreenScroll = new FloatingScreenScroll {IsStackLayout = false};
            _floatingScreenScroll.SetFloatingScreen(equipment);
            PopupNavigation.Instance.PushAsync(_floatingScreenScroll);
        }

        private ManualSchedule GetSelectedManualSchedule()
        {
            ManualSchedule selectedManualSchedule = null;

            foreach (var manualSchedule in _observableIrrigation.ManualScheduleList)
            {
                foreach (var manualDetail in manualSchedule.ManualDetails)
                {
                    if (_observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments
                        .Contains(manualDetail.id_Equipment))
                    {
                        selectedManualSchedule = manualSchedule;
                        break;
                    }
                    if (selectedManualSchedule != null)
                        break;
                }
            }

            return selectedManualSchedule;
        }
    }
}