using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Database.Query;
using Firebase.Database.Streaming;
using Pump.Class;
using Pump.Database;
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
        private readonly DatabaseController _databaseController= new DatabaseController();
        private List<ManualSchedule> _manualScheduleList;
        private List<Equipment> _equipmentList;

        private List<ManualSchedule> _oldManualScheduleList;
        private List<Equipment> _oldEquipmentList;

        private FloatingScreenScroll _floatingScreenScroll;
        private bool? _firebaseHasReplied = false;

        public ManualScheduleHomeScreen()
        {
            InitializeComponent();
            new Thread(SubscribeToFirebase).Start();
        }
        private void SubscribeToFirebase()
        {
            var auth = new Authentication();
            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/ManualSchedule")
                .AsObservable<ManualSchedule>()

                .Subscribe(x =>
                {
                    try
                    {
                        if(_manualScheduleList == null)
                            _manualScheduleList = new List<ManualSchedule>();
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            if (x.Object != null)
                            {
                                var manualSchedule = x.Object;
                                _manualScheduleList.Clear();
                                _manualScheduleList.Add(manualSchedule);
                            }
                            else
                            {
                                _manualScheduleList.Clear();

                            }

                        });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });

            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Equipment")
                .AsObservable<Equipment>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (_equipmentList == null)
                            _equipmentList = new List<Equipment>();
                        var equipment = x.Object;

                        _equipmentList.RemoveAll(y => y.ID == x.Key);
                        if (x.EventType != FirebaseEventType.Delete)
                        {
                            equipment.ID = x.Key;
                            _equipmentList.Add(equipment);
                        }
                    }
                    catch (Exception e)
                    {
                        ScrollViewManualPump.Children.Clear();
                        ScrollViewManualPump.Children.Add(new ViewNoConnection());
                        Console.WriteLine(e);
                    }

                });

            PopulateManualElements();
        }

        private void PopulateManualElements()
        {

            while (_databaseController.IsRealtimeFirebaseSelected())
            {
                try
                {
                    //Populating 
                    if (_equipmentList != null && (_oldEquipmentList == null ||
                                                   (!_equipmentList.All(_oldEquipmentList.Contains) ||
                                                    _equipmentList.Count < _oldEquipmentList.Count)))
                    {
                        if (_oldEquipmentList == null)
                        {
                            _oldEquipmentList = new List<Equipment>();
                            ScrollViewManualPump.Children.Clear();
                            ScrollViewManualZone.Children.Clear();
                        }

                        _oldEquipmentList.Clear();
                        foreach (var equipment in _equipmentList)
                        {
                            _oldEquipmentList.Add(equipment);
                        }

                        _equipmentList = _equipmentList.OrderBy(equip => Convert.ToInt16(equip.GPIO)).ToList();



                        Device.BeginInvokeOnMainThread(() =>
                        {
                            

                            foreach (var pump in _equipmentList.Where(x => x.isPump))
                            {
                                var existingButton =
                                    ScrollViewManualPump.Children.FirstOrDefault(view => view.AutomationId == pump.ID);
                                if (existingButton == null)
                                    ScrollViewManualPump.Children.Add(CreateEquipmentButton(pump));
                                else
                                    ((Button) existingButton).Text = pump.NAME;
                            }
                            if (_equipmentList.Count(x => x.isPump) == 0)
                            {
                                ScrollViewManualPump.Children.Add(new ViewEmptySchedule("No Equipment Found Here"));
                            }

                            foreach (var zone in _equipmentList.Where(x => !x.isPump))
                            {
                                var existingButton =
                                    ScrollViewManualZone.Children.FirstOrDefault(view => view.AutomationId == zone.ID);
                                if (existingButton == null)
                                    ScrollViewManualZone.Children.Add(CreateEquipmentButton(zone));
                                else
                                    ((Button)existingButton).Text = zone.NAME;
                            }
                            //don't change :)
                            if (_equipmentList.Count(x => x.isPump == false) == 0)
                            {
                                ScrollViewManualZone.Children.Add(new ViewEmptySchedule("No Equipment Found Here"));
                            }
                        });
                    }
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ScrollViewManualPump.Children.Clear();
                        ScrollViewManualZone.Children.Clear();
                        ScrollViewManualPump.Children.Add(new ViewNoConnection());
                        ScrollViewManualZone.Children.Add(new ViewNoConnection());
                    });
                }

                try
                {
                    //Making sure we got both Equipments and Manual :/
                    if (_equipmentList != null && _manualScheduleList != null && (_oldManualScheduleList == null ||
                        (!_manualScheduleList.All(_oldManualScheduleList.Contains) ||
                         _manualScheduleList.Count < _oldManualScheduleList.Count)))
                    {
                        if (_oldManualScheduleList == null)
                            _oldManualScheduleList = new List<ManualSchedule>();
                        _oldManualScheduleList.Clear();
                        foreach (var manualSchedules in _manualScheduleList)
                        {
                            _oldManualScheduleList.Add(manualSchedules);
                        }

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            //Sorts Out all the button color stuff
                            SetActiveEquipmentButton();
                            if (_manualScheduleList.Count == 0)
                            {
                                //Clear Manual
                                MaskedEntryTime.Text = "";
                                MaskedEntryTime.IsEnabled = true;
                                ButtonStartManual.IsEnabled = true;
                                SwitchRunWithSchedule.IsEnabled = true;
                                SwitchRunWithSchedule.IsToggled = true;
                            }
                            else
                            {
                                //Populate :/
                                SwitchRunWithSchedule.IsToggled = _manualScheduleList[0].RunWithSchedule;
                                SwitchRunWithSchedule.IsEnabled = false;
                                MaskedEntryTime.IsEnabled = false;
                                MaskedEntryTime.Text = ScheduleTime.ConvertTimeSpanToString(ScheduleTime.FromUnixTimeStampUtc(_manualScheduleList[0].EndTime) - DateTime.Now);
                                ButtonStartManual.IsEnabled = false;
                            }
                        });
                    }
                }
                catch
                {
                    // ignored
                }

                Thread.Sleep(2000);
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
                    button.BackgroundColor = _manualScheduleList.FirstOrDefault(x =>
                        x.ManualDetails.Select(y => y.id_Equipment).Contains(button.AutomationId)) != null ? Color.BlueViolet : Color.AliceBlue;
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
                    DURATION = MaskedEntryTime.Text,
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
            if (_oldEquipmentList == null)
                return;
            var buttonList = new List<Button>();
            foreach (var zone in _equipmentList.Where(x => !x.isPump))
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
            if (_oldEquipmentList == null)
                return;
            var buttonList = new List<Button>();
            foreach (var zone in _equipmentList.Where(x => x.isPump))
            {
                var button = CreateEquipmentButton(zone);
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
    }
}