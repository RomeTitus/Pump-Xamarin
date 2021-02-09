using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewBasicAlert : ContentView
    {
        private string _title;
        private string _message;
        private string _subMessage;
        private string _accept;
        private string _cancel;
        public bool Editable;
        public ViewBasicAlert(string title, string message, string accept, string cancel, bool editable = false)
        {
            InitializeComponent();
            BasicAlertEnter.IsVisible = editable;
            Editable = editable;
            _title = title;
            _message = message;
            _accept = accept;
            _cancel = cancel;
            populate();
        }

        public ViewBasicAlert(string title, string message,string subMessage, string accept, string cancel, bool editable)
        {
            InitializeComponent();
            BasicAlertEnter.IsVisible = editable;
            SubBasicAlertEnter.IsVisible = editable;
            LabelSubMessage.IsVisible = editable;
            _subMessage = subMessage;
            Editable = editable;
            _title = title;
            _message = message;
            _accept = accept;
            _cancel = cancel;
            LabelSubMessage.Text = _subMessage;
            populate();
        }

        private void populate()
        {
            LabelTitle.Text = _title;
            LabelMessage.Text = _message;
            ButtonAccept.Text = _accept;
            ButtonCancel.Text = _cancel;
        }

        public string getEditableText()
        {
            return BasicAlertEnter.Text;
        }

        public string getSubEditableText()
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