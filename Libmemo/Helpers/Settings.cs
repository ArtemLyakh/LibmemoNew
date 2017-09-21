// Helpers/Settings.cs
using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace Libmemo
{
	public static class Settings
	{
		private static ISettings AppSettings => CrossSettings.Current;

		#region Setting Constants
        public const string SERVER_URL = "http://libmemo.com";

		public const string RELATIVES_URL = SERVER_URL + "/api2/relatives/";
		public const string ACCOUNT_URL = SERVER_URL + "/api2/account/";
		public const string LOGIN_URL = SERVER_URL + "/api2/auth/login/";
		public const string REGISTER_URL = SERVER_URL + "/api2/auth/register/";
		public const string TREE_URL = SERVER_URL + "/api2/tree/";
		public const string PERSONS_URL = SERVER_URL + "/api2/persons/";
		public const string MAP_URL = SERVER_URL + "/api2/map/";

		public const string ADMIN_USERS_URL = SERVER_URL + "/api2/admin/users/";
		public const string ADMIN_RELATIVES_URL = SERVER_URL + "/api2/admin/relatives/";
		public const string ADMIN_TREE_URL = SERVER_URL + "/api2/admin/tree/";
		#endregion


		private static bool _authInfoCacheNeedReset = true;
		private static AuthInfo _authInfoCache = null;
		public static AuthInfo AuthInfo
		{
			get
			{
				if (_authInfoCacheNeedReset)
				{
					var str = AppSettings.GetValueOrDefault("auth_data", null);
					if (string.IsNullOrWhiteSpace(str)) return null;

					_authInfoCache = AuthInfo.Deserialize(str);
					_authInfoCacheNeedReset = false;
				}

				return _authInfoCache;
			}
			set
			{
				_authInfoCacheNeedReset = true;
				AppSettings.AddOrUpdateValue("auth_data", value?.Serialize());
			}
		}


		private static bool _authCredentialsCacheNeedReset = true;
		private static AuthCredentials _authCredentialsCache = null;
		public static AuthCredentials AuthCredentials
		{
			get
			{
				if (_authCredentialsCacheNeedReset)
				{
					var str = AppSettings.GetValueOrDefault("auth_credentials", null);
					if (string.IsNullOrWhiteSpace(str)) return null;

					_authCredentialsCache = AuthCredentials.Deserialize(str);
					_authCredentialsCacheNeedReset = false;
				}

				return _authCredentialsCache;
			}
			set
			{
				_authCredentialsCacheNeedReset = true;
				AppSettings.AddOrUpdateValue("auth_credentials", value?.Serialize());
			}
		}


		private static bool _cookiesCacheNeedReset = true;
		private static CookieContainer _cookiesCache = null;
		public static CookieContainer Cookies
		{
			get
			{
				if (_cookiesCacheNeedReset)
				{
					var str = AppSettings.GetValueOrDefault("cookies", null);

					try
					{
						List<Cookie> cookieList = JsonConvert.DeserializeObject<List<Cookie>>(str);
						CookieCollection cookieCollection = new CookieCollection();
						foreach (var cookie in cookieList)
						{
							cookieCollection.Add(cookie);
						}

						_cookiesCache = new CookieContainer();
						_cookiesCache.Add(new Uri(Settings.SERVER_URL), cookieCollection);
					}
					catch
					{
						_cookiesCache = null;
					}
					finally
					{
						_cookiesCacheNeedReset = false;
					}
				}

				return _cookiesCache;
			}
			set
			{
				_cookiesCacheNeedReset = true;

				string str = null;
				if (value != null)
				{
					List<Cookie> cookieList = new List<Cookie>();
					foreach (var item in value.GetCookies(new Uri(Settings.SERVER_URL)))
					{
						var cookie = (Cookie)item;
						cookieList.Add(cookie);
					}
					if (cookieList.Count > 0)
					{
						str = JsonConvert.SerializeObject(cookieList);
					}
				}

				AppSettings.AddOrUpdateValue("cookies", str);
			}
		}

	}
}