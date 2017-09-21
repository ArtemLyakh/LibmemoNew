using System;
using Xamarin.Forms;

namespace Libmemo
{
    public partial class App : Application
    {
		#region Services
		private static ToastNotificator _toastNotificator;
		public static ToastNotificator ToastNotificator
		{
			get
			{
				if (_toastNotificator == null)
				{
					_toastNotificator = new ToastNotificator();
				}
				return _toastNotificator;
			}
		}
		#endregion

		#region Pages

		public static void SetShowMenu(bool show = true)
		{
            (Application.Current.MainPage as Pages.Core.Main).IsPresented = show;
		}

		public static void InitMenu()
		{
            ((Application.Current.MainPage as Pages.Core.Main).Master as Pages.Core.Menu).SetMenuPage();
		}

		public static Pages.Core.Main GlobalPage => (Pages.Core.Main)Application.Current.MainPage;

		#endregion

		public App()
        {
            InitializeComponent();
            //TK.CustomMap.Api.Google.GmsDirection.Init("AIzaSyCFwd7VMckhN6zZdbmCfGO0WXvJyyqh1OA");

            MainPage = new Pages.Core.Main();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
