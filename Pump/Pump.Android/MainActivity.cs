
using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Rg.Plugins.Popup.Services;
using Android.Util;
using Android.Gms.Common;
using Pump.Droid.Firebase;
using Xamarin.Forms;

namespace Pump.Droid
{
    [Activity(Label = "Pump", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public const string TAG = "MainActivity";
        internal static readonly string CHANNEL_ID = "my_notification_channel";

        protected override void OnCreate(Bundle savedInstanceState)
        {

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            if (Intent.Extras != null)
            {
                foreach (var key in Intent.Extras.KeySet())
                {
                    if (key != null)
                    {
                        var value = Intent.Extras.GetString(key);
                        Log.Debug(TAG, "Key: {0} Value: {1}", key, value);
                    }
                }
            }

            //WireUpLongRunningTask();
            IsPlayServicesAvailable();
            CreateNotificationChannel();
            
            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
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

        
        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var channelName = CHANNEL_ID;
            var channelDescription = string.Empty;
            var channel = new NotificationChannel(CHANNEL_ID, channelName, NotificationImportance.Default)
            {
                Description = channelDescription
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }


        public void WireUpLongRunningTask()
        {

            try
            {
                var intent = new Intent(this, typeof(MyFirebaseMessagingService));
                StartForegroundService(intent);
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat)
                {

                    PendingIntent pintent = PendingIntent.GetService(this, 0,
                        new Intent(this, typeof(MyFirebaseMessagingService)), 0);
                    AlarmManager alarm = (AlarmManager) this.GetSystemService(Context.AlarmService);
                    alarm.Cancel(pintent);
                }
            }
            catch
            {

            }
                
        }

    }
}