using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue402_Inconsistent_transient_disposable_behavior_when_using_Made : ITest
    {
        public int Run()
        {
            Test1();
            return 1;
        }

        [Test]
        public void Test1()
        {
            var container = new Container();

            container.Register<IService, Service>(
                setup: Setup.With(trackDisposableTransient: true), 
                made: Made.Of(() => new Service(Arg.Of<Func<object, ILogger>>(), Arg.Of<Service1>(), Arg.Of<Service2>(), Arg.Of<Service3>())));

            var logger = new ListLogger { Messages = new List<string>() };
            container.RegisterInstance<ILogger>(logger);
            
            container.Register<Service1>();
            container.Register<Service2>();
            container.Register<Service3>();


            using (var scope = container.OpenScope())
            {
                var s = scope.Resolve<IService>();
            }

            Assert.AreEqual("disposed", logger.Messages[0]);
        }

        class Service1 {}
        class Service2 {}
        class Service3 {}

        interface ILogger
        {
            void Log(string m);
        }
        class ListLogger : ILogger
        {
            public List<string> Messages;
            public void Log(string m) => Messages.Add(m);
        }
        interface IService {}
        class Service : IService, IDisposable
        {
            ILogger _logger;
            public Service(Func<object, ILogger> getLogger, Service1 s1, Service2 s2, Service3 s3) => _logger = getLogger(this);

            public void Dispose() => _logger.Log("disposed");
        }
    }
}