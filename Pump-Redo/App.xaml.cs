using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Pump.Class;
using Pump.Layout;
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

            if (_client.User == null)
                MainPage = new AuthenticationScreen(_client);
            else
                MainPage = new MainPage();
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
            _client.AuthStateChanged += ClientOnAuthStateChanged;
        }

        private void ClientOnAuthStateChanged(object sender, UserEventArgs e)
        {
        }
    }
}