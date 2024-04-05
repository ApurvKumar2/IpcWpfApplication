namespace Ipc.Base
{
    public interface IIpcMmfServer<in T>
    {
        void Send(T data);
    }
}
