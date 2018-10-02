using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue_InjectingSerilogLogger
    {
        [Test]
        public void Test()
        {
            var c = new Container();

            c.Register(Made.Of(() => Log.Logger), 
                setup: Setup.With(condition: r => r.Parent.ImplementationType == null));
            c.Register(Made.Of(() => Log.ForContext(Arg.Index<Type>(0)), r => r.Parent.ImplementationType),
                setup: Setup.With(condition: r => r.Parent.ImplementationType != null));

            c.Resolve<ILogger>();

            c.Register<LogSubject>();
            c.Resolve<LogSubject>();
        }

        public class LogSubject
        {
            public ILogger Logger { get; private set; }
            public LogSubject(ILogger logger)
            {
                Logger = logger;
            }
        }

        public interface ILogger
        {
            Type Type { get; }
        }

        public static class Log
        {
            public static readonly ILogger Logger = new DefaultLogger();

            public class DefaultLogger : ILogger {
                public Type Type { get { return null; } }
            }

            public static ILogger ForContext(Type context)
            {
                return new ContextualLogger(context);
            }

            public class ContextualLogger : ILogger
            {
                public Type Type { get; set; }

                public ContextualLogger(Type type)
                {
                    Type = type;
                }
            }
        }
    }
}
