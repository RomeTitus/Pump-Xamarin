using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using Firebase;
using Firebase.Messaging;
using Pump.Database;
using Pump.Droid.Database.Table;
using Xamarin.Forms;

[assembly: Permission(Name = "com.Varkensvlei.Pump.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "android.permission.WAKE_LOCK")]
[assembly: UsesPermission(Name = "com.Varkensvlei.Pump.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "com.google.android.c2dm.permission.RECEIVE")]
[assembly: UsesPermission(Name = "android.permission.GET_ACCOUNTS")]
[assembly: UsesPermission(Name = "android.permission.INTERNET")]

namespace Pump.Droid.Firebase
{

    [Service(Exported = true, Name = "com.Varkensvlei.Pump.Droid.Firebase.MyFirebaseMessagingService"), IntentFilter(new[] { "com.google.android.c2dm.intent.RECEIVE" })]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    [IntentFilter(new string[] {
        Intent.ActionBootCompleted, Intent.ActionLockedBootCompleted, "android.intent.action.QUICKBOOT_POWERON"
    })]

    public class MyFirebaseMessagingService : FirebaseMessagingService 
    {
        public MyFirebaseMessagingService()
        {

        }
        const string TAG = "MyFirebaseMsgService";

        /*
        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);
            PowerManager.WakeLock sWakeLock;
            var pm = PowerManager.FromContext(this);
            sWakeLock = pm.NewWakeLock(WakeLockFlags.Partial, "GCM Broadcast Reciever Tag");
            sWakeLock.Acquire();

            SendNotification("Hi", "Whats up", new DatabaseController().getControllerNameByMac("Vlas"));

            //handle the notification here

            sWakeLock.Release();

            
        }

        
        public override void HandleIntent(Intent intent)
        {
            base.HandleIntent(intent);
            if (intent.GetStringExtra("Bluetooth") != null)
            {
                DatabaseController controller = new DatabaseController();

                PumpConnection pumpConnection = controller.getControllerNameByMac(intent.GetStringExtra("Bluetooth"));
                if (pumpConnection != null)
                {
                    
                    if (intent.GetStringExtra("Ngrok") != null)
                    {
                        var ngrokConnectionInfo = intent.GetStringExtra("Ngrok");
                        pumpConnection.ExternalPort = Convert.ToInt32(ngrokConnectionInfo.Split(":").Last().Replace("\n", ""));
                        pumpConnection.ExternalPath = ngrokConnectionInfo.Split("//").Last().Split(":").First();
                        controller.UpdatePumpConnection(pumpConnection);
                        SendNotification(ngrokConnectionInfo, "Connection Updated", pumpConnection);
                    }
                    else
                    {
                        SendNotification(intent.GetStringExtra("Message"), intent.GetStringExtra("Header"), pumpConnection);
                    }
                }
            }


        }

        */
        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);
            if (message.GetNotification() != null)
            {
                //These is how most messages will be received
                Log.Debug(TAG, "Notification Message Body: " + message.GetNotification().Body);
            }
            else
            {

                //Check if message contains a data payload.
                if (message.Data.Count > 0)
                {
                    var remoteData = message.Data;

                    if (remoteData["Bluetooth"] != null)
                    {
                        DatabaseController controller = new DatabaseController();



                        PumpConnection pumpConnection = controller.getControllerNameByMac(remoteData["Bluetooth"]);
                        if (pumpConnection != null)
                        {
                            bool ngrokExist;
                            try
                            {
                                remoteData["Ngrok"].ToString();
                                ngrokExist = true;
                            }
                            catch
                            {
                                ngrokExist = false;
                            }
                            if (ngrokExist)
                            {
                                var ngrokConnectionInfo = remoteData["Ngrok"];
                                pumpConnection.ExternalPort = Convert.ToInt32(ngrokConnectionInfo.Split(":").Last().Replace("\n", ""));
                                pumpConnection.ExternalPath = ngrokConnectionInfo.Split("//").Last().Split(":").First();
                                controller.UpdatePumpConnection(pumpConnection);
                                SendNotification(ngrokConnectionInfo, "Connection Updated", pumpConnection);
                            }
                            else
                            {
                                SendNotification(remoteData["Message"], remoteData["Header"], pumpConnection);
                            }
                        }
                    }
                }
            }
        }

      
        
        void SendNotification(string messageBody, string messageTopic, PumpConnection pumpConnection)
        {

            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);

            //var notificationBuilder = new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID);
            var notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
            var NOTIFICATION_Channel_ID = "EDMTDev";
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var notificationChannel =
                    new NotificationChannel(NOTIFICATION_Channel_ID,
                        "EDMT Notification", NotificationImportance.High);

                notificationChannel.EnableLights(true);
                notificationChannel.LightColor = Android.Resource.Color.Black;
                notificationChannel.SetVibrationPattern(new long[] { 0, 1000, 5000 });
                notificationChannel.EnableVibration(true);

                notificationManager.CreateNotificationChannel(notificationChannel);
            }

            var notificationBuilder =
                new NotificationCompat.Builder(this, NOTIFICATION_Channel_ID);

            notificationBuilder
                .SetAutoCancel(true)
                //.SetWhen(SystemClock.CurrentThreadTimeMillis())
                .SetSmallIcon(Resource.Drawable.ic_launcher)
                .SetContentTitle(pumpConnection.Name + ": " + messageTopic)
                .SetContentText(messageBody)
                .SetShowWhen(false)
                .SetContentIntent(pendingIntent);


            //var notificationManager = NotificationManager.FromContext(this);

            notificationManager.Notify(pumpConnection.ID, notificationBuilder.Build());
        }
    }
}