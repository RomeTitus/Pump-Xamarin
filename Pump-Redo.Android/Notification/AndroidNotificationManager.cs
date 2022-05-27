/*
using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Plugin.FirebasePushNotification;
using Pump.Class;
using Pump.Database;
using Pump.Droid.Notification;
using Pump.Notification;
using Xamarin.Forms;
using Application = Android.App.Application;
using NotificationPriority = Plugin.FirebasePushNotification.NotificationPriority;
using String = Java.Lang.String;
using Pump.Droid;
using Android.Support.V4.App;

[assembly: Dependency(typeof(AndroidNotificationManager))]

namespace Pump.Droid.Notification
{
    class AndroidNotificationManager : DefaultPushNotificationHandler, INotificationManager
    {
        public new const string TitleKey = "title";
        public new const string MessageKey = "message";
        public const string ControllerNameKey = "btmac";
        const string ChannelId = "default";
        const string ChannelName = "Default";
        const string ChannelDescription = "The default channel for notifications.";
        private readonly Context _mContext;
        private bool _channelInitialized;
        private NotificationManager _manager;
        int _messageId;
        int _pendingIntentId;

        public AndroidNotificationManager()
        {
            _mContext = Application.Context;
            if (Instance == null)
            {
                CreateNotificationChannel();
                Instance = this;
            }
        }

        public static AndroidNotificationManager Instance { get; private set; }
        public event EventHandler NotificationReceived;

        public void SendNotification(string title, string message, string BTmac, DateTime? notifyTime = null)
        {
            var controller = new DatabaseController().GetControllerNameByMac(BTmac);
            if (controller == null) return;

            if (!_channelInitialized)
            {
                CreateNotificationChannel();
            }

            if (notifyTime != null)
            {
                Intent intent = new Intent(_mContext, typeof(AndroidNotificationManager));
                intent.PutExtra(TitleKey, title);
                intent.PutExtra(MessageKey, message);
                intent.PutExtra(ControllerNameKey, controller.Name);


                PendingIntent pendingIntent = PendingIntent.GetBroadcast(_mContext, _pendingIntentId++, intent,
                    PendingIntentFlags.CancelCurrent);
                long triggerTime = GetNotifyTime(notifyTime.Value);
                AlarmManager alarmManager = _mContext.GetSystemService(Context.AlarmService) as AlarmManager;
                alarmManager?.Set(AlarmType.RtcWakeup, triggerTime, pendingIntent);
            }
            else
            {
                Show(title, message, controller.Name);
            }
        }

        public void ReceiveNotification(string title, string message, string controllerName)
        {
            var args = new NotificationEventArgs
            {
                Title = title,
                Message = message,
                ControllerName = controllerName
            };
            NotificationReceived?.Invoke(null, args);
        }


        public override void OnReceived(IDictionary<string, object> parameters)
        {
            //Do Not show notifications for now.... we will use Custom Ones
        }

        void CreateNotificationChannel()
        {
            _manager = (NotificationManager)_mContext.GetSystemService(Context.NotificationService);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelNameJava = new String(ChannelName);
                var channel = new NotificationChannel(ChannelId, channelNameJava, NotificationImportance.Default)
                {
                    Description = ChannelDescription
                };

                channel.EnableLights(true);
                channel.EnableVibration(true);
                //channel.SetSound(sound, alarmAttributes);
                channel.SetShowBadge(true);
                channel.Importance = NotificationImportance.High;
                channel.SetVibrationPattern(new long[] { 100, 200, 300, 400, 500, 400, 300, 200, 400 });
                _manager?.CreateNotificationChannel(channel);
            }

            _channelInitialized = true;
        }

        public void Show(string title, string message, string controllerName)
        {
            Intent intent = new Intent(_mContext, typeof(MainActivity));
            intent.PutExtra(title, message);
            intent.AddFlags(ActivityFlags.ClearTop);

            PendingIntent pendingIntent = PendingIntent.GetActivity(_mContext, _pendingIntentId++, intent,
                PendingIntentFlags.UpdateCurrent);



            NotificationCompat.Builder builder = new NotificationCompat.Builder(_mContext, ChannelId)
                .SetContentIntent(pendingIntent)
                .SetContentTitle(title)
                .SetSubText(controllerName)
                .SetAutoCancel(true)
                .SetChannelId(ChannelId)
                .SetPriority((int)NotificationPriority.High)
                .SetVibrate(new long[0])
                .SetVisibility((int)NotificationVisibility.Public)
                .SetLargeIcon(BitmapFactory.DecodeResource(_mContext.Resources, Resource.Drawable.FieldSun))
                .SetSmallIcon(Resource.Drawable.Logo)
                .SetStyle(new NotificationCompat.BigTextStyle()
                    .BigText(message))
                .SetDefaults((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate);

            var notification = builder.Build();
            _manager.Notify(_messageId++, notification);

            /*
            try
            {
                Intent intent = new Intent(_mContext, typeof(MainActivity));
                intent.AddFlags(ActivityFlags.ClearTop);
                intent.PutExtra(title, message);
                var pendingIntent = PendingIntent.GetActivity(_mContext, 0, intent, PendingIntentFlags.OneShot);

                //var sound = global::Android.Net.Uri.Parse(ContentResolver.SchemeAndroidResource + "://" + mContext.PackageName + "/" + Resource.Raw.notification);
                // Creating an Audio Attribute
                var alarmAttributes = new AudioAttributes.Builder()
                    .SetContentType(AudioContentType.Sonification)
                    ?.SetUsage(AudioUsageKind.Notification)
                    ?.Build();

                var mBuilder = new NotificationCompat.Builder(_mContext, ChannelId);
                mBuilder
                    .SetContentTitle(title) 
                    //.SetSound(sound)
                    .SetAutoCancel(true)
                    .SetContentTitle(title)
                    .SetContentText(message)
                    .SetChannelId(ChannelId)
                    .SetPriority((int)NotificationPriority.High)
                    .SetVibrate(new long[0])
                    .SetDefaults((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate)
                    .SetVisibility((int)NotificationVisibility.Public)
                    .SetSmallIcon(Resource.Drawable.Logo)
                    .SetContentIntent(pendingIntent);



                NotificationManager notificationManager = _mContext.GetSystemService(Context.NotificationService) as NotificationManager;

                if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.O)
                {
                    NotificationImportance importance = global::Android.App.NotificationImportance.High;

                    NotificationChannel notificationChannel = new NotificationChannel(ChannelId, title, importance);
                    notificationChannel.EnableLights(true);
                    notificationChannel.EnableVibration(true);
                    //notificationChannel.SetSound(sound, alarmAttributes);
                    notificationChannel.SetShowBadge(true);
                    notificationChannel.Importance = NotificationImportance.High;
                    notificationChannel.SetVibrationPattern(new long[] { 100, 200, 300, 400, 500, 400, 300, 200, 400 });

                    if (notificationManager != null)
                    {
                        mBuilder.SetChannelId(ChannelId);
                        notificationManager.CreateNotificationChannel(notificationChannel);
                    }
                }

                notificationManager?.Notify(0, mBuilder.Build());
            }
            catch (Exception ex)
            {
                Log.Error("PumpNotification", "Exception: " + ex.ToString());
            }
            *0/
        }

        private long GetNotifyTime(DateTime notifyTime)
        {
            var utcTime = TimeZoneInfo.ConvertTimeToUtc(notifyTime);
            var epochDiff = (new DateTime(1970, 1, 1) - DateTime.MinValue).TotalSeconds;
            var utcAlarmTime = utcTime.AddSeconds(-epochDiff).Ticks / 10000;
            return utcAlarmTime; // milliseconds
        }
    }
}
*/