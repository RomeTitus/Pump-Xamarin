using Firebase.Auth;
using Firebase.Auth.Providers;
using Pump.Class;
using Xamarin.Forms;

namespace Pump
{
    public partial class App : Application
    {
        private FirebaseAuthClient _client;

        public App()
        {
            AuthenticateUser();
            InitializeComponent();
            MainPage = new MainPage(_client);
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        private void AuthenticateUser()
        {
            var config = new FirebaseAuthConfig
            {
                ApiKey = "AIzaSyDxfc71frXHM-gtVgynCft8rokK_Bl6r0c",
                AuthDomain = "pump-25eee.firebaseapp.com",
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                },
                UserRepository = new StorageRepository()
            };
            _client = new FirebaseAuthClient(config);
        }
    }
}