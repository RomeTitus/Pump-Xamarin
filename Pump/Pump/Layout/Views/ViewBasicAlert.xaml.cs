using System;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewBasicAlert : ContentView
    {
        private readonly string _accept;
        private readonly string _cancel;
        private readonly string _message;
        private readonly string _title;
        public readonly bool Editable;

        public ViewBasicAlert(string title, string message, string accept, string cancel, bool editable = false)
        {
            InitializeComponent();
            BasicAlertEnter.IsVisible = editable;
            Editable = editable;
            _title = title;
            _message = message;
            _accept = accept;
            _cancel = cancel;
            Populate();
        }

        public ViewBasicAlert(string title, string message, string subMessage, string accept, string cancel,
            bool editable)
        {
            InitializeComponent();
            BasicAlertEnter.IsVisible = editable;
            SubBasicAlertEnter.IsVisible = editable;
            LabelSubMessage.IsVisible = editable;
            Editable = editable;
            _title = title;
            _message = message;
            _accept = accept;
            _cancel = cancel;
            LabelSubMessage.Text = subMessage;
            Populate();
        }

        private void Populate()
        {
            LabelTitle.Text = _title;
            LabelMessage.Text = _message;
            ButtonAccept.Text = _accept;
            ButtonCancel.Text = _cancel;
        }

        public string GetEditableText()
        {
            return BasicAlertEnter.Text;
        }

        public string GetSubEditableText()
        {
            return SubBasicAlertEnter.Text;
        }

        public Button GetAcceptButton()
        {
            return ButtonAccept;
        }

        private void ButtonCancel_OnClicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAllAsync();
        }
    }
}