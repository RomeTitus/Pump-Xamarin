using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using Firebase.Messaging;
using Android.Support.V4.App;
using WindowsAzure.Messaging;
using Android.Media.Midi;
using Pump.Azure;
using Pump.Database;
using Pump.Database.Table;
using Pump.Droid.Database.Table;

namespace Pump.Droid.Firebase
{
    [Service(Exported = true, Name = "com.Varkensvlei.Pump.Droid.Firebase.MyFirebaseMessagingService"), IntentFilter(new[] { "com.google.android.c2dm.intent.RECEIVE" }) ]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        const string TAG = "MyFirebaseMsgService";
        NotificationHub hub;

        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);


            Log.Debug(TAG, "From: " + message.From);
            if (message.GetNotification() != null)
            {
                //These is how most messages will be received
                Log.Debug(TAG, "Notification Message Body: " + message.GetNotification().Body);
            }
            else
            {

                //Check if message contains a data payload.
                if (message.Data.Count >0)
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

            var notificationBuilder = new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID);

            notificationBuilder.SetContentTitle(pumpConnection.Name + ": " + messageTopic)
                .SetSmallIcon(Resource.Drawable.ic_launcher)
                .SetContentText(messageBody)
                .SetAutoCancel(true)
                .SetShowWhen(false)
                .SetContentIntent(pendingIntent);

            var notificationManager = NotificationManager.FromContext(this);

            notificationManager.Notify(0, notificationBuilder.Build());
        }

        public override void OnNewToken(string token)
        {
            Log.Debug(TAG, "FCM token: " + token);
            SendRegistrationToServer(token);
        }

        void SendRegistrationToServer(string token)
        {
            // Register with Notification Hubs
            var notificationToken = new NotificationToken();
            notificationToken.token = token;
            new DatabaseController().setNotificationToken(notificationToken);
            hub = new NotificationHub(Constants.NotificationHubName,
                Constants.ListenConnectionString, this);

            var tags = new List<string>() { };
            var regID = hub.Register(token, tags.ToArray()).RegistrationId;

            Log.Debug(TAG, $"Successful registration of ID {regID}");
        }

        

    }
}