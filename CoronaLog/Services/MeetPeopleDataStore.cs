using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoronaLog.Models;
using Plugin.BluetoothLE;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace CoronaLog.Services
{
    public class MeetPeopleDataStore : IDataStore<PeopleMeet>
    {
        readonly List<PeopleMeet> items = new List<PeopleMeet>();        
        private CancellationTokenSource _cancellationTokenSource;
        readonly IAdapter adapter;        

        public MeetPeopleDataStore()
        {
            adapter = CrossBleAdapter.Current;
        }

        public async Task<bool> ScannPeopleAsync()
        {

            if (adapter.IsScanning) return await Task.FromResult(false);

            if (Device.RuntimePlatform == Device.Android)
            {

                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                {

                    var permissionResult = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();


                    if (permissionResult != PermissionStatus.Granted)
                    {
                        await Application.Current.MainPage.DisplayAlert("Permisions", "Permission denied. Not scanning.", "OK");

                        return await Task.FromResult(false);
                    }
                }
            }
            var sconf = new ScanConfig
            {
                ScanType = BleScanType.Balanced
            };

            var results = adapter.Scan(sconf);
            results.Subscribe(
                r =>
                {
                    var item = new PeopleMeet
                    {
                        
                        Description = r.AdvertisementData.LocalName,
                        Id = r.Device.Uuid,
                        Nick = $"Status: {r.Device.Status}, AdvertisementData: {r.AdvertisementData}, Name: {r.Device.Name}"

                    };

                    var oldItem = items.Where((PeopleMeet arg) => arg.Id == item.Id).FirstOrDefault();
                    if (oldItem != null) 
                        items.Remove(oldItem);
                    items.Add(item);


                },
                 async ex =>
                 {
                     await Application.Current.MainPage.DisplayAlert("ERROR", ex.ToString(), "OK");
                 }                                  
            );
            
            return await Task.FromResult(true);
        }

        public async Task<bool> AddItemAsync(PeopleMeet item)
        {

            items.Add(item);
            return await Task.FromResult(true);
        }

       

        public async Task<bool> UpdateItemAsync(PeopleMeet item)
        {
            var oldItem = items.Where((PeopleMeet arg) => arg.Id == item.Id).FirstOrDefault();
            items.Remove(oldItem);
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(Guid id)
        {
            var oldItem = items.Where((PeopleMeet arg) => arg.Id == id).FirstOrDefault();
            items.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<PeopleMeet> GetItemAsync(Guid id)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<PeopleMeet>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(items);
        }
    }
}
