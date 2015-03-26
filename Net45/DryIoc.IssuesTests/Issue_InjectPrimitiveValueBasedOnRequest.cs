using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class ParameterResolutionFixture
    {
        private IContainer _subject;

        [SetUp]
        public void SetUp()
        {
            _subject = new Container();
        }

        [Test]
        public void Resolves_For_StringType()
        {
            // setup
            _subject.Register<Target>();
            _subject.Register<ILog, Log>(made: Made.Of(parameters: request => parameter =>
            {
                if (parameter.Name == "input" && parameter.ParameterType == typeof(string))
                {
                    var target = request.Parent.ImplementationType.FullName;

                    return ParameterServiceInfo.Of(parameter)
                        .WithDetails(ServiceInfoDetails.Of(defaultValue: target), request);
                }

                return null;
            }));

            // exercise
            var resolved = _subject.Resolve<Target>();

            // verify
            Assert.AreEqual("DryIoc.IssuesTests.Target", resolved.LogInput);
        }

        [Test, Ignore]
        public void Resolves_For_CollectionType()
        {
            // setup
            _subject.Register<Target>();
            _subject.Register<ILog, LogWrapper>(made: Made.Of(parameters: request => parameter =>
            {
                if (parameter.Name == "loggers" && parameter.ParameterType == typeof(ILog[]))
                {
                    var target = request.Parent.ImplementationType.FullName;
                    var logs = GetLogs(target);

                    CollectionAssert.IsNotEmpty(logs);

                    return ParameterServiceInfo.Of(parameter)
                        .WithDetails(ServiceInfoDetails.Of(defaultValue: logs), request);
                }

                return null;
            }));

            // exercise
            var resolved = _subject.Resolve<Target>();

            // verify
            Assert.AreEqual("1", resolved.LogInput);
        }

        public ILog[] GetLogs(string type)
        {
            var ret = new List<ILog>();

            ret.Add(new Log(type));

            return ret.ToArray();
        }

        [Test]
        public void Injects_parent_type_using_factory()
        {
            // setup
            _subject.Register<Target>();
            _subject.Register<ILog, Log>(
                setup: Setup.With(cacheFactoryExpression: false),
                made: Made.Of(request =>
                {
                    var targetType = request.ParentNonWrapper().Enumerate().First(p => p.ServiceType != typeof(ILog)).ImplementationType;
                    return FactoryMethod.Of(typeof(LoggerFactory).GetDeclaredMethodOrNull("GetLog").MakeGenericMethod(targetType));
                }));

            // exercise
            var resolved = _subject.Resolve<Target>();

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
