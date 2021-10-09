using System;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace DryIoc.Microsoft.DependencyInjection.Specification.Tests
{
    [TestFixture]
    public class GHIssue429_Memory_leak_on_MS_DI_with_Disposable_Transient
    {
        [Test]
        public void Test1()
        {
            using (var container = new Container()
                .WithDependencyInjectionAdapter(new ServiceCollection())
                .With(rules => rules.WithoutTrackingDisposableTransients()))
            {
                container.Register<DisposableViewModel>(Reuse.Transient);

                ResolveManyTimes(container, typeof(DisposableViewModel));
            }
        }

        private static void ResolveManyTimes(IResolver container, Type typeToResolve)
        {
            for (var i = 0; i < 100; i++)
            {
                container.Resolve(typeToResolve);
            }
        }

        internal class NotDisposableViewModel
        {
        }

        internal class DisposableViewModel : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
