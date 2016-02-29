using System;
using System.Diagnostics;

namespace Web.Components
{
    public abstract class ServiceBase : IDisposable
    {
        private static int _instanceId;

        protected ServiceBase()
        {
            InstanceId = System.Threading.Interlocked.Increment(ref _instanceId);
            Debug.WriteLine($"Created[#{InstanceId:D3}]: {GetType().FullName}");
        }

        public int InstanceId { get; private set; }

        void IDisposable.Dispose()
        {
            Debug.WriteLine($"Disposed[#{InstanceId:D3}]: {GetType().FullName}");
        }
    }
}
