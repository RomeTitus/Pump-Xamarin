using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pump.Database;
using Pump.Database.Table;
using Pump.SocketController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomeScreen : TabbedPage
    {
        private readonly DatabaseController _databaseController = new DatabaseController();
        
        
        public HomeScreen()
        {
            InitializeComponent();
            if (Device.RuntimePlatform == Device.iOS)
            {
                ScheduleStatusTab.IconImageSource = new FileImageSource();
                ManualStatusTab.IconImageSource = new FileImageSource();
                SettingTab.IconImageSource = new FileImageSource();
            }



            if (_databaseController.GetPumpSelection() == null)
            {
                _databaseController.SetActivityStatus(new ActivityStatus(false));
                Navigation.PushModalAsync(new AddController());
               // Navigation.PopModalAsync();
            }
            else
            {
                Thread sendToken = new Thread(() => SentNotificationToken());
                sendToken.Start();
                
            }
                

        }

        public void SentNotificationToken()
        {
            try
            {
                new SocketMessage().Message(
                    new SocketCommands().setToken(
                        _databaseController.GetNotificationToken().token));
            }
            catch
            {

            }
        }
  
    }
}