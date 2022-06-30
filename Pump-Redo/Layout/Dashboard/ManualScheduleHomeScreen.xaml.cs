using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Pump.Class;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Pump.SocketController;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Dashboard
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManualScheduleHomeScreen : ContentView
    {
        private readonly ObservableSiteFilteredIrrigation _observableFilteredIrrigation;
        private readonly SocketPicker _socketPicker;
        private FloatingScreenScroll _floatingScreenScroll;

        public ManualScheduleHomeScreen(ObservableSiteFilteredIrrigation observableFilteredIrrigation, SocketPicker socketPicker)
        {
            InitializeComponent();
            _observableFilteredIrrigation = observableFilteredIrrigation;
            _socketPicker = socketPicker;
            _observableFilteredIrrigation.ManualScheduleList.CollectionChanged += PopulateManualElementsEvent;
            _observableFilteredIrrigation.EquipmentList.CollectionChanged += PopulateManualElementsEvent;
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
                if (!_observableFilteredIrrigation.LoadedAllData()) return;

                foreach (var pump in _observableFilteredIrrigation.EquipmentList.Where(x => x.isPump)
                             .OrderBy(c => c.NAME.Length).ThenBy(c => c.NAME))
                {
                    var existingButton =
                        ScrollViewManualPump.Children.FirstOrDefault(view => view.AutomationId == pump.Id);
                    if (existingButton == null)
                        ScrollViewManualPump.Children.Add(CreateEquipmentButton(pump));
                    else
                        ((Button)existingButton).Text = pump.NAME;
                }

                if (_observableFilteredIrrigation.EquipmentList.Count(x => x.isPump) == 0)
                    ScrollViewManualPump.Children.Add(new ViewEmptySchedule("No Pump Found Here"));
                foreach (var zone in _observableFilteredIrrigation.EquipmentList.Where(x => !x.isPump)
                             .OrderBy(c => c.NAME.Length).ThenBy(c => c.NAME).ToList())
                {
                    var existingButton =
                        ScrollViewManualZone.Children.FirstOrDefault(view => view.AutomationId == zone.Id);
                    if (existingButton == null)
                        ScrollViewManualZone.Children.Add(CreateEquipmentButton(zone));
                    else
                        ((Button)existingButton).Text = zone.NAME;
                }

                if (_observableFilteredIrrigation.EquipmentList.Count(x => x.isPump == false) == 0)
                    ScrollViewManualZone.Children.Add(new ViewEmptySchedule("No Zone Found Here"));
            }
            catch (Exception e)
            {
                ScrollViewManualPump.Children.Add(new ViewException(e));
                ScrollViewManualZone.Children.Add(new ViewException(e));
            }

            try
            {
                //Sorts Out all the button color stuff
                SetActiveEquipmentButton();

                if (_observableFilteredIrrigation.ManualScheduleList.Any())
                {
                    //Populate
                    ButtonStopManual.IsEnabled = true;
                    ButtonStartManual.Text = "UPDATE";
                    var selectedManualSchedule = _observableFilteredIrrigation.ManualScheduleList.FirstOrDefault();
                    if (selectedManualSchedule != null)
                        MaskedEntryTime.Text = ScheduleTime.ConvertTimeSpanToString(
                            ScheduleTime.FromUnixTimeStampUtc(selectedManualSchedule.EndTime) - DateTime.UtcNow);
                }
                else
                {
                    //Clear Manual
                    MaskedEntryTime.Text = "";
                    MaskedEntryTime.IsEnabled = true;
                    ButtonStartManual.IsEnabled = true;
                    ButtonStartManual.Text = "START";
                    //Does This Still Matter?
                    SwitchRunWithSchedule.IsEnabled = true;
                    SwitchRunWithSchedule.IsToggled = true;
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
                if (_observableFilteredIrrigation.LoadedAllData())
                {
                    var pumpsThatAreOnDisplay = _observableFilteredIrrigation.EquipmentList.Where(x => x.isPump)
                        .Select(y => y.Id).ToList();
                    if (pumpsThatAreOnDisplay.Count == 0)
                        pumpsThatAreOnDisplay.Add(new ViewEmptySchedule().AutomationId);


                    for (var index = 0; index < ScrollViewManualPump.Children.Count; index++)
                    {
                        var existingItems = pumpsThatAreOnDisplay.FirstOrDefault(x =>
                            x == ScrollViewManualPump.Children[index].AutomationId);
                        if (existingItems != null) continue;
                        ScrollViewManualPump.Children.RemoveAt(index);
                        index--;
                    }

                    var zonesThatAreOnDisplay = _observableFilteredIrrigation.EquipmentList.Where(x => !x.isPump)
                        .Select(x => x.Id).ToList();
                    if (zonesThatAreOnDisplay.Count == 0)
                        zonesThatAreOnDisplay.Add(new ViewEmptySchedule().AutomationId);


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
                    var loadingIcon = new ActivityIndicator
                    {
                        AutomationId = "ActivityIndicatorSiteLoading",
                        HorizontalOptions = LayoutOptions.Center,
                        IsEnabled = true,
                        IsRunning = true,
                        IsVisible = true,
                        VerticalOptions = LayoutOptions.Center
                    };

                    if (ScrollViewManualZone.Children.Count > 0 && ScrollViewManualZone.Children.First().AutomationId !=
                        "ActivityIndicatorSiteLoading")
                    {
                        ScrollViewManualZone.Children.Clear();
                        ScrollViewManualZone.Children.Add(loadingIcon);
                    }

                    if (ScrollViewManualPump.Children.Count > 0 && ScrollViewManualPump.Children.First().AutomationId !=
                        "ActivityIndicatorSiteLoading")
                    {
                        ScrollViewManualPump.Children.Clear();
                        ScrollViewManualPump.Children.Add(loadingIcon);
                    }
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
                    if (!_observableFilteredIrrigation.ManualScheduleList.Any())
                    {
                        button.BackgroundColor = Color.AliceBlue;
                        continue;
                    }

                    var existing = _observableFilteredIrrigation.ManualScheduleList.FirstOrDefault(x =>
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
            if (_observableFilteredIrrigation.ManualScheduleList.Any(x =>
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
            if (selectedEquipment.Count < 1 || MaskedEntryTime.Text.Count() != 5)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Application.Current.MainPage.DisplayAlert("Incomplete Operation",
                        "Please Make sure that the Time and Equipment are selected correctly", "Understood");
                });
                return;
            }

            var duration = MaskedEntryTime.Text.Split(':');
            var manualSchedule = _observableFilteredIrrigation.ManualScheduleList.FirstOrDefault(x =>
                x.ManualDetails.Any(y => selectedEquipment.Contains(y?.id_Equipment)));

            if (manualSchedule == null)
            {
                manualSchedule = new ManualSchedule
                {
                    EndTime = ScheduleTime.GetUnixTimeStampUtcNow(TimeSpan.FromHours(long.Parse(duration[0])),
                        TimeSpan.FromMinutes(long.Parse(duration[1]))),
                    RunWithSchedule = SwitchRunWithSchedule.IsToggled,
                    ManualDetails = selectedEquipment
                        .Select(queue => new ManualScheduleEquipment { id_Equipment = queue }).ToList()
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

            await _socketPicker.SendCommand(manualSchedule);
        }

        private async void ButtonStopManual_Clicked(object sender, EventArgs e)
        {
            var manualSchedule = _observableFilteredIrrigation.ManualScheduleList.FirstOrDefault() ?? new ManualSchedule();
            manualSchedule.DeleteAwaiting = true;
            await _socketPicker.SendCommand(manualSchedule);
        }


        private void ScrollViewManualZoneTap_Tapped(object sender, EventArgs e)
        {
            if (_observableFilteredIrrigation.EquipmentList == null)
                return;
            var buttonList = new List<Button>();
            foreach (var zone in _observableFilteredIrrigation.EquipmentList.Where(x => x.isPump == false)
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
            if (_observableFilteredIrrigation.EquipmentList.Contains(null))
                return;
            var buttonList = new List<Button>();
            foreach (var pump in _observableFilteredIrrigation.EquipmentList.Where(x => x.isPump).OrderBy(c => c.NAME.Length)
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
    }
}