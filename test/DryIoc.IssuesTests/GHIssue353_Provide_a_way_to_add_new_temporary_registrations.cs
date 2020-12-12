using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue353_Provide_a_way_to_add_new_temporary_registrations
    {
        [Test]
        public void Isolated_CreateFacade_without_disposing_the_parent()
        {
            var container = new Container(rules => rules.WithTrackingDisposableTransients());
            container.Register<IDep, D1>();
            container.Register<UnitOfWork>();

            IDep dep;

            // now I need a robust way to override IDep registration
            
            // Facade marks all new registrations with the key and sets the rule prefer them over the default one
            // Given that facade has the Registry, Singleton and Scopes (if present) cloned - detached from the source container,
            // you won't affect anything in the source container.
            using (var child = container.CreateFacade())
            {
                child.Register<IDep, D2>();
                using (var unit = child.Resolve<UnitOfWork>())
                {
                    // unit work                    
                    dep = unit.Dep;
                    Assert.IsInstanceOf<D2>(dep);
                }
            }

            // because the transient is tracked in a Singleton scope (without other Scoped present),
            // it will be disposed together with facades and their singletons
            Assert.IsTrue(dep.IsDisposed);
            
            // at this point I want only D2 to be disposed,
            // not instances from parent container nor parent it self should be affected    
        }

        interface IDep
        {
            bool IsDisposed { get; }
        }

        class D1 : IDep
        {
            public bool IsDisposed { get; private set; }
            public void Dispose() => IsDisposed = true;
        }

        class D2 : IDep, IDisposable
        {
            public bool IsDisposed { get; private set; }
            public void Dispose() => IsDisposed = true;
        }

        class UnitOfWork : IDisposable
        {
            public readonly IDep Dep;
            public UnitOfWork(IDep d) => Dep = d;
            public void Dispose() {}
        }
    }
}
