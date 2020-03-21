using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using CoronaLog.Models;
using CoronaLog.Views;

namespace CoronaLog.ViewModels
{
    public class MeetPeopleViewModel : BaseViewModel
    {
        public ObservableCollection<PeopleMeet> Items { get; set; }
        public Command LoadItemsCommand { get; set; }
        public Command ScannItemsCommand { get; set; }
        public Command StartMeetingCommand { get; set; }

        public MeetPeopleViewModel()
        {
            Title = "Browse";
            Items = new ObservableCollection<PeopleMeet>();
            ScannItemsCommand = new Command(async () => await ExecuteScanItemsCommand());
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            StartMeetingCommand = new Command(async () => await ExecuteStartMeetingCommand());
            MessagingCenter.Subscribe<NewItemPage, PeopleMeet>(this, "AddItem", async (obj, item) =>
            {
                var newItem = item as PeopleMeet;
                Items.Add(newItem);
                await DataStore.AddItemAsync(newItem);
            });
        }


        
        async Task ExecuteStartMeetingCommand()
        {
            IsBusy = true;
            try
            {
                var isOk = await DataStore.StartMeeting();
                if (isOk) await ExecuteLoadItemsCommand();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task ExecuteScanItemsCommand()
        {
            IsBusy = true;
            try
            {
                var isOk = await DataStore.ScannPeopleAsync();
                if (isOk) await ExecuteLoadItemsCommand();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await DataStore.GetItemsAsync(true);
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}