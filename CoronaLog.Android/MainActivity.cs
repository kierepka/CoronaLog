using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android;


[assembly: MetaData ("com.google.android.geo.API_KEY", Value="AIzaSyC1SHfcMHxFdb2JKq-6pbXbKMAl52o0lVA")]

namespace CoronaLog.Droid
{
    [Activity(Label = "CoronaHack", Icon = "@mipmap/icon", Theme = "@style/splash", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.SetTheme(Resource.Style.MainTheme);
            base.OnCreate(savedInstanceState);

            global::Xamarin.Forms.Forms.SetFlags("CollectionView_Experimental");
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Xamarin.FormsMaps.Init(this, savedInstanceState);

            LoadApplication(new App());
            RequestPermissions(new[]
            {
                Manifest.Permission.AccessCoarseLocation,
                Manifest.Permission.AccessFineLocation,
                Manifest.Permission.BluetoothPrivileged
            }, 0);
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}