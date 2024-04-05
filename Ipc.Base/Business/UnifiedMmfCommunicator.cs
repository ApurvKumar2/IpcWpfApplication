using System;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ipc.Base
{
    public class UnifiedMmfCommunicator :IIpcMmfClient<string>, IIpcMmfServer<string>
    {
        public static UnifiedMmfCommunicator Instance = new UnifiedMmfCommunicator();
        private UnifiedMmfCommunicator()
        { }

        public event DataHandler<string> DataReceived;
        private readonly ManualResetEvent _killer = new ManualResetEvent(false);

        void IIpcMmfClient<string>.StopPolling()
        {
            _killer.Set();
        }

        void IDisposable.Dispose()
        {
            (this as IIpcMmfClient<string>).StopPolling();
            _killer.Dispose();
        }

        public void StartPolling()
        {
            _killer.Reset();
            //var context = Thread.CurrentContext;
            var context = SynchronizationContext.Current;
            Task.Factory.StartNew(() =>
            {
                var evt = GetSharedWaitHandle();
                using (evt)
                using (var file = MemoryMappedFile.CreateOrOpen(MmfConstants.MemoryMappedFileName,
                           MmfConstants.MaxCapacity))
                using (var view = file.CreateViewAccessor())
                {
                    var data = new byte[MmfConstants.MaxCapacity];
                    while (WaitHandle.WaitAny(new WaitHandle[] { _killer, evt }) == 1)
                    {
                        view.ReadArray(0, data, 0, data.Length);
                        var convertedData = SpawnData(data);
                        if (context != null)
                            context.Send((a) => { DataReceived?.Invoke(convertedData); }, new object());
                        else
                            DataReceived?.Invoke(convertedData);
                        //context.DoCallBack(() => { DataReceived?.Invoke(convertedData); });
                    }
                }
            });
        }

        //private int GetByteSizeOfData()
        //{
        //    var type = typeof(T);

        //    if (type.IsEnum)
        //    {
        //        return Marshal.SizeOf(Enum.GetUnderlyingType(type));
        //    }
        //    if (type.IsValueType)
        //    {
        //        return Marshal.SizeOf(type);
        //    }
        //    if (type == typeof(string))
        //    {
        //        return Encoding.Default.GetByteCount(type.ToString());
        //    }

        //    return IntPtr.Size;
        //}

        public void Send(string data)
        {
            var evt = GetSharedWaitHandle();
            var dataStream = StreamData(data);
            using (evt)
            using (var file = MemoryMappedFile.CreateOrOpen(MmfConstants.MemoryMappedFileName, MmfConstants.MaxCapacity))
            using (var view = file.CreateViewAccessor())
            {
                view.WriteArray(0, dataStream, 0, dataStream.Length);
                evt.Set();
            }
        }

        private static byte[] StreamData(string data)
        {
            //var bf = new BinaryFormatter();
            //using (var ms = new MemoryStream())
            //{
            //    bf.Serialize(ms, data);
            //    return ms.ToArray();
            //}

            return Encoding.Default.GetBytes(data);
        }

        private static string SpawnData(byte[] byteStream)
        {
            //using (var memStream = new MemoryStream())
            //{
            //    var binForm = new BinaryFormatter();
            //    memStream.Write(byteStream, 0, byteStream.Length);
            //    memStream.Seek(0, SeekOrigin.Begin);
            //    var obj = binForm.Deserialize(memStream);
            //    return (T)obj;
            //}

            return Encoding.Default.GetString(byteStream);
        }

        private static EventWaitHandle GetSharedWaitHandle()
        {
            if (EventWaitHandle.TryOpenExisting(MmfConstants.EventHandleName, out var evt) == false)
            {
                evt = new EventWaitHandle(false, EventResetMode.AutoReset, MmfConstants.EventHandleName);
            }

            return evt;
        }
    }
}
