using System;

namespace Monitor
{
    public class LogConfiguration
    {
        public string LogName { get; }

        public LogConfiguration(
            string logName)
        {
            LogName = logName;
        }
    }

    public interface IDataCollectorClient
    {
    }


    public class DataCollectorClient : IDataCollectorClient
    {
    }

    public class ClientConfiguration
    {
        public string CustomerId { get; }
        public string SharedKey { get; }

        public ClientConfiguration(
            string customerId,
            string sharedKey)
        {
            CustomerId = customerId;
            SharedKey = sharedKey;
        }
    }

    public class AsynchronousDataCollectorClient : IDataCollectorClient, IDisposable
    {
        bool disposed;


        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    DisposeManagedObjects();
                }

                disposed = true;
            }
        }

        void DisposeManagedObjects()
        {

        }

        public void Dispose()
        {
            Dispose(true);
        }
    }

    public interface IMessageLogger
    {

    }

    public class CalendarSyncLogConfiguration : LogConfiguration
    {
        public CalendarSyncLogConfiguration(string logname) : base(logname)
        {
        }
    }


    public class MessageLogger : IMessageLogger
    {
        private readonly IDataCollectorClient DataCollectorClient;

        public MessageLogger(IDataCollectorClient dataCollectorClient)
        {
            DataCollectorClient = dataCollectorClient;
        }
    }
}