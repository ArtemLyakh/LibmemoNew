﻿using System;
using System.Threading.Tasks;

namespace Libmemo
{
    public static class AuthHelper
    {
		public static void Login(AuthInfo info, AuthCredentials credentials)
		{
			Settings.AuthInfo = info;
			Settings.AuthCredentials = credentials;

			App.InitMenu();

            AuthChanged?.Invoke(null, null);

			App.ToastNotificator.Show($"Добро пожаловать, {(!string.IsNullOrWhiteSpace(info.Fio) ? info.Fio : info.Email)}");
		}

        public static event EventHandler AuthChanged;

		private static void InnerLogout()
		{
			Settings.AuthInfo = null;
			App.InitMenu();

            AuthChanged?.Invoke(null, null);
		}

		public static async Task Logout()
		{
			InnerLogout();

			//принудительный сброс страницы
			await App.GlobalPage.PopToRootPage();
		}

		public static async Task ReloginAsync()
		{
			InnerLogout();

            await App.GlobalPage.PushRoot(new Pages.Auth.Login());
		}

		public static bool IsLogged { get => Settings.AuthInfo != null; }
		public static bool IsAdmin { get => Settings.AuthInfo?.IsAdmin ?? false; }
		public static int? CurrentUserId { get => Settings.AuthInfo?.UserId; }

		public static string UserEmail { get => Settings.AuthCredentials?.Email; }
		public static string UserPassword { get => Settings.AuthCredentials?.Password; }
		public static System.Net.CookieContainer CookieContainer { get => Settings.Cookies; }
    }
}
