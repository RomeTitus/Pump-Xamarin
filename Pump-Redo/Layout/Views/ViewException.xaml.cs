using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewException : ContentView
    {
        public ViewException(Exception e)
        {
            InitializeComponent();
            ID = "-849";
            AutomationId = ID;
            LabelException.Text = e.Message;
        }

        public string ID { get; }
    }
}