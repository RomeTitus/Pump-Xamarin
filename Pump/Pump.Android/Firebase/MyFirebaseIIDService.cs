using Android.App;
using Android.Util;
using Firebase.Iid;
using Pump.Azure;
using Pump.Database;
using Pump.Database.Table;
using System.Collections.Generic;
using WindowsAzure.Messaging;

namespace Pump.Droid.Firebase
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class MyFirebaseIIDService : FirebaseInstanceIdService
    {
        NotificationHub hub;
        const string TAG = "MyFirebaseIIDService";

        public MyFirebaseIIDService()
        {

        }
        public override void OnTokenRefresh()
        {
            var refreshedToken = FirebaseInstanceId.Instance.Token;
            SendRegistrationToServer(refreshedToken);
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