using System.Reflection;
using EmbeddedImages;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewEquipment
    {
        public ViewEquipment(Equipment equipment)
        {
            InitializeComponent();
            AutomationId = equipment.Id;
            Populate(equipment);
        }

        public void Populate(Equipment equipment)
        {
            LabelEquipmentName.Text = equipment.NAME;
            LabelPin.Text = "Pin: " + equipment.GPIO;
            if (equipment.DirectOnlineGPIO != null)
                LabelPin.Text += "-" + equipment.DirectOnlineGPIO;
            if (equipment.isPump)
                EquipmentImage.Source = ImageSource.FromResource(
                    "Pump.Icons.activePump.png",
                    typeof(ImageResourceExtension).GetTypeInfo().Assembly);
            StackLayoutStatus.AddUpdateRemoveStatus(equipment.ControllerStatus);
        }

        public TapGestureRecognizer GetTapGestureRecognizer()
        {
            return GridViewEquipmentTapGesture;
        }
        
        public void AddStatusActivityIndicator()
        {
            StackLayoutStatus.AddStatusActivityIndicator();
        }
    }
}