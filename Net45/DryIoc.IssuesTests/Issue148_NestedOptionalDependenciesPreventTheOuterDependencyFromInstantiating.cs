using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue148_NestedOptionalDependenciesPreventTheOuterDependencyFromInstantiating
    {
        [Test]
        public void Should_use_optional_parameter_if_registered_in_nested_dependency()
        {
            var c = new Container();
            c.Register<F>(reuse: Reuse.Transient);
            c.Register<D>(reuse: Reuse.Transient);

            var x = c.Resolve<F>(); // x.D == null

            Assert.IsNotNull(x.D);
        }

        class F
        {
            public D D;
            public F(D d = null) { D = d; }
        }
        class D
        {
            public D(E e = null) { }
        }
        class E
        {
            public E() { }
        }
    }
}
