using System;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue230_CustomInitializerAttachedToLazilyResolvedDependencyIsCalledOncePerResolution
    {
        [Test]
        public void Custom_initializer_attached_to_lazily_resolved_dependency_is_called_once_per_resolution_not_once_per_construction()
        {
            var ctr = new Container(Rules.Default.With(
                propertiesAndFields: req => req.ImplementationType.GetProperties().Select(PropertyOrFieldServiceInfo.Of)));

            ctr.Register<A>(Reuse.Singleton);
            ctr.Register<B>(Reuse.Singleton);

            var initializedTimes = 0;
            ctr.RegisterInitializer<A>((a, r) => ++initializedTimes);

            var b = ctr.Resolve<B>();
            b.A();
            b.A();
            b.A();

            Assert.AreEqual(1, initializedTimes);
        }

        public class A
        {
        }

        public class B
        {
            public Func<A> A { get; set; }
        }
    }
}
