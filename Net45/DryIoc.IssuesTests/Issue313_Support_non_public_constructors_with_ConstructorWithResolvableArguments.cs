using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue313_Support_non_public_constructors_with_ConstructorWithResolvableArguments
    {
        public class A { internal A() { } }
        public class B { internal B(A a) { } }

        [Test]
        public void Test()
        {
            var c = new Container();
            c.RegisterMany(new[] {typeof(A), typeof(B)},
                made: FactoryMethod.ConstructorWithResolvableArgumentsIncludingNonPublic);

            c.Resolve<B>();
        }
    }
}
