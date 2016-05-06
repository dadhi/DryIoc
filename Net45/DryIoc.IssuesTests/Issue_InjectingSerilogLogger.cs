using System;
using NUnit.Framework;
using Serilog;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue_InjectingSerilogLogger
    {
        [Test]
        public void Test()
        {
            var c = new Container();

            c.Register(Made.Of(() => Serilog.Log.Logger), 
                setup: Setup.With(condition: r => r.Parent.ImplementationType == null));
            c.Register(Made.Of(() => Serilog.Log.ForContext(Arg.Index<Type>(0)), r => r.Parent.ImplementationType),
                setup: Setup.With(condition: r => r.Parent.ImplementationType != null));

            c.Resolve<Serilog.ILogger>();

            c.Register<LogSubject>();
            c.Resolve<LogSubject>();
        }

        public class LogSubject
        {
            public ILogger Logger { get; private set; }
            public LogSubject(Serilog.ILogger logger)
            {
                Logger = logger;
            }
        }
    }
}
