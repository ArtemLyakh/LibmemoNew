using System;
using Android.Widget;
using Libmemo.Droid.Dependencies;
using Xamarin.Forms;

[assembly: Dependency(typeof(IToastNotificationsImplementation))]
namespace Libmemo.Droid.Dependencies
{
	public class IToastNotificationsImplementation : IToastNotification
	{

		public void Show(string text)
		{
			Toast.MakeText(Forms.Context, text, ToastLength.Short).Show();
		}
	}
}
