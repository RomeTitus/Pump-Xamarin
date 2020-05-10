namespace Pump.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            
            LoadApplication(new Pump.App());
        }
    }
}
