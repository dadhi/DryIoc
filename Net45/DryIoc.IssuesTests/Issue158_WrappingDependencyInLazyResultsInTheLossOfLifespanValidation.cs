using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue158_WrappingDependencyInLazyResultsInTheLossOfLifespanValidation
    {
        [Test]
        public void Test()
        {
            var c = new Container();
            c.Register<C>(Reuse.Singleton);
            c.Register<D>(Reuse.InCurrentScope);

            var scope = c.OpenScope();

            var ex = Assert.Throws<ContainerException>(() => 
                scope.Resolve<C>());

            Assert.AreEqual(
                Error.NameOf(Error.DependencyHasShorterReuseLifespan), 
                Error.NameOf(ex.Error));
        }

        class C 
        { 
            public C(Lazy<D> d)
            {
                D = d.Value;
            }

            public D D { get; set; }
        }
        class D { }
    }
}
