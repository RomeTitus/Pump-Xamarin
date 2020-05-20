using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Iid;
using Firebase.Messaging;
using Pump.Database;
using Xamarin.Forms.Internals;
using Pump.Database.Table;

namespace Pump.Droid.Firebase
{

    [Service]
    [IntentFilter(new[] {"com.google.firebase.MESSAGING_EVENT"})]
    public class MyFirebaseIIDService : FirebaseMessagingService
    {
        public override void OnNewToken(string token)
        {

            SendRegistrationToServer(token);
        }

        public void SendRegistrationToServer(string token)
        {
            new DatabaseController().setNotificationToken(new NotificationToken(token));
        }
    }
}
