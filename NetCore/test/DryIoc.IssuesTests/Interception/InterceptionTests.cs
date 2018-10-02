using System;
using System.Linq;
using Castle.DynamicProxy;
using NUnit.Framework;

using DryIoc.Interception;

namespace DryIoc.IssuesTests.Interception
{
    [TestFixture]
    public class InterceptionTests
    {
        [Test]
        public void Test_interface_interception()
        {
            var c = new Container();

            c.Register<ICalculator1, Calculator1>();
            var result = string.Empty;
            c.Register<LoggerInterceptor>(made:
                Parameters.Of.Type<Action<IInvocation>>(_ => invocation =>
                    result = string.Join("+", invocation.Arguments.Select(x => x.ToString()))));

            c.Intercept<ICalculator1, LoggerInterceptor>();

            var calc = c.Resolve<ICalculator1>();
            calc.Add(1, 2);

            Assert.AreEqual("1+2", result);
        }

        [Test]
        public void Intercepting_disposable_transient()
        {
            var container = new Container();
            container.Register<ITestDisposable, TestDisposable>(Reuse.Singleton);
            container.Intercept<ITestDisposable, LoggerInterceptor>();

            string methodCallLog = null;
            container.Register<LoggerInterceptor>(
                made: Parameters.Of.Type<Action<IInvocation>>(_ =>
                    invocation => methodCallLog = invocation.Method.Name));

            container.Resolve<ITestDisposable>().Dispose();

            Assert.AreEqual("Dispose", methodCallLog);
        }
    }

    public sealed class LoggerInterceptor : IInterceptor
    {
        private readonly Action<IInvocation> _logAction;

        public LoggerInterceptor(Action<IInvocation> logAction)
        {
            _logAction = logAction;
        }

        public void Intercept(IInvocation invocation)
        {
            _logAction(invocation);
            invocation.Proceed();
        }
    }

    public interface ICalculator1
    {
        int Add(int first, int second);
    }

    public class Calculator1 : ICalculator1
    {
        public virtual int Add(int first, int second)
        {
            return first + second;
        }
    }

    public interface ITestDisposable : IDisposable
    {
    }

    public class TestDisposable : ITestDisposable
    {
        public void Dispose()
        {
        }
    }

}
