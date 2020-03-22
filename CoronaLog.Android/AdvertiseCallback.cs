using System;
using CoronaLog.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidBluetooth))]
namespace CoronaLog.Droid
{
    public class AdvertiseCallback : Android.Bluetooth.LE.AdvertiseCallback
    {
    }
}
