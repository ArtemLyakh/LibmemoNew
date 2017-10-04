using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using UserNotifications;
using Xamarin.Forms;

namespace Libmemo.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            global::FFImageLoading.Forms.Touch.CachedImageRenderer.Init();
			DependencyService.Register<Plugin.Toasts.ToastNotification>(); // Register your dependency
            Plugin.Toasts.ToastNotification.Init();
            Xamarin.FormsMaps.Init();

			App.ScreenWidth = UIScreen.MainScreen.Bounds.Width;
			App.ScreenHeight = UIScreen.MainScreen.Bounds.Height;

            GetPermissions(app);

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        private void GetPermissions(UIApplication app)
        {
			// Request Permissions
			if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
			{
				// Request Permissions
				UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound, (granted, error) =>
				{
					// Do something if needed
				});
			}
			else if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
			{
				var notificationSettings = UIUserNotificationSettings.GetSettingsForTypes(
				UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound, null);

				app.RegisterUserNotificationSettings(notificationSettings);
			}
        }
    }
}
