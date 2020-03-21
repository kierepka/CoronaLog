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
        private CancellationTokenSource _cancellationTokenSource;
        readonly IAdapter adapter;
        private IGattServer server;
         IDisposable timer;
        public string ServerText { get; set; } = "Start Server";
        public string CharacteristicValue { get; set; }
        public string Output { get; private set; }

        public MeetPeopleDataStore()
        {
            adapter = CrossBleAdapter.Current;
        }

        public async Task<bool> StartMeeting ()
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

            if (server == null)
            {
                await BuildServer();
                adapter.Advertiser.Start(new AdvertisementData
                {
                   LocalName = "CoronaHack",
                   AndroidIsConnectable = true,
                   AndroidUseDeviceName = false
                });
            }
            else
            {
                ServerText = "Start Server";
                adapter.Advertiser.Stop();
                server.Dispose();
                server = null;
                timer?.Dispose();
            }
       
            return await Task.FromResult(true);
        }

        async Task BuildServer()
        {
            try
            {
             
                server = await adapter.CreateGattServer();
                
                var counter = 0;
                var service = server.CreateService(Guid.Parse("A495FF20-C5B1-4B44-B512-1370F02D74DE"), true);
                BuildCharacteristics(service, Guid.Parse("A495FF21-C5B1-4B44-B512-1370F02D74DE")); // scratch #1
                BuildCharacteristics(service, Guid.Parse("A495FF22-C5B1-4B44-B512-1370F02D74DE")); // scratch #2
                BuildCharacteristics(service, Guid.Parse("A495FF23-C5B1-4B44-B512-1370F02D74DE")); // scratch #3
                BuildCharacteristics(service, Guid.Parse("A495FF24-C5B1-4B44-B512-1370F02D74DE")); // scratch #4
                BuildCharacteristics(service, Guid.Parse("A495FF25-C5B1-4B44-B512-1370F02D74DE")); // scratch #5
                server.AddService(service);
                ServerText = "Stop Server";

                timer = Observable
                    .Interval(TimeSpan.FromSeconds(1))
                    .Select(_ => Observable.FromAsync(async ct =>
                    {
                        var subscribed = service.Characteristics.Where(x => x.SubscribedDevices.Count > 0);
                        foreach (var ch in subscribed)
                        {
                            counter++;
                            await ch.BroadcastObserve(Encoding.UTF8.GetBytes(counter.ToString()));
                        }
                    }))
                    .Merge(5)
                    .Subscribe();

                server
                    .WhenAnyCharacteristicSubscriptionChanged()
                    .Subscribe(x =>                       
                        OnEvent($"[WhenAnyCharacteristicSubscriptionChanged] UUID: {x.Characteristic.Uuid} - Device: {x.Device.Uuid} - Subscription: {x.IsSubscribing}")
                    ) ;

                //descriptor.WhenReadReceived().Subscribe(x =>
                //    OnEvent("Descriptor Read Received")
                //);
                //descriptor.WhenWriteReceived().Subscribe(x =>
                //{
                //    var write = Encoding.UTF8.GetString(x.Value, 0, x.Value.Length);
                //    OnEvent($"Descriptor Write Received - {write}");
                //});
                OnEvent("GATT Server Started");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Error building gatt server - " + ex, "OK");
            }
        }

        void OnEvent(string msg) => Device.BeginInvokeOnMainThread(() =>
            Output += msg + Environment.NewLine + Environment.NewLine
        );

        void BuildCharacteristics(Plugin.BluetoothLE.Server.IGattService service, Guid characteristicId)
        {
            var characteristic = service.AddCharacteristic(
                characteristicId,
                CharacteristicProperties.Notify | CharacteristicProperties.Read | CharacteristicProperties.Write | CharacteristicProperties.WriteNoResponse,
                GattPermissions.Read | GattPermissions.Write
            );

            characteristic
                .WhenDeviceSubscriptionChanged()
                .Subscribe(e =>
                {
                    var @event = e.IsSubscribed ? "Subscribed" : "Unsubcribed";
                    OnEvent($"Device {e.Device.Uuid} {@event}");
                    OnEvent($"Charcteristic Subcribers: {characteristic.SubscribedDevices.Count}");
                });

            characteristic.WhenReadReceived().Subscribe(x =>
            {
                var write = CharacteristicValue;
                if (String.IsNullOrWhiteSpace(write))
                    write = "(NOTHING)";

                x.Value = Encoding.UTF8.GetBytes(write);
                OnEvent("Characteristic Read Received");
            });
            characteristic.WhenWriteReceived().Subscribe(x =>
            {
                var write = Encoding.UTF8.GetString(x.Value, 0, x.Value.Length);
                OnEvent($"Characteristic Write Received - {write}");
            });
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
