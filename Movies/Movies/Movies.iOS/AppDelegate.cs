using System;
using WindowsAzure.Messaging;
using Foundation;
using ImageCircle.Forms.Plugin.iOS;
using Movies.iOS.Services;
using Movies.Services;
using UIKit;

namespace Movies.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        private SBNotificationHub _notificationHub;

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(
                    UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                    new NSSet());

                UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);
                UIApplication.SharedApplication.RegisterForRemoteNotifications();
            }
            else
            {
                var notificationTypes = UIRemoteNotificationType.Alert | UIRemoteNotificationType.Badge | UIRemoteNotificationType.Sound;
                UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(notificationTypes);
            }

            Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

            global::Xamarin.Forms.Forms.Init();
            ImageCircleRenderer.Init();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            _notificationHub = new SBNotificationHub(NotificationHubData.ListenConnectionString,
                                                     NotificationHubData.NotificationHubName);

            _notificationHub.UnregisterAllAsync(deviceToken, error =>
            {
                if (error != null)
                {
                    Console.WriteLine($"Error Unregistering: {error.ToString()}");
                    return;
                }

                var tags = new NSSet(); // Tags
                _notificationHub.RegisterNativeAsync(deviceToken, tags, (errorCallback) =>
                {
                    if (errorCallback != null)
                    {
                        Console.WriteLine($"RegisterNativeAsync error: {errorCallback}");
                    }
                });
            });
        }

        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            if (null == userInfo || !userInfo.ContainsKey(new NSString("aps")))
            {
                return;
            }

            var aps = userInfo.ObjectForKey(new NSString("aps")) as NSDictionary;
            var alert = string.Empty;

            if (aps != null && aps.ContainsKey(new NSString("alert")))
            {
                alert = (aps[new NSString("alert")] as NSString)?.ToString();
            }

            if (string.IsNullOrEmpty(alert))
            {
                return;
            }

            var alertView = new UIAlertView("Notification", alert, null, "OK", null);
            alertView.Show();
        }
    }
}
