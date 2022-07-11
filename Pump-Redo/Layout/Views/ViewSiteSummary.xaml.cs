using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Pump.CustomRender;
using Pump.Database.Table;
using Pump.IrrigationController;
using Pump.SocketController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewSiteSummary
    {
        private Timer _timer;
        private readonly KeyValuePair<string, List<string>> _keyControllerPair;
        private readonly KeyValuePair<IrrigationConfiguration, ObservableIrrigation> _observableKeyValuePair;
        private readonly SocketPicker _socketPicker;
        public ViewSiteSummary(KeyValuePair<string,List<string>> keyControllerPair, KeyValuePair<IrrigationConfiguration, ObservableIrrigation> observableKeyValuePair, SocketPicker socketPicker)
        {
            _keyControllerPair = keyControllerPair;
            _observableKeyValuePair = observableKeyValuePair;
            _socketPicker = socketPicker;
            InitializeComponent();
            StartEvent();
            Populate();
        }

        private async void Populate()
        {
            await SetFocus();
            SiteNameEntry.Text = _keyControllerPair.Key;
            foreach (var subControllerId in _keyControllerPair.Value)
            {
                ViewSubControllerSummary subControllerSummary; 
                if (_observableKeyValuePair.Value.LoadedData)
                {
                    var subController = _observableKeyValuePair.Value.SubControllerList.FirstOrDefault(x => x.Id == subControllerId);
                    subControllerSummary = new ViewSubControllerSummary(subController);
                    
                }else
                    subControllerSummary = new ViewSubControllerSummary(subControllerId);
                StackLayoutSubController.Children.Add(subControllerSummary);
                if(subControllerSummary.AutomationId != null)
                    subControllerSummary.GetTapGestureRecognizer().Tapped += OnTapped_SubController;
            }
        }

        private async void OnTapped_SubController(object sender, EventArgs e)
        {
            var stackLayout = (StackLayout)sender;
            var subController = _observableKeyValuePair.Value.SubControllerList.First(x => x.Id == ((ViewSubControllerSummary) stackLayout.Parent).AutomationId);
            await Navigation.PushModalAsync(new SubControllerUpdate(_socketPicker, subController, _observableKeyValuePair.Key));

        }

        private void UpdateExisting()
        {
            foreach (var view in StackLayoutSubController.Children)
            {
                var subControllerSummary = (ViewSubControllerSummary)view;
                if (subControllerSummary.LoadedData) continue;
                var sub = _observableKeyValuePair.Value.SubControllerList.FirstOrDefault(x =>
                    x.Id == subControllerSummary.AutomationId);
                subControllerSummary.Populate(sub);
            }
        }

        private async Task SetFocus()
        {
            await Task.Delay(200);
            SiteNameEntry.TextBox_Focused(this, new FocusEventArgs(this, true));
            await Task.Delay(300);
        }

        private void TapGestureExpandSite_OnTapped(object sender, EventArgs e)
        {
            ExpandImage.Rotation += 180;
            if (ExpandImage.Rotation > 180)
            {
                ExpandImage.Rotation = 0;
                StackLayoutSubController.IsVisible = false;
            }
            else
                StackLayoutSubController.IsVisible = true;
        }
        
        private void StartEvent()
        {
            _timer = new Timer(800);
            _timer.Elapsed += timer_Elapsed;
            _timer.Enabled = true;
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Enabled = false;
            UpdateExisting();
            _timer.Enabled = _observableKeyValuePair.Value.LoadedData == false;
        }

        public EntryOutlined GetSiteNameEntry()
        {
            return SiteNameEntry;
        }

        private void SiteNameEntry_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (EntryOutlined)sender;
            if (e.NewTextValue == string.Empty)
                SetPlaceholderColor(entry, Color.Red, Color.Red);
            else
                SetPlaceholderColor(entry, Color.Navy, Color.Black);
            
        }
        
        private static void SetPlaceholderColor(EntryOutlined entry, Color placeholderColor, Color borderColor)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                entry.PlaceholderColor =placeholderColor;
                entry.BorderColor = borderColor;
            });
        }

        public  KeyValuePair<string, List<string>> GetKeyValuePair()
        {
            return _keyControllerPair;
        }
    }
}