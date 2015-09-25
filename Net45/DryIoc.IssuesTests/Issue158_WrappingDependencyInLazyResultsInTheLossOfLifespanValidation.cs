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

            Assert.Throws<ContainerException>(() => 
                scope.Resolve<C>());
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
