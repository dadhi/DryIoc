using System.Diagnostics.CodeAnalysis;
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
            Should_throw_for_unresolved_required_property();
            Should_skip_required_property_injection_when_using_ctor_with_SetsRequiredProperties();
            return 3;
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

        [Test]
        public void Should_throw_for_unresolved_required_property()
        {
            var c = new Container(Rules.Default.With(propertiesAndFields: PropertiesAndFields.RequiredProperties()));
            
            c.Register<BS>();
  
            var ex = Assert.Throws<ContainerException>(() => c.Resolve<BS>());
            Assert.AreEqual(Error.NameOf(Error.UnableToResolveUnknownService), ex.ErrorName);
        }

        [Test]
        public void Should_skip_required_property_injection_when_using_ctor_with_SetsRequiredProperties()
        {
            var c = new Container(Rules.Default.With(propertiesAndFields: PropertiesAndFields.RequiredProperties()));
            
            c.Register<SS>();
            
            c.Register<A>();
            c.Register<B>();
  
            var x = c.Resolve<SS>();

            Assert.Null(x.A);
            Assert.NotNull(x.B);
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

        public class SS
        {
            public required A A { get; init; }
            public B B { get; private set; }

            [SetsRequiredMembers]
            public SS(B b) => B = b;
        }
#endif
    }
}