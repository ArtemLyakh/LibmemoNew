using System;
using System.Threading.Tasks;
using Plugin.Toasts;
using Xamarin.Forms;

namespace Libmemo
{
    public class ToastNotificator
    {
		public ToastNotificator() { }

		public void Show(string text)
		{
            switch (Device.RuntimePlatform) {
                case Device.Android:
                    DependencyService.Get<IToastNotification>().Show(text);
                    break;
                case Device.iOS:
                    DependencyService.Get<IToastNotificator>().Notify(new NotificationOptions
                    {
                        Description = text,
                        ClearFromHistory = true,
                        IsClickable = false,
                    });
                    break;
            }
		}
    }
}
