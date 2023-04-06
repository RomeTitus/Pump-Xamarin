using System;
using System.Globalization;
using Pump.SocketController;
using Pump.SocketController.BT;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PopupMoreConnection : PopupPage
    {
        private readonly BluetoothManager _blueToothManager;
        private int _address;
        public PopupMoreConnection(string loRaConfig, BluetoothManager blueToothManager)
        {
            _blueToothManager = blueToothManager;
            InitializeComponent();
            Populate(loRaConfig);
        }
        
        private void Populate(string loRaConfig)
        {
            var loRaTuple = DeserializeLoRaConfig(loRaConfig);
                
            if (loRaTuple == null)
            {
                ModemConfigPicker.SelectedIndex = 0;
                FreqSlider.Value = 444.33;
                TransmissionSlider.Value = 16;
            }
            else
            {
                _address = loRaTuple.Value.address;
                ModemConfigPicker.SelectedIndex = loRaTuple.Value.modem;
                FreqSlider.Value = loRaTuple.Value.freq;
                TransmissionSlider.Value = loRaTuple.Value.power;
            }
        }

        private void freq_OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            FrequencyLabel.Text = $"Freq: {e.NewValue:0.0}";
        }

        private void TransmissionPower_OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            TransmissionLabel.Text = $"Power: {e.NewValue:00}  ";
        }
        private (int address, double freq, int power, int modem)? DeserializeLoRaConfig(string config)
        {
            if (!config.Contains("Config"))
                return null;
            var configList = config.Split(',');
            var address = short.Parse(configList[0].Split('=')[1]);
            var freq = Convert.ToDouble(configList[1].Split('=')[1], CultureInfo.InvariantCulture);
            var power = short.Parse(configList[2].Split('=')[1]);
            var modem = short.Parse(configList[3].Split('=')[1].Remove(1));
            return (address, freq, power, modem);
        }
        
        private void ButtonCancel_OnClicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }

        private async void ButtonLoRaConfigSave_OnClicked(object sender, EventArgs e)
        {
            if(!await DisplayAlert("Warning", "The Data Rate and Frequency on the other LoRa's need to match to communicate",
                "accept", "cancel"))
                return;
            
            try
            {
                var loadingScreen = new PopupLoading { CloseWhenBackgroundIsClicked = false };
                await PopupNavigation.Instance.PushAsync(loadingScreen);
                (int address, double freq, int power, int modem) loRaTuple = (_address, FreqSlider.Value, Convert.ToInt16(TransmissionSlider.Value), ModemConfigPicker.SelectedIndex);
                var loRaConfig = await _blueToothManager.SendAndReceiveToBleAsync(SocketCommands.SetLoRaConfig(loRaTuple));
                await PopupNavigation.Instance.PopAsync();
                if(!loRaConfig.Contains("Config("))
                    await DisplayAlert("Warning", loRaConfig, "Acknowledge");
                else
                    await PopupNavigation.Instance.PopAsync();
            }
            catch (Exception exception)
            {
                await PopupNavigation.Instance.PopAllAsync();
                await DisplayAlert("Exception", exception.ToString(), "Understood");
            }
            
        }
    }
}