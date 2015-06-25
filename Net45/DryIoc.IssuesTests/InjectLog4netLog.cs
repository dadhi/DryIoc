using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class InjectLog4netLog
    {
        [Test]
        public void Can_register_ILog_with_factory()
        {
            var container = new Container();
            container.Register<A>();

            container.Register<log4net.ILog>(Made.Of(
                () => log4net.LogManager.GetLogger(Arg.OfValue<Type>(0)), 
                request => request.ParentNonWrapper().ImplementationType));

            var a = container.Resolve<A>();
        }

        public class A
        {
            public log4net.ILog Log { get; private set; }

            public A(log4net.ILog log)
            {
                Log = log;
            }
        }
    }
}
