using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Timers;
using Pump.Class;
using Pump.Database.Table;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Pump.SocketController;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Dashboard
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManualScheduleHomeScreen
    {
        private readonly KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation>
            _observableFilterKeyValuePair;

        private readonly SocketPicker _socketPicker;
        private FloatingScreenScroll _floatingScreenScroll;
        private Timer _timer;
        public ManualScheduleHomeScreen(
            KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation> observableFilterKeyValuePair,
            SocketPicker socketPicker)
        {
            InitializeComponent();
            _observableFilterKeyValuePair = observableFilterKeyValuePair;
            _socketPicker = socketPicker;
            _observableFilterKeyValuePair.Value.ManualScheduleList.CollectionChanged += PopulateManualElementsEvent;
            _observableFilterKeyValuePair.Value.EquipmentList.CollectionChanged += PopulateManualElementsEvent;
            PopulateManualElements();
        }
        
        private void PopulateManualElementsEvent(object sender,
            NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(PopulateManualElements);
        }

        private void PopulateManualElements()
        {
            ScreenCleanupForManualScreen();
            try
            {
                if (!_observableFilterKeyValuePair.Value.LoadedData) return;

                foreach (var equipment in _observableFilterKeyValuePair.Value.EquipmentList
                             .OrderBy(c => c.NAME.Length).ThenBy(c => c.NAME))
                {
                    var flexLayout = equipment.isPump ? ScrollViewManualPump : ScrollViewManualZone;
                    
                    var existingButton =
                        flexLayout.Children.FirstOrDefault(view => view.AutomationId == equipment.Id);
                    if (existingButton == null)
                        flexLayout.Children.Add(CreateEquipmentButton(equipment));
                    else
                        ((Button)existingButton).Text = equipment.NAME;
                }

                //The buttons do not render unless we create a view, temp fix for UWP
                if (Device.RuntimePlatform == Device.UWP && _timer == null)
                {
                    ScrollViewManualPump.Children.Add(new ViewEmptySchedule("No Pump Found Here", automationId:"-848"));
                    ScrollViewManualZone.Children.Add(new ViewEmptySchedule("No Zone Found Here", automationId:"-848"));
                    
                    _timer = new Timer(300); // 0.3 seconds
                    _timer.Elapsed += RemoveUnusedUwpView;
                    _timer.Enabled = true;
                }
                
                if (_observableFilterKeyValuePair.Value.EquipmentList.Count(x => x.isPump) == 0)
                    ScrollViewManualPump.Children.Add(new ViewEmptySchedule("No Pump Found Here"));

                if (_observableFilterKeyValuePair.Value.EquipmentList.Count(x => x.isPump == false) == 0)
                    ScrollViewManualZone.Children.Add(new ViewEmptySchedule("No Zone Found Here"));

                DisplayActiveButtons();
            }
            catch (Exception e)
            {
                ScrollViewManualPump.Children.Add(new ViewException(e));
                ScrollViewManualZone.Children.Add(new ViewException(e));
            }
            StackLayoutStatus.AddUpdateRemoveStatus(_observableFilterKeyValuePair.Value.ManualScheduleList.FirstOrDefault()?.ControllerStatus);
        }
        private void DisplayActiveButtons()
        {
            //Sorts Out all the button color stuff
            SetActiveEquipmentButton();

            if (_observableFilterKeyValuePair.Value.ManualScheduleList.Any())
            {
                //Populate
                ButtonStopManual.IsEnabled = true;
                ButtonStartManual.Text = "UPDATE";
                var selectedManualSchedule =
                    _observableFilterKeyValuePair.Value.ManualScheduleList.FirstOrDefault();
                if (selectedManualSchedule != null)
                {
                    MaskedEntryTime.Text = ScheduleTime.ConvertTimeSpanToString(
                        ScheduleTime.FromUnixTimeStampUtc(selectedManualSchedule.EndTime) - DateTime.UtcNow);
                }
            }
            else
            {
                //Clear Manual
                MaskedEntryTime.Text = "";
                MaskedEntryTime.IsEnabled = true;
                ButtonStartManual.IsEnabled = true;
                ButtonStartManual.Text = "START";
            }
        }

        private void ScreenCleanupForManualScreen()
        {
            if (_observableFilterKeyValuePair.Value.LoadedData)
            {
                var pumpsThatAreOnDisplay = _observableFilterKeyValuePair.Value.EquipmentList.Where(x => x.isPump)
                    .Select(y => y.Id).ToList();
            
                var zonesThatAreOnDisplay = _observableFilterKeyValuePair.Value.EquipmentList.Where(x => !x.isPump)
                    .Select(x => x.Id).ToList();

                if (pumpsThatAreOnDisplay.Count == 0)
                    pumpsThatAreOnDisplay.Add(new ViewEmptySchedule().AutomationId);

                ScrollViewManualPump.RemoveUnusedViews(pumpsThatAreOnDisplay);
                
                if (zonesThatAreOnDisplay.Count == 0)
                    zonesThatAreOnDisplay.Add(new ViewEmptySchedule().AutomationId);

                ScrollViewManualZone.RemoveUnusedViews(zonesThatAreOnDisplay);
            }
            else
            {
                ScrollViewManualPump.DisplayActivityLoading();
                ScrollViewManualZone.DisplayActivityLoading();
            }
        }

        private Button CreateEquipmentButton(Equipment equipment)
        {
            var button = new Button
            {
                Text = equipment.NAME,
                AutomationId = equipment.Id,
                HeightRequest = 40,
                TextColor = Color.DarkSlateBlue,
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
            var equipmentList = ScrollViewManualPump.Children
                .Where(x => x.AutomationId != "ActivityIndicatorSiteLoading" && x.AutomationId != "-849").ToList();
            equipmentList.AddRange(ScrollViewManualZone.Children.ToList());
            try
            {
                foreach (var button in equipmentList.Cast<Button>())
                {
                    if (!_observableFilterKeyValuePair.Value.ManualScheduleList.Any())
                    {
                        button.BackgroundColor = Color.AliceBlue;
                        continue;
                    }

                    var existing = _observableFilterKeyValuePair.Value.ManualScheduleList.FirstOrDefault(x =>
                        x.ManualDetails.Select(y => y.id_Equipment).Contains(button.AutomationId));
                    button.BackgroundColor = existing == null ? Color.AliceBlue : Color.BlueViolet;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void Button_Tapped(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var equipmentViewList = ScrollViewManualPump.Children.ToList().ToList();
            equipmentViewList.AddRange(ScrollViewManualZone.Children.ToList());
            var equipmentOnScreen =
                (Button)equipmentViewList.FirstOrDefault(x => x.AutomationId == button.AutomationId);
            if (_observableFilterKeyValuePair.Value.ManualScheduleList.Any(x =>
                    x.ManualDetails.Select(y => y?.id_Equipment).Contains(button.AutomationId)))
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
                    button.BackgroundColor =
                        button.BackgroundColor == Color.AliceBlue ? Color.CadetBlue : Color.AliceBlue;
            }
        }

        private async void ButtonStartManual_Clicked(object sender, EventArgs e)
        {
            var equipmentList = ScrollViewManualPump.Children.ToList().ToList();
            equipmentList.AddRange(ScrollViewManualZone.Children.ToList());

            var selectedEquipment = (from button in equipmentList.Cast<Button>()
                where button.BackgroundColor == Color.BlueViolet || button.BackgroundColor == Color.CadetBlue
                select button.AutomationId).ToList();
            if (selectedEquipment.Count < 1 || MaskedEntryTime.Text.Length != 5)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Application.Current.MainPage.DisplayAlert("Incomplete Operation",
                        "Please Make sure that the Time and Equipment are selected correctly", "Understood");
                });
                return;
            }

            var duration = MaskedEntryTime.Text.Split(':');
            var manualSchedule = _observableFilterKeyValuePair.Value.ManualScheduleList.FirstOrDefault(x =>
                x.ManualDetails.Any(y => selectedEquipment.Contains(y?.id_Equipment)));

            if (manualSchedule == null)
            {
                manualSchedule = new ManualSchedule
                {
                    EndTime = ScheduleTime.GetUnixTimeStampUtcNow(TimeSpan.FromHours(long.Parse(duration[0])),
                        TimeSpan.FromMinutes(long.Parse(duration[1]))),
                    ManualDetails = selectedEquipment
                        .Select(queue => new ManualScheduleEquipment { id_Equipment = queue }).ToList()
                };
            }
            else
            {
                manualSchedule.EndTime = ScheduleTime.GetUnixTimeStampUtcNow(
                    TimeSpan.FromHours(long.Parse(duration[0])), TimeSpan.FromMinutes(long.Parse(duration[1])));
                manualSchedule.ManualDetails = selectedEquipment
                    .Select(queue => new ManualScheduleEquipment { id_Equipment = queue }).ToList();
            }

            StackLayoutStatus.AddStatusActivityIndicator();
            await _socketPicker.SendCommand(manualSchedule, _observableFilterKeyValuePair.Key);
        }
        private async void ButtonStopManual_Clicked(object sender, EventArgs e)
        {
            var manualSchedule = _observableFilterKeyValuePair.Value.ManualScheduleList.FirstOrDefault() ??
                                 new ManualSchedule();
            manualSchedule.DeleteAwaiting = true;
            StackLayoutStatus.AddStatusActivityIndicator();
            await _socketPicker.SendCommand(manualSchedule, _observableFilterKeyValuePair.Key);
        }

        private void ScrollViewManualZoneTap_Tapped(object sender, EventArgs e)
        {
            if (_observableFilterKeyValuePair.Value.EquipmentList == null)
                return;
            var buttonList = new List<Button>();
            foreach (var zone in _observableFilterKeyValuePair.Value.EquipmentList.Where(x => x.isPump == false)
                         .OrderBy(c => c.NAME.Length).ThenBy(c => c.NAME))
            {
                var button = CreateEquipmentButton(zone);
                button.Scale = 1.25;

                var zoneButton = ScrollViewManualZone.Children.First(x => x.AutomationId == button.AutomationId);
                button.BackgroundColor = zoneButton.BackgroundColor;
                buttonList.Add(button);
            }

            ViewTapped(buttonList);
        }

        private void ScrollViewManualPump_Tapped(object sender, EventArgs e)
        {
            if (_observableFilterKeyValuePair.Value.EquipmentList.Contains(null))
                return;
            var buttonList = new List<Button>();
            foreach (var pump in _observableFilterKeyValuePair.Value.EquipmentList.Where(x => x.isPump)
                         .OrderBy(c => c.NAME.Length)
                         .ThenBy(c => c.NAME))
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
            _floatingScreenScroll = new FloatingScreenScroll { IsStackLayout = false };
            _floatingScreenScroll.SetFloatingScreen(equipment);
            PopupNavigation.Instance.PushAsync(_floatingScreenScroll);
        }
        
        private void RemoveUnusedUwpView(object sender, ElapsedEventArgs e)
        {
            var timer = (Timer)sender;
            timer.Enabled = false;
            Device.BeginInvokeOnMainThread(ScreenCleanupForManualScreen);
            
        }
    }
}