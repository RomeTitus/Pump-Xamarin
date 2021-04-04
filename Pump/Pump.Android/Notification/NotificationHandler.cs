using Android.App;
using Android.Runtime;
using System;
using System.Collections.Generic;
using Android.Util;
using Newtonsoft.Json.Linq;
using Plugin.FirebasePushNotification;
using Pump.SocketController;
using Application = Android.App.Application;

namespace Pump.Droid.Notification
{
    [Application]
    public class NotificationHandler : Application
    {
        public NotificationHandler(IntPtr handle, JniHandleOwnership transer) : base(handle, transer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();
            /*
            FirebasePushNotificationManager.NotificationActivityType = typeof(MainActivity);
            FirebasePushNotificationManager.NotificationActivityFlags =
                ActivityFlags.ClearTop | ActivityFlags.SingleTop;

            //Set the default notification channel for your app when running Android Oreo
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                //Change for your default notification channel id here
                FirebasePushNotificationManager.DefaultNotificationChannelId = "FirebasePushNotificationChannel";

                //Change for your default notification channel name here
                FirebasePushNotificationManager.DefaultNotificationChannelName = "General";
            }


            //If debug you should reset the token each time.
#if DEBUG
            FirebasePushNotificationManager.Initialize(this, new NotificationUserCategory[]
            {
                new NotificationUserCategory("message",new List<NotificationUserAction> {
                    new NotificationUserAction("Reply","Reply",NotificationActionType.Foreground),
                    new NotificationUserAction("Forward","Forward",NotificationActionType.Foreground)

                }),
                new NotificationUserCategory("request",new List<NotificationUserAction> {
                    new NotificationUserAction("Accept","Accept",NotificationActionType.Default,"check"),
                    new NotificationUserAction("Reject","Reject",NotificationActionType.Default,"cancel")
                })

            }, true);
#else
	            FirebasePushNotificationManager.Initialize(this,new NotificationUserCategory[]
		    {
			new NotificationUserCategory("message",new List<NotificationUserAction> {
			    new NotificationUserAction("Reply","Reply",NotificationActionType.Foreground),
			    new NotificationUserAction("Forward","Forward",NotificationActionType.Foreground)

			}),
			new NotificationUserCategory("request",new List<NotificationUserAction> {
			    new NotificationUserAction("Accept","Accept",NotificationActionType.Default,"check"),
			    new NotificationUserAction("Reject","Reject",NotificationActionType.Default,"cancel")
			})

		    },false);
#endif
*/
#if DEBUG
            FirebasePushNotificationManager.Initialize(this,new AndroidNotificationManager(), true,false);
#else
	            FirebasePushNotificationManager.Initialize(this, new AndroidNotificationManager(), false, false);
#endif

            //FirebasePushNotificationManager.Initialize(this, new AndroidNotificationManager(), false, false);
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

            
            CrossFirebasePushNotification.Current.OnTokenRefresh += async (s, p) =>
            {
                if(string.IsNullOrEmpty(p.Token))
                    return;
                var notification = new IrrigationController.NotificationToken
                {
                    ID = Android.Provider.Settings.Secure.GetString(Context.ContentResolver,
                        Android.Provider.Settings.Secure.AndroidId),
                    Token = p.Token
                };
                Log.Info("PumpNotification", $"NewToken: { p.Token}");
                var result = await new SocketPicker().SendCommand(notification);
            };
            
        }
    }
}