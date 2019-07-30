namespace Logging
{
    public class IgnoreLogger
        : ILogger
    {
        public IgnoreLogger(
        )
        {
        }
    }


    public interface ILogger
    {
    }


    public interface ILoggerConfiguration
    {
    }


    public class LoggerConfiguration
        : ILoggerConfiguration
    {
        public LoggerConfiguration(
        )
        {
        }
    }


    public interface ILoggerFactory
    {
        ILogger Create();
    }


    public class LoggerFactory
        : ILoggerFactory
    {
        public LoggerFactory(
            ILoggerConfiguration arg0,
            IRGWebApiClientFactory arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly ILoggerConfiguration field0;
        public readonly IRGWebApiClientFactory field1;
        public ILogger Create() => new IgnoreLogger();
    }


    public interface IRGWebApiClient
    {
    }


    public interface IRGWebApiClientFactory
    {
    }


    public class RGWebApiClientFactory
        : IRGWebApiClientFactory
    {
        public RGWebApiClientFactory(
        )
        {
        }
    }


    public class RGLogger
        : ILogger
    {
        public RGLogger(
            IRGWebApiClientFactory arg0,
            ILoggerConfiguration arg1
        )
        {
            field0 = arg0;
            field1 = arg1;
        }

        public readonly IRGWebApiClientFactory field0;
        public readonly ILoggerConfiguration field1;
    }
}