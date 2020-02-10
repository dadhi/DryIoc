using System;
using System.Collections.Generic;
using Autofac.Core;
using ImTools;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue215_RegisterInitializer_causes_additional_call_to_Dispose_when_container_is_disposed
    {
        [Test]
        public void Should_call_dispose_once()
        {
            IService1 service1;
            IService2 service2;

            using (var container = new Container())
            {
                container.Register<IService1, Disposable1>(reuse: Reuse.Singleton);
                container.Register<IService2, Disposable2>(reuse: Reuse.Transient, setup: Setup.With(trackDisposableTransient: true));

                var disposables = new List<Type>();
                var service2Initialized = false; 
                container.RegisterInitializer<IDisposable>((service, _) => disposables.Add(service.GetType()));
                container.RegisterInitializer<IService2>((service, _) => service2Initialized = true);

                service1 = container.Resolve<IService1>();
                service2 = container.Resolve<IService2>();

                Assert.IsTrue(service2Initialized);
                CollectionAssert.AreEquivalent(new[] { typeof(Disposable1), typeof(Disposable2)}, disposables);
            }

            Assert.AreEqual(1, service1.To<Disposable>().DisposedCount);
            Assert.AreEqual(1, service2.To<Disposable>().DisposedCount);
        }

        public interface IService1 : IDisposable { }
        public interface IService2 : IDisposable { }

        public abstract class Disposable
        {
            public int DisposedCount { get; private set; }

            public void Dispose() => ++DisposedCount;

            public override string ToString() => 
                GetType().Name + " disposed " + DisposedCount + " times";
        }

        public class Disposable1 : Disposable, IService1 { }
        public class Disposable2 : Disposable, IService2 { }
    }
}
