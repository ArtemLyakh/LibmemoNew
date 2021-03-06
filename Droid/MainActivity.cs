﻿using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Forms;

namespace Libmemo.Droid
{
    [Activity(Label = "Libmemo.Droid", Icon = "@drawable/icon", Theme = "@style/MainTheme",ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, WindowSoftInputMode = SoftInput.AdjustPan)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            global::FFImageLoading.Forms.Droid.CachedImageRenderer.Init();
			DependencyService.Register<Plugin.Toasts.ToastNotification>(); // Register your dependency
            Plugin.Toasts.ToastNotification.Init(this);
            Xamarin.FormsMaps.Init(this, bundle);

			var width = Resources.DisplayMetrics.WidthPixels;
			var height = Resources.DisplayMetrics.HeightPixels;
			var density = Resources.DisplayMetrics.Density;

			App.ScreenWidth = (width - 0.5f) / density;
			App.ScreenHeight = (height - 0.5f) / density;

            LoadApplication(new App());
        }
    }
}
