using Android.App;
using Android.Content;
using Android.Media;
using Android.Support.V4.App;
using Android.Util;
using Gcm.Client;
using System;
using System.Text;
using WindowsAzure.Messaging;
using Movies.Services;

namespace Movies.Droid.Services
{
    [Service]
    public class GcmService : GcmServiceBase
    {
        public const string SenderId = "51649508836";
        private NotificationHub _hub;

        public GcmService() : base(SenderId)
        {
        }

        protected override void OnRegistered(Context context, string registrationId)
        {
            _hub = new NotificationHub(NotificationHubData.NotificationHubName, 
                                       NotificationHubData.ListenConnectionString, context);

            try
            {
                _hub.UnregisterAll(registrationId);
                _hub.Register(registrationId);
            }
            catch (Exception e)
            {
                Log.WriteLine(LogPriority.Debug, "GcmService", e.Message);
            }
        }

        protected override void OnMessage(Context context, Intent intent)
        {
            var message = new StringBuilder();

            if (intent?.Extras == null)
            {
                return;
            }

            foreach (var key in intent.Extras.KeySet())
            {
                message.AppendLine(key + "=" + intent.Extras.Get(key));
            }

            var text = intent.Extras.GetString("message");
            if (!string.IsNullOrEmpty(text))
            {
                CreateNotification("Hub message", text);
            }
            else
            {
                CreateNotification("Unknown message format", message.ToString());
            }
        }

        void CreateNotification(string title, string description)
        {
            var notificationManager = GetSystemService(NotificationService) as NotificationManager;

            var uiIntent = new Intent(this, typeof(MainActivity));
            var builder = new NotificationCompat.Builder(this);

            var notification = builder.SetContentIntent(PendingIntent.GetActivity(this, 0, uiIntent, 0))
                .SetSmallIcon(Android.Resource.Drawable.SymActionEmail)
                .SetTicker(title)
                .SetContentTitle(title)
                .SetContentText(description)
                .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))
                .SetAutoCancel(true).Build();

            //Show the notification
            notificationManager?.Notify(1, notification);
        }

        protected override void OnUnRegistered(Context context, string registrationId)
        {
            CreateNotification("GcmService - GCM Registered", "Device has been unregistered");
        }

        protected override void OnError(Context context, string errorId)
        {
            Log.WriteLine(LogPriority.Debug, "GcmService", "GCM Error: " + errorId);
        }
    }
}