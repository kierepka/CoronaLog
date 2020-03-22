using System;
using System.Collections.Generic;
using Android.Bluetooth.LE;
using Android.Runtime;
using CoronaLog.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidBluetooth))]
namespace CoronaLog.Droid
{
    class ScanCallback : Android.Bluetooth.LE.ScanCallback
    {
        public event EventHandler OnScanFinished;
        public override void OnBatchScanResults(IList<ScanResult> results)
        {
            OnScanFinished?.Invoke(this, new EventArgs());
            base.OnBatchScanResults(results);
        }
        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
        {
            OnScanFinished?.Invoke(this, new EventArgs());
        }
    }
}
