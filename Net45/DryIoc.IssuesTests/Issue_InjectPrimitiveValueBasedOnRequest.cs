using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class ParameterResolutionFixture
    {
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = new Container();
        }

        [Test]
        public void Resolves_For_StringType()
        {
            // setup
            _container.Register<Target>();
            _container.Register<ILog, Log>(
                made: Parameters.Of.Details((request, parameter) => 
                    parameter.Name == "input" && parameter.ParameterType == typeof(string)
                    ? ServiceDetails.Of(request.Parent.ImplementationType.FullName)
                    : null));

            // exercise
            var resolved = _container.Resolve<Target>();

            // verify
            Assert.AreEqual("DryIoc.IssuesTests.Target", resolved.LogInput);
        }

        [Test]
        public void Resolves_For_CollectionType()
        {
            // setup
            _container.Register<Target>();
            _container.Register<ILog, Log>(
                serviceKey: "normal",
                made: Parameters.Of.Type(request =>
                    request.Parent.First(p => p.ServiceType != typeof(ILog)).ImplementationType.FullName));
            
            _container.Register<ILog, LogWrapper>();

            var target = _container.Resolve<Target>();

            Assert.AreEqual("1", target.LogInput);
        }

        [Test]
        public void Injects_parent_type_using_factory()
        {
            // setup
            _container.Register<Target>();
            _container.Register<ILog, Log>(
                made: Made.Of(request => typeof(LoggerFactory)
                    .SingleMethod(nameof(LoggerFactory.GetLog))
                    .MakeGenericMethod(request.Parent.First(p => p.ServiceType != typeof(ILog)).ImplementationType)));

            // exercise
            var resolved = _container.Resolve<Target>();

            // verify
            Assert.AreEqual("DryIoc.IssuesTests.Target", resolved.LogInput);
        }

        public class LoggerFactory
        {
            public static ILog GetLog<TTarget>()
            {
                return new Log(typeof(TTarget).FullName);
            }
        }
    }

    public interface ILog
    {
        string Go();
    }

    public class LogWrapper : ILog
    {
        private readonly ILog[] _loggers;

        public LogWrapper(ILog[] loggers)
        {
            _loggers = loggers;
        }

        public string Go()
        {
            return _loggers.Count().ToString();
        }
    }

    public class Log : ILog
    {
        private readonly string _input;

        public Log(string input)
        {
            _input = input;
        }

        public string Go()
        {
            return _input;
        }
    }

    public class Target
    {
        private readonly ILog _log;

        public Target(ILog log)
        {
            _log = log;
        }

        public string LogInput
        {
            get
            {
                return _log.Go();
            }
        }
    }

}
