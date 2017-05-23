using System;
using System.Reflection;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue407_Cannot_resolve_MadeOf_params_explicitly
    {
        [Test]
        public void Test()
        {
            var container = new Container(rules => rules
                .WithAutoConcreteTypeResolution()
                .With(FactoryMethod.ConstructorWithResolvableArguments))
                .WithAutoFallbackDynamicRegistrations(new[] { Assembly.GetExecutingAssembly() });

            container.Register<Log>(
                Made.Of(_ => ServiceInfo.Of<LogService>(),
                    ls => ls.Logger(Arg.Index<Type>(0)),
                    r => r.Parent.ImplementationType));

            container.Resolve<TestClass>();
            container.Resolve<Log>();
        }

        class LogService
        {
            public Log Logger(Type type = null)
            {
                return new ALog(type ?? typeof(LogService));
            }
        }

        public abstract class Log
        {
        }

        public class ALog : Log
        {
            public Type Type { get; }

            public ALog(Type type)
            {
                Type = type;
            }
        }

        class TestClass
        {
            public Log Log { get; }

            public TestClass(Log log)
            {
                Log = log;
            }
        }
    }
}
