using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoronaLog.Models;
using Plugin.BluetoothLE;
using Plugin.BluetoothLE.Server;
using Xamarin.Essentials;
using Xamarin.Forms;


namespace CoronaLog.Services
{
    public class MeetPeopleDataStore : IDataStore<PeopleMeet>
    {
        readonly List<PeopleMeet> items = new List<PeopleMeet>();
        readonly Guid CoronaHackUid = Guid.Parse("00001805-0000-1000-8000-00805f9b34fb");
        readonly IAdapter adapter;


        public string ServerText { get; set; } = "Start Server";
        public string CharacteristicValue { get; set; }
        public string Output { get; private set; }

        public MeetPeopleDataStore()
        {
            adapter = CrossBleAdapter.Current;
        }

        public async Task<bool> StartMeeting()
        {

            if (adapter.Status != AdapterStatus.PoweredOn)
            {
                await Application.Current.MainPage.DisplayAlert("Error:", "Could not start GATT Server.  Adapter Status: " + adapter.Status, "OK");
                return await Task.FromResult(false);
            }

            if (!adapter.Features.HasFlag(AdapterFeatures.ServerGatt))
            {
                await Application.Current.MainPage.DisplayAlert("Error:", "GATT Server is not supported on this platform configuration", "OK");
                return await Task.FromResult(false);
            }

            DependencyService.Get<IBluetooth>().StartServer();

            return await Task.FromResult(true);

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
                    if (r.AdvertisementData != null)
                    {
                        if (r.AdvertisementData.ServiceUuids.Contains(CoronaHackUid))
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
                        }
                    }

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
