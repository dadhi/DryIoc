using System;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace DryIoc.Microsoft.DependencyInjection.Specification.Tests
{
    [TestFixture]
    public class GHIssue429_Memory_leak_on_MS_DI_with_Disposable_Transient : ITest
    {
        public int Run()
        {
            Test_with_MS_DI_rules();
            Test_without_disposable_transient();
            return 2;
        }

        [Test]
        public void Test_with_MS_DI_rules()
        {
            DisposableViewModel[] xs = null;
            using (var container = new Container()
                .WithDependencyInjectionAdapter(new ServiceCollection())
                .Container)
            {
                container.Register<DisposableViewModel>(Reuse.Transient);

                xs = ResolveManyTimes<DisposableViewModel>(container);
            }

            foreach (var x in xs)
                Assert.IsTrue(x.IsDisposed);
        }

        [Test]
        public void Test_without_disposable_transient()
        {
            DisposableViewModel[] xs = null;
            using (var container = new Container()
                .WithDependencyInjectionAdapter(new ServiceCollection())
                .Container
                .With(rules => rules.WithoutTrackingDisposableTransients()))
            {
                container.Register<DisposableViewModel>(Reuse.Transient);

                xs = ResolveManyTimes<DisposableViewModel>(container);
            }

            foreach (var x in xs)
                Assert.IsFalse(x.IsDisposed);
        }

        private static T[] ResolveManyTimes<T>(IResolver container, int times = 10)
        {
            var xs = new T[times];
            for (var i = 0; i < times; i++)
            {
                xs[i] = container.Resolve<T>();
            }

            return xs;
        }

        internal class DisposableViewModel : IDisposable
        {
            public bool IsDisposed;
            public void Dispose() => IsDisposed = true;
        }
    }
}
