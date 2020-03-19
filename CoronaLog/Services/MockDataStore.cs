using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoronaLog.Models;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;

namespace CoronaLog.Services
{
    public class MeetPeopleDataStore : IDataStore<Item>
    {
        readonly List<Item> items = new List<Item>();
        IBluetoothLE ble;
        IAdapter adapter;

        public MeetPeopleDataStore()
        {
            ble = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;
        }

        public async Task<bool> AddItemAsync(Item item)
        {
            if (adapter.IsScanning) return await Task.FromResult(false);

            adapter.DeviceDiscovered += Adapter_DeviceDiscovered;

            await adapter.StartScanningForDevicesAsync();

            return await Task.FromResult(true);
        }

        private void Adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            var item = new Item
            {
                Description = e.Device.Name,
                Id = e.Device.Id,
                Text = e.Device.State.ToString()
            };

            items.Add(item);
        }

        public async Task<bool> UpdateItemAsync(Item item)
        {
            var oldItem = items.Where((Item arg) => arg.Id == item.Id).FirstOrDefault();
            items.Remove(oldItem);
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(Guid id)
        {
            var oldItem = items.Where((Item arg) => arg.Id == id).FirstOrDefault();
            items.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<Item> GetItemAsync(Guid id)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(items);
        }
    }
}