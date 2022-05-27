using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class AuthenticationScreen : ContentPage
{
        //GoogleSignInOptions gso;
        //GoogleApiClient googleApiClient;
        //FirebaseAuth firebaseAuth;

        public AuthenticationScreen()
    {
        InitializeComponent();

    }

        private void signinButton_Clicked(object sender, EventArgs e)
        {

        }
    }
}