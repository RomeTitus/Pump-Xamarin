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
using Firebase.Messaging;
using NotificationSample.Droid;

namespace Pump.Droid.Firebase
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        public MyFirebaseMessagingService()
        {

        }
        public override void OnMessageReceived(RemoteMessage message)
        {
            Console.WriteLine("[" + this.GetType().FullName + "]\t\t\t\t Push Notification Received! \n\n" + message.GetNotification().Body);
            base.OnMessageReceived(message);
            
            new NotificationHelper().CreateNotification(message.GetNotification().Title, message.GetNotification().Body);
        }
    }
}