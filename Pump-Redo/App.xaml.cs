using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Pump.Layout;

namespace Pump
{
    public partial class App : Application
    {
        FirebaseAuthClient _client;
        public App()
        {
            AuthenticateUser();
            InitializeComponent();

            MainPage = new AuthenticationScreen(_client);
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
                    }
            };
            _client = new FirebaseAuthClient(config);
        }

    }
}
