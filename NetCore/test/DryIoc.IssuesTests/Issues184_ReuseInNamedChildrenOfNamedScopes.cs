using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issues184_ReuseInNamedChildrenOfNamedScopes
    {
        [Test]
        public void Test()
        {
            var container = new Container(Rules.Default
                // What is the scopeName? How it relates to scopes below?
                //.WithDefaultReuseInsteadOfTransient(Reuse.InCurrentNamedScope(scopeName)) 
                .With(propertiesAndFields: PropertiesAndFields.Auto)
                .WithoutThrowOnRegisteringDisposableTransient());

            container.Register(typeof(IFoo), typeof(Foo), Reuse.InCurrentNamedScope("Parent"));
            container.Register(typeof(IBar), typeof(Bar), Reuse.InCurrentNamedScope("Child"));

            var parentScope = container.OpenScope("Parent");
            var childScope = parentScope.OpenScope("Child");

            var bar = childScope.Resolve<IBar>();
            Assert.IsNotNull(bar);
            Assert.IsNotNull(bar.Foo);

            var parentFoo = parentScope.Resolve<IFoo>();
            Assert.IsNotNull(parentFoo);
        }

        interface IFoo {}

        interface IBar
        {
            IFoo Foo { get; }
        }

        class Foo : IFoo {}

        class Bar : IBar
        {
            public IFoo Foo { get; set; }
        }
    }
}
