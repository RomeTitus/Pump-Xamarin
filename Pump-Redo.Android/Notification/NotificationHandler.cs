using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Newtonsoft.Json.Linq;
using Plugin.FirebasePushNotification;
using Pump.IrrigationController;
using System;
using System.Collections.Generic;
using Android.Provider;
using Pump.SocketController;
using Pump.Database;
using Pump.Database.Table;
using Firebase.Auth;
using Pump.SocketController.Firebase;
using Firebase.Auth.Providers;
using Pump.Class;

namespace Pump.Droid.Notification
{
    [Application]
    public class NotificationHandler : Application
    {
        private User _user;
        private FirebaseAuthClient _firebaseAuthClient;

        public NotificationHandler(IntPtr handle, JniHandleOwnership transer) : base(handle, transer)
        {
        }
        public override void OnCreate()
        {
            base.OnCreate();

            FirebasePushNotificationManager.NotificationActivityType = typeof(MainActivity);
            FirebasePushNotificationManager.NotificationActivityFlags = Android.Content.ActivityFlags.ClearTop | Android.Content.ActivityFlags.SingleTop;
            //Set the default notification channel for your app when running Android Oreo
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                //Change for your default notification channel id here
                FirebasePushNotificationManager.DefaultNotificationChannelId = "FirebasePushNotificationChannel";
                //Change for your default notification channel name here
                FirebasePushNotificationManager.DefaultNotificationChannelName = "General";
            }
#if DEBUG
            FirebasePushNotificationManager.Initialize(this, true, false);
#else
	            FirebasePushNotificationManager.Initialize(this, new AndroidNotificationManager(), false, false);
#endif

            //Handle notification when app is closed here
            CrossFirebasePushNotification.Current.OnNotificationReceived += (s, p) =>
            {
                try
                {
                    Log.Info("PumpNotification", "Received Notification");
                    if (!p.Data.ContainsKey("data")) return;
                    try
                    {
                        var notificationData = p.Data["data"];
                        JObject jObject = JObject.Parse(notificationData.ToString());
                        var title = jObject.Value<string>("title") ?? "Not Provided";
                        var body = jObject.Value<string>("body") ?? "Not Provided";
                        var BtMac = jObject.Value<string>("BTmac") ?? "Not Provided";
                        new AndroidNotificationManager().SendNotification(title, body, BtMac);
                    }
                    catch
                    {
                        // ignored
                    }
                }
                catch (Exception e)
                {
                    Log.Info("PumpNotification", "Notification Failed! " + e);
                }
            };
            
            //Push Notification when signing in?
            //var test = CrossFirebasePushNotification.Current.Token;

            CrossFirebasePushNotification.Current.OnTokenRefresh += async (s, p) =>
            {
                if (string.IsNullOrEmpty(p.Token))
                    return;
                AuthenticateUser();
                var notification = new NotificationToken
                {
                    Id = Settings.Secure.GetString(Context.ContentResolver,
                        Settings.Secure.AndroidId),
                    Token = p.Token
                };

                Log.Info("PumpNotification", $"NewToken: {p.Token}");

                var observableDict = new Dictionary<IrrigationConfiguration, ObservableIrrigation>();
                DatabaseController databaseController = new DatabaseController();
                var database = databaseController;
                var irrigationConfigurationList = database.GetIrrigationConfigurationList();
                var userAuthentication = database.GetUserAuthentication();
                irrigationConfigurationList.ForEach(x => observableDict.Add(x, new ObservableIrrigation()));
                
                var socketPicker = new SocketPicker(new FirebaseManager(), observableDict);
                if(_user != null)
                {
                    socketPicker.SetFirebaseUser(_user);
                    var result = await socketPicker.SendCommand(notification, irrigationConfigurationList);
                }
            };
        }


        private void AuthenticateUser()
        {
            if (_firebaseAuthClient != null)
                return;

            _firebaseAuthClient = new FirebaseAuthClient(new FirebaseAuthConfig
            {
                ApiKey = "AIzaSyDxfc71frXHM-gtVgynCft8rokK_Bl6r0c",
                AuthDomain = "pump-25eee.firebaseapp.com",
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                },
                UserRepository = new StorageRepository()
            });

            _firebaseAuthClient.AuthStateChanged += ClientOnAuthStateChanged;
        }


        private void ClientOnAuthStateChanged(object sender, UserEventArgs e)
        {
            _user = e.User;
        }


    }
}