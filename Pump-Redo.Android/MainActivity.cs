using System;
using Android;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Rg.Plugins.Popup;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Platform = Xamarin.Essentials.Platform;

namespace Pump_Redo.Droid
{
    [Activity(Label = "Pump", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public const string TAG = "MainActivity";

        private readonly string[] _permissions =
        {
            Manifest.Permission.Bluetooth,
            Manifest.Permission.BluetoothAdmin,
            Manifest.Permission.BluetoothScan,
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            Popup.Init(this);
            Platform.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);

            CheckPermissions();
            //IsPlayServicesAvailable();

            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
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

        public override void OnBackPressed()
        {
            if (Popup.SendBackPressed(base.OnBackPressed))
            {
                PopupNavigation.Instance.PopAsync();
            }
        }
    }
}