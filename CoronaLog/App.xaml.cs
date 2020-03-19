using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CoronaLog.Services;
using CoronaLog.Views;

namespace CoronaLog
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MeetPeopleDataStore>();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
