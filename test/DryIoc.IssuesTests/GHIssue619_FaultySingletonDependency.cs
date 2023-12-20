using DryIoc.MefAttributedModel;
using NUnit.Framework;
using System;
using System.ComponentModel.Composition;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue619_FaultySingletonDependency
    {
        [Test]
        public void Test()
        {
            // default MEF reuse is a singleton
            var container = new Container().WithMef();
            container.RegisterExports(typeof(Dependency), typeof(ServiceWithLazyImport), typeof(ServiceWithNormalImport));

            // dependency initialization failed (test passes)
            var s1 = container.Resolve<ServiceWithLazyImport>();
            Assert.That(s1.DoWork, Throws.TypeOf<InvalidOperationException>());

            // should it fail or should it work? (test fails)
            var s2 = container.Resolve<ServiceWithNormalImport>();
            s2.DoWork();
            //or Assert.That(s2.DoWork, Throws.TypeOf<InvalidOperationException>());?

            // should it fail or should it work?
            var s3 = container.Resolve<ServiceWithLazyImport>();
            s3.DoWork();
            //or Assert.That(s3.DoWork, Throws.TypeOf<InvalidOperationException>());?
        }

        [Export, PartCreationPolicy(CreationPolicy.NonShared)]
        public class ServiceWithLazyImport
        {
            [Import]
            private Lazy<IDependency> LazyDependency { get; set; }

            public void DoWork() => LazyDependency.Value.DoWork();
        }

        [Export, PartCreationPolicy(CreationPolicy.NonShared)]
        public class ServiceWithNormalImport
        {
            [Import]
            private IDependency Dependency { get; set; }

            public void DoWork() => Dependency.DoWork();
        }

        public interface IDependency
        {
            void DoWork();
        }

        [Export(typeof(IDependency))]
        public class Dependency : IDependency
        {
            static bool firstTime = true;

            public Dependency()
            {
                if (firstTime)
                {
                    firstTime = false;
                    throw new InvalidOperationException("The first initialization failed " +
                        "due to a temporary problem, e.g. database connection timeout.");
                }
            }

            public void DoWork()
            { 
            }
        }
    }
}
