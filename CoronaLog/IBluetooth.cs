using System;
namespace CoronaLog
{
    public interface IBluetooth
    {

        void StartServer();

        event EventHandler OnLeScan;
    }
}


