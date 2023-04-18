using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class RequiredPropertiesTests : ITest
    {
#if !NET7_0_OR_GREATER
        public int Run() => 0;
#else
        public int Run()
        {
            Can_inject_required_properties();
            return 1;
        }

        [Test]
        public void Can_inject_required_properties()
        {
            var c = new Container(Rules.Default.With(propertiesAndFields: PropertiesAndFields.RequiredProperties()));
            
            c.Register<S>();
            
            c.Register<A>();
            c.Register<B>();
            c.Register<C>();
            c.Register<D>();

            var x = c.Resolve<S>();

            Assert.NotNull(x.A);
            Assert.NotNull(x.B);
            Assert.NotNull(x.C);
            Assert.NotNull(x.DD);
        }

        public class A {}
        public class B {}
        public class C {}
        public class D {}

        public class S : BS
        {
            public required A A { get; set; }
            public required B B { get; init; }
            public required C C { internal get; init; }
            public D DD => D;
        }

        public class BS
        {
            public required D D { protected get; set; }
        }
#endif
    }
}