using System;
using Xamarin.Essentials;

namespace CoronaLog.Models
{
    public class PeopleMeet
    {
        public Guid Id { get; set; }
        public string Nick { get; set; }
        public string Description { get; set; }
        public Location Location {get;set;}

        public List<PeopleMeet> People {get;set;}
    }
}