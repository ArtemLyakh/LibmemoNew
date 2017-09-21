using System;
using Foundation;
using Libmemo.iOS.Dependencies;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(IToastNotificationsImplementation))]
namespace Libmemo.iOS.Dependencies
{
    public class IToastNotificationsImplementation : IToastNotification
    {
		const double DELAY = 2.0;

		NSTimer alertDelay;
		UIAlertController alert;

        public void Show(string text)
        {
            ShowAlert(text, DELAY);
        }

		void ShowAlert(string message, double seconds)
		{
			alertDelay = NSTimer.CreateScheduledTimer(seconds, (obj) =>
			{
				dismissMessage();
			});
			alert = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);
			UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);
		}

		void dismissMessage()
		{
			if (alert != null)
			{
				alert.DismissViewController(true, null);
			}
			if (alertDelay != null)
			{
				alertDelay.Dispose();
			}
		}
    }
}
