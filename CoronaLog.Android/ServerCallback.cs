using System;
using Android.Bluetooth;
using Android.Runtime;
using CoronaLog.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidBluetooth))]
namespace CoronaLog.Droid
{
    class ServerCallback : BluetoothGattServerCallback
    {
        public delegate void OnDeviceAddedHandler(BluetoothDevice device);
        public event OnDeviceAddedHandler OnDeviceAdded;

        public delegate void OnDescriptorReadRequestHandler(BluetoothDevice device, int requestId, int offset, BluetoothGattDescriptor descriptor);
        public event OnDescriptorReadRequestHandler OnReadRequest;

        public override void OnConnectionStateChange(BluetoothDevice device, [GeneratedEnum] ProfileState status, [GeneratedEnum] ProfileState newState)
        {
            if (status == ProfileState.Connected)
                OnDeviceAdded?.Invoke(device);

            base.OnConnectionStateChange(device, status, newState);
        }

        public override void OnDescriptorReadRequest(BluetoothDevice device, int requestId, int offset, BluetoothGattDescriptor descriptor)
        {
            OnReadRequest?.Invoke(device, requestId, offset, descriptor);
        }

        public override void OnCharacteristicReadRequest(BluetoothDevice device, int requestId, int offset, BluetoothGattCharacteristic characteristic)
        {
            Console.WriteLine(device.Name);
        }
    }
}
