using System;
using System.Globalization;
using Pump.Class;
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
        public PopupMoreConnection(LoRaNodeInfo loRaNode)
        {
            InitializeComponent();
            Populate(loRaNode);
        }
        
        private void Populate(LoRaNodeInfo loRaNode)
        {
            LabelId.Text = "id: " + loRaNode.id;
            LabelLongName.Text = "longName: " + loRaNode.longName;
            LabelShortName.Text = "shortName: " + loRaNode.shortName;
            LabelMacAddr.Text = "macAddr: " + loRaNode.macaddr;
            LabelHwModel.Text = "hwModel: " + loRaNode.hwModel;
        }
        
        private void ButtonCancel_OnClicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }
    }
}