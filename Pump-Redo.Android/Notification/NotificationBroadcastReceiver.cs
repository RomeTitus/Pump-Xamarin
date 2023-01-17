
using Android.Content;
namespace Pump.Droid.Notification
{
    [BroadcastReceiver(Enabled = true, Label = "Local Notifications Broadcast Receiver")]
    public class NotificationBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent?.Extras != null)
            {
                string title = intent.GetStringExtra(AndroidNotificationManager.TitleKey);
                string message = intent.GetStringExtra(AndroidNotificationManager.MessageKey);
                string controllerName = intent.GetStringExtra(AndroidNotificationManager.ControllerNameKey);

                AndroidNotificationManager manager =
                    AndroidNotificationManager.Instance ?? new AndroidNotificationManager();
                manager.Show(title, message, controllerName);
            }
        }
    }
}


