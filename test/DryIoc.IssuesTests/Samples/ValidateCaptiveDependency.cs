using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests.Samples
{
    [TestFixture]
    public class ValidateCaptiveDependency
    {
        [Test]
        public void  Scoped_in_a_Singleton_should_be_reported()
        {
            var c = new Container();
            c.Register<Foo>(Reuse.Scoped);
            c.Register<Bar>(Reuse.Singleton);
            c.Register<Buz>(Reuse.Scoped);

            var errors = c.Validate(ServiceInfo.Of<Foo>());

            Assert.AreEqual(1, errors.Length);
            var error = errors[0].Value;
            Assert.AreEqual(Error.NameOf(Error.DependencyHasShorterReuseLifespan), error.ErrorName);

            /* Exception message:
            """
            code: DependencyHasShorterReuseLifespan; 
            message: Dependency Buz as parameter "buz" (IsSingletonOrDependencyOfSingleton) with reuse Scoped {Lifespan=100} has shorter lifespan than its parent's Singleton Bar as parameter "bar" FactoryId=145 (IsSingletonOrDependencyOfSingleton)
              in Resolution root Scoped Foo FactoryId=144
              from container without scope
             with Rules with {UsedForValidation} and without {ImplicitCheckForReuseMatchingScope, EagerCachingSingletonForFasterAccess} with TotalDependencyCountInLambdaToSplitBigObjectGraph=2147483647
            If you know what you're doing you may disable this error with the rule `new Container(rules => rules.WithoutThrowIfDependencyHasShorterReuseLifespan())`.
            """
            */
        }

        public class Foo
        {
            public Foo(Bar bar) {}
        }
        
        public class Bar
        {
            public Bar(Buz buz) {}
        }

        public class Buz
        {
        }
    }
}
