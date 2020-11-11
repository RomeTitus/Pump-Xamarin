﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Database.Query;
using Pump.Class;
using Pump.Database;
using Pump.Droid.Database.Table;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ManualScheduleHomeScreen : ContentPage
    {
        private readonly ObservableCollection<ManualSchedule> _manualScheduleList;
        private readonly ObservableCollection<Equipment> _equipmentList;
        private readonly ObservableCollection<Site> _siteList;
        private readonly PumpConnection PumpConnection;

        private FloatingScreenScroll _floatingScreenScroll;
        private bool? _firebaseHasReplied = false;



        public ManualScheduleHomeScreen(ObservableCollection<ManualSchedule> manualScheduleList, ObservableCollection<Equipment> equipmentList,
            ObservableCollection<Site> siteList)
        {
            _manualScheduleList = manualScheduleList;
            _equipmentList = equipmentList;
            _siteList = siteList;
            InitializeComponent();
            PumpConnection = new DatabaseController().GetControllerConnectionSelection();
            new Thread(GetManualElementsReady).Start();
        }
        private void GetManualElementsReady()
        {
            bool hasSubscribed = false;
            bool scheduleHasRun = false;
            while (!hasSubscribed)
            {
                try
                {
                    if (!_equipmentList.Contains(null) && !_manualScheduleList.Contains(null))
                    {
                        hasSubscribed = true;
                        _manualScheduleList.CollectionChanged += PopulateManualElementsEvent;
                        Device.InvokeOnMainThreadAsync(PopulateManualElements);

                    }

                    if (_equipmentList.Contains(null) && scheduleHasRun == false)
                    {
                        scheduleHasRun = true;
                        _equipmentList.CollectionChanged += PopulateManualElementsEvent;
                    }
                }
                catch
                {
                    // ignored
                }
                Thread.Sleep(100); //Change to 100 when you can
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
                if (_manualScheduleList.Contains(null) || _equipmentList.Contains(null)) return;
                if (_equipmentList.Any())
                {
                    foreach (var pump in _equipmentList.Where(x => x.isPump && _siteList.First(y => y.ID == PumpConnection.SiteSelectedId).Attachments.Contains(x.ID)))
                    {
                        var existingButton =
                            ScrollViewManualPump.Children.FirstOrDefault(view => view.AutomationId == pump.ID);
                        if (existingButton == null)
                            ScrollViewManualPump.Children.Add(CreateEquipmentButton(pump));
                        else
                            ((Button)existingButton).Text = pump.NAME;
                    }
                    if (_equipmentList.Count(x => x.isPump) == 0)
                    {
                        ScrollViewManualPump.Children.Add(new ViewEmptySchedule("No Pump Found Here"));
                    }
                    foreach (var zone in _equipmentList.Where(x => !x.isPump && _siteList.First(y => y.ID == PumpConnection.SiteSelectedId).Attachments.Contains(x.ID)))
                    {
                        var existingButton =
                            ScrollViewManualZone.Children.FirstOrDefault(view => view.AutomationId == zone.ID);
                        if (existingButton == null)
                            ScrollViewManualZone.Children.Add(CreateEquipmentButton(zone));
                        else
                            ((Button)existingButton).Text = zone.NAME;
                    }
                    if (_equipmentList.Count(x => x.isPump == false) == 0)
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
                if(_manualScheduleList.Contains(null))return;
                
                if (_manualScheduleList.Count == 0)
                {
                    //Clear Manual
                    ButtonStopManual.IsEnabled = false;
                    MaskedEntryTime.Text = "";
                    MaskedEntryTime.IsEnabled = true;
                    ButtonStartManual.IsEnabled = true;
                    SwitchRunWithSchedule.IsEnabled = true;
                    SwitchRunWithSchedule.IsToggled = true;
                }
                else
                {
                    //Populate :/
                    ButtonStopManual.IsEnabled = true;
                    SwitchRunWithSchedule.IsToggled = _manualScheduleList[0].RunWithSchedule;
                    SwitchRunWithSchedule.IsEnabled = false;
                    MaskedEntryTime.IsEnabled = false;
                    MaskedEntryTime.Text = ScheduleTime.ConvertTimeSpanToString(ScheduleTime.FromUnixTimeStampUtc(_manualScheduleList[0].EndTime) - DateTime.UtcNow);
                    ButtonStartManual.IsEnabled = false;
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
                if (!_equipmentList.Contains(null))
                {

                    var pumpsThatAreOnDisplay = _equipmentList.Where(x => x.isPump).Select(x => x.ID).ToList();
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

                    var zonesThatAreOnDisplay = _equipmentList.Where(x => !x.isPump).Select(x => x.ID).ToList();
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
                    if (_manualScheduleList.Count == 0)
                    {
                        button.BackgroundColor = Color.AliceBlue;
                        button.IsEnabled = true;
                        continue;
                    }

                    var existing = _manualScheduleList.FirstOrDefault(x =>
                        x.ManualDetails.Select(y => y.id_Equipment).Contains(button.AutomationId));

                    if (existing == null)
                    {
                        button.BackgroundColor = Color.AliceBlue;
                        button.IsEnabled = false;
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
            if (_manualScheduleList.Count > 0)
                return;
            var button = (Button)sender;
            var equipmentList = ScrollViewManualPump.Children.ToList().ToList();
            equipmentList.AddRange(ScrollViewManualZone.Children.ToList());
            var equipmentOnScreen = (Button)equipmentList.FirstOrDefault(x => x.AutomationId == button.AutomationId);
            if (equipmentOnScreen != null)
                equipmentOnScreen.BackgroundColor = button.BackgroundColor == Color.BlueViolet ? Color.AliceBlue : Color.BlueViolet;
            
            if (button != equipmentOnScreen)
                button.BackgroundColor = button.BackgroundColor == Color.BlueViolet ? Color.AliceBlue : Color.BlueViolet;
        }

        private void ButtonStartManual_Clicked(object sender, EventArgs e)
        {
            new Thread(StartManualSchedule).Start();
        }

        private void StartManualSchedule()
        {
            var equipmentList = ScrollViewManualPump.Children.ToList().ToList();
            equipmentList.AddRange(ScrollViewManualZone.Children.ToList());

            var selectedEquipment = (from button in equipmentList.Cast<Button>() where button.BackgroundColor == Color.BlueViolet select button.AutomationId).ToList();

            if (selectedEquipment.Count < 1 && MaskedEntryTime.Text.Count() < 4)
            {
                Device.BeginInvokeOnMainThread(() => { DisplayAlert("Incomplete Operation", "Please Make sure that the Time and Equipment are selected correctly", "Understood"); });
                return;
            }

            Device.BeginInvokeOnMainThread(() => { ButtonStartManual.IsEnabled = false; });

            if (new DatabaseController().IsRealtimeFirebaseSelected())
            {
                var auth = new Authentication();
                var duration = MaskedEntryTime.Text.Split(':');
                _firebaseHasReplied = null;
                var manual = new ManualSchedule
                {
                    EndTime = ScheduleTime.GetUnixTimeStampUtcNow(TimeSpan.FromHours(long.Parse(duration[0])), TimeSpan.FromMinutes(long.Parse(duration[1]))),
                    RunWithSchedule = SwitchRunWithSchedule.IsToggled,
                    ManualDetails = selectedEquipment.Select(queue => new ManualScheduleEquipment { id_Equipment = queue }).ToList()
                };

                var key = Task.Run(() => auth.SetManualSchedule(manual)).Result;
                auth._FirebaseClient
                    .Child(auth.getConnectedPi()).Child("Status")
                    .AsObservable<ControllerStatus>()
                    .Subscribe(x =>
                    {
                        try
                        {
                            if (x.Key != key || _firebaseHasReplied == false)
                                return;
                            _firebaseHasReplied = true;
                            PopupNavigation.Instance.PopAsync();
                            var result = x.Object;
                            result.ID = x.Key;
                            Device.BeginInvokeOnMainThread(() => { DisplayAlert(result.Operation, result.Code, "Understood"); });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    });

                var stopwatch = new Stopwatch();
                var floatingScreenScreen = new FloatingScreen { CloseWhenBackgroundIsClicked = false };
                PopupNavigation.Instance.PushAsync(floatingScreenScreen);
                stopwatch.Start();

                while (stopwatch.Elapsed < TimeSpan.FromSeconds(20))
                {
                    Thread.Sleep(500);
                }
                stopwatch.Stop();


                if (_firebaseHasReplied != null) return;
                {
                    PopupNavigation.Instance.PopAsync();
                    _firebaseHasReplied = false;
                    Device.BeginInvokeOnMainThread(async () => { if(await DisplayAlert("No Reply :/", "We never got a reply back\nWould you like to keep your changes?", "I'm a Dev", "Revert"))
                         await Task.Run(() => auth.DeleteManualSchedule());});
                }
            }
        }

        private void ButtonStopManual_Clicked(object sender, EventArgs e)
        {
            new Thread(StopManualSchedule).Start();
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
                    .AsObservable<ControllerStatus>()
                    .Subscribe(x =>
                    {
                        try
                        {
                            if (x.Key != key || _firebaseHasReplied == false)
                                return;
                            _firebaseHasReplied = true;

                            PopupNavigation.Instance.PopAsync();
                            var result = x.Object;
                            result.ID = x.Key;
                            if (result.Code == "success")
                            {
                                Device.BeginInvokeOnMainThread(() => { DisplayAlert(result.Operation, result.Code, "Understood"); });
                                return;
                            }
                            Device.BeginInvokeOnMainThread(() => { DisplayAlert(result.Operation, result.Code, "Understood"); });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
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
                PopupNavigation.Instance.PopAsync();
                _firebaseHasReplied = false;
                Device.BeginInvokeOnMainThread(() => { DisplayAlert("Cleared!", "We never got a reply back", "Understood"); });
            }
        }

        private void ScrollViewManualZoneTap_Tapped(object sender, EventArgs e)
        {
            if (_equipmentList == null)
                return;
            var buttonList = new List<Button>();
            foreach (var zone in _equipmentList.Where(x => !x.isPump))
            {
                var button =  CreateEquipmentButton(zone);
                button.Scale = 1.25;

                var zoneButton = ScrollViewManualZone.Children.First(x => x.AutomationId == button.AutomationId);
                button.BackgroundColor = zoneButton.BackgroundColor;
                button.IsEnabled = zoneButton.IsEnabled;
                buttonList.Add(button);

            }
            ViewTapped(buttonList);
        }

        private void ScrollViewManualPump_Tapped(object sender, EventArgs e)
        {
            if (_equipmentList == null)
                return;
            var buttonList = new List<Button>();
            foreach (var zone in _equipmentList.Where(x => x.isPump))
            {
                var button = CreateEquipmentButton(zone);
                button.Scale = 1.25;
                var pumpButton = ScrollViewManualPump.Children.First(x => x.AutomationId == button.AutomationId);
                button.BackgroundColor = pumpButton.BackgroundColor;
                button.IsEnabled = pumpButton.IsEnabled;
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
    }
}