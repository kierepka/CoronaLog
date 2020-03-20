using System;

using CoronaLog.Models;

namespace CoronaLog.ViewModels
{
    public class ItemDetailViewModel : BaseViewModel
    {
        public PeopleMeet Item { get; set; }
        public ItemDetailViewModel(PeopleMeet item = null)
        {
            Title = item?.Nick;
            Item = item;
        }
    }
}
