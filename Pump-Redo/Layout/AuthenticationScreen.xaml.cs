using System;
using System.Net.Mail;
using Firebase.Auth;
using Pump.Class;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AuthenticationScreen : ContentPage
    {
        private readonly FirebaseAuthClient _client;

        public AuthenticationScreen(FirebaseAuthClient client)
        {
            _client = client;
            InitializeComponent();
            TxtSignUpFullName.Placeholder = DeviceInfo.Name;
        }


        private string ValidationAuthentication(bool validateEmailOnly = false)
        {
            var notification = "";
            //Shows the SignUp Layout
            if (SignUpStackLayout.IsVisible)
            {
                if (string.IsNullOrWhiteSpace(TxtSignUpFullName.Text))
                {
                    notification += "\n\u2022 Full name required";
                    TxtSignUpFullName.PlaceholderColor = Color.Red;
                }

                if (string.IsNullOrWhiteSpace(TxtSignUpEmail.Text))
                {
                    notification += "\n\u2022 Email required";
                    TxtSignUpEmail.PlaceholderColor = Color.Red;
                }
                else
                {
                    try
                    {
                        var unused = new MailAddress(TxtSignUpEmail.Text);
                    }
                    catch (FormatException)
                    {
                        notification += "\n\u2022 " + TxtSignUpEmail.Text + " is not a valid email";
                    }
                }

                //Manage password Complex
                var passwordStrengthScore = PasswordAdvisor.CheckStrength(TxtSignUpPassword.Text);
                switch (passwordStrengthScore)
                {
                    case PasswordScore.Blank:
                        notification += "\n\u2022 Password required";
                        TxtSignUpPassword.PlaceholderColor = Color.Red;
                        break;
                    case PasswordScore.VeryWeak:
                    case PasswordScore.Weak:
                        notification += "\n\u2022 Password is: " + passwordStrengthScore + ". try again";
                        break;
                }

                if (string.IsNullOrWhiteSpace(TxtSignUpRepeatPassword.Text))
                {
                    notification += "\n\u2022 Repeat password required";
                    TxtSignUpRepeatPassword.PlaceholderColor = Color.Red;
                }
                else if (TxtSignUpPassword.Text != TxtSignUpRepeatPassword.Text)
                {
                    notification += "\n\u2022 Repeat password does not match password";
                }
            }

            if (SignInStackLayout.IsVisible)
            {
                if (string.IsNullOrWhiteSpace(TxtSignInEmail.Text))
                {
                    notification += "\n\u2022 Email required";
                    TxtSignInEmail.PlaceholderColor = Color.Red;
                }
                else
                {
                    try
                    {
                        var unused = new MailAddress(TxtSignInEmail.Text);
                        if(TxtSignInEmail.Text.Contains(".") == false)
                            notification += "\n\u2022 " + TxtSignInEmail.Text + " is not a valid email";
                    }
                    catch (FormatException)
                    {
                        notification += "\n\u2022 " + TxtSignInEmail.Text + " is not a valid email";
                    }
                }

                if(validateEmailOnly == false)
                    if (string.IsNullOrWhiteSpace(TxtSignInPassword.Text))
                    {
                        notification += "\n\u2022 Password required";
                        TxtSignInPassword.PlaceholderColor = Color.Red;
                    }
            }

            return notification;
        }

        private void SigninButton_Clicked(object sender, EventArgs e)
        {
            SignUpStackLayout.IsVisible = false;
            SignInStackLayout.IsVisible = true;
            LabelLogin.Text = "Login";
            LabelSignIn.IsVisible = true;
        }


        private void SignupButton_Clicked(object sender, EventArgs e)
        {
            SignUpStackLayout.IsVisible = true;
            SignInStackLayout.IsVisible = false;
            LabelLogin.Text = "Create Account";
            LabelSignIn.IsVisible = false;
        }

        private async void ForgotButton_Clicked(object sender, EventArgs e)
        {
            var notification = ValidationAuthentication(true);
            if (!string.IsNullOrWhiteSpace(notification))
            {
                await DisplayAlert("Incomplete", notification, "Understood");
                return;
            }

            try
            {
                await DisplayAlert("Confirmation", "Are you sure you want to reset your password?", "Accept","Cancel");

                var loadingScreen = new VerifyConnections { CloseWhenBackgroundIsClicked = false };
                await PopupNavigation.Instance.PushAsync(loadingScreen);
                await _client.ResetEmailPasswordAsync(TxtSignInEmail.Text);
                await PopupNavigation.Instance.PopAllAsync();
                await DisplayAlert("Complete", "Password reset sent to " + TxtSignInEmail.Text, "Understood");

            }
            catch (FirebaseAuthHttpException ex)
            {
                await PopupNavigation.Instance.PopAllAsync();
                await DisplayAlert("Invalid", "Reason: " + ex.Reason, "Understood");
            }
        }

        private async void ButtonLogin_OnClicked(object sender, EventArgs e)
        {
            var notification = ValidationAuthentication();
            if (!string.IsNullOrWhiteSpace(notification))
            {
                await DisplayAlert("Incomplete", notification, "Understood");
                return;
            }

            try
            {
                //User is already signed in
                if(_client.User?.Info.Email == TxtSignInEmail.Text)
                    return;
                var loadingScreen = new VerifyConnections { CloseWhenBackgroundIsClicked = false };
                await PopupNavigation.Instance.PushAsync(loadingScreen);
                var userCredential = await _client.SignInWithEmailAndPasswordAsync(TxtSignInEmail.Text,
                    TxtSignInPassword.Text);
                await PopupNavigation.Instance.PopAllAsync();
            }
            catch (FirebaseAuthHttpException ex)
            {
                await PopupNavigation.Instance.PopAllAsync();
                await DisplayAlert("Invalid", "Reason: " + ex.Reason, "Understood");
            }
        }

        private async void ButtonSignUp_OnClicked(object sender, EventArgs e)
        {
            var notification = ValidationAuthentication();
            if (!string.IsNullOrWhiteSpace(notification))
            {
                await DisplayAlert("Incomplete", notification, "Understood");
                return;
            }

            try
            {
                var loadingScreen = new VerifyConnections { CloseWhenBackgroundIsClicked = false };
                await PopupNavigation.Instance.PushAsync(loadingScreen);
                var userCredential = await _client.CreateUserWithEmailAndPasswordAsync(TxtSignUpEmail.Text,
                    TxtSignUpPassword.Text, TxtSignUpFullName.Text);
                await PopupNavigation.Instance.PopAllAsync();
            }
            catch (FirebaseAuthHttpException ex)
            {
                await PopupNavigation.Instance.PopAllAsync();
                await DisplayAlert("Invalid", "Reason: " + ex.Reason, "Understood");
            }
        }
    }
}