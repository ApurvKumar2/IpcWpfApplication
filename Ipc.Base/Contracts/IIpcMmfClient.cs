using System;

namespace Ipc.Base
{
    public interface IIpcMmfClient<out T> : IDisposable
    {
        event DataHandler<T> DataReceived;
        void StartPolling();
        void StopPolling();
    }

    public delegate void DataHandler<in T>(T data);
}
