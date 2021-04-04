using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.OS;
using Android.Runtime;
using Rg.Plugins.Popup.Services;
using Android.Util;
using Plugin.FirebasePushNotification;
using Pump.Droid.Notification;
using Pump.Notification;
using Xamarin.Forms;
using ConnectionResult = Android.Gms.Common.ConnectionResult;

namespace Pump.Droid
{
    [Activity(Label = "Pump", Icon = "@drawable/Logo", Theme = "@style/MainTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private readonly string[] _permissions =
        {
            Manifest.Permission.Bluetooth,
            Manifest.Permission.BluetoothAdmin,
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation
        };

        public const string TAG = "MainActivity";
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            if (Intent?.Extras != null)
            {
                foreach (var key in Intent?.Extras?.KeySet())
                {
                    var value = Intent.Extras.GetString(key);
                    Log.Debug(TAG, "Key: {0} Value: {1}", key, value ?? string.Empty);
                }
            }
            Rg.Plugins.Popup.Popup.Init(this);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);

            CheckPermissions();
            IsPlayServicesAvailable();
            LoadApplication(new App());
            FirebasePushNotificationManager.ProcessIntent(this, Intent);

        }


        private void CheckPermissions()
        {
            bool minimumPermissionsGranted = true;

            foreach (string permission in _permissions)
            {
                if (CheckSelfPermission(permission) != Permission.Granted)
                {
                    minimumPermissionsGranted = false;
                }
            }

            // If any of the minimum permissions aren't granted, we request them from the user
            if (!minimumPermissionsGranted)
            {
                RequestPermissions(_permissions, 0);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private bool IsPlayServicesAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                    Log.Debug(TAG, GoogleApiAvailability.Instance.GetErrorString(resultCode));
                else
                {
                    Log.Debug(TAG, "This device is not supported");
                    Finish();
                }
                return false;
            }

            Log.Debug(TAG, "Google Play Services is available.");
            return true;
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            FirebasePushNotificationManager.ProcessIntent(this, intent);
            CreateNotificationFromIntent(intent);
        }

        void CreateNotificationFromIntent(Intent intent)
        {
            if (intent?.Extras != null)
            {
                string title = intent.GetStringExtra(AndroidNotificationManager.TitleKey);
                string message = intent.GetStringExtra(AndroidNotificationManager.MessageKey);
                string controllerName = intent.GetStringExtra(AndroidNotificationManager.ControllerNameKey);
                DependencyService.Get<INotificationManager>().ReceiveNotification(title, message, controllerName);
            }
        }

        public override void OnBackPressed()
        {
            if (Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed))
            {
                PopupNavigation.Instance.PopAsync(); 
            }
            else
            {
                // Do something if there are not any pages in the `PopupStack`
            }
        }



    }
}