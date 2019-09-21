using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue179_MadeOf_Parameters_do_not_follow_Reuse_setting
    { 
        [Test]
        public void Test()
        {
            var container = new Container();
            container.Register<A>();
            container.Register<B>();

            container.Register(Made.Of(
                () => LogManager.GetLogger(Arg.Index<Type>(0)),
                request => request.Parent.ImplementationType));

            var b = container.Resolve<B>();

            Assert.AreNotSame(b.Log, b.A.Log);
        }

        public interface ILog
        {
        }

        public class Log : ILog
        {
            public readonly Type ContextType;
            public Log(Type contextType) => ContextType = contextType;
        }

        public class LogManager
        {
            public static ILog GetLogger(Type type) => new Log(type);
        }

        public class B
        {
            public ILog Log { get; }
            public A A { get; }

            public B(ILog log, A a)
            {
                Log = log;
                A = a;
            }
        }

        public class A
        {
            public ILog Log { get; }
            public A(ILog log) => Log = log;
        }
    }
}
