using System.Web.Http;
using Web.Rest.API;

using DryIoc;
using DryIoc.WebApi;
using FastExpressionCompiler.LightExpression;
using NUnit.Framework;

namespace LoadTest
{
    /*
     * Reproduces https://github.com/dadhi/DryIoc/issues/139
     */
    [TestFixture]
    public class ReducedLoadTest
    {
        [Test]
        public void Test_with_UseDecorateeReuse_decorators_Examine_expression_and_the_split_graph()
        {
            var container = new Container(rules => rules
                    .WithoutInterpretationForTheFirstResolution()
                    .WithUseDecorateeReuseForDecorators()
                    .With(FactoryMethod.ConstructorWithResolvableArguments))
                .WithWebApi(new HttpConfiguration());

            Registrations.RegisterTypes(container, false);

            using (var scope = container.OpenScope(Reuse.WebRequestScopeName))
            {
                var x = scope.Resolve<LambdaExpression>(typeof(EmailController));
                Assert.IsNotNull(x);
            }
        }

        [Test]
        public void Test_with_singleton_decorators()
        {
            var container = new Container(rules => rules
                .With(FactoryMethod.ConstructorWithResolvableArguments))
                .WithWebApi(new HttpConfiguration());

            Registrations.RegisterTypes(container, true);

            for (var i = 0; i < 10; i++)
                using (var scope = container.OpenScope(Reuse.WebRequestScopeName))
                    scope.Resolve(typeof(EmailController));
        }

        [Test]
        public void Test_with_transient_decorators()
        {
            var container = new Container(rules => rules
                .With(FactoryMethod.ConstructorWithResolvableArguments))
                .WithWebApi(new HttpConfiguration());

            Registrations.RegisterTypes(container, false);

            for (var i = 0; i < 10; i++)
                using (var scope = container.OpenScope(Reuse.WebRequestScopeName))
                    scope.Resolve(typeof(EmailController));
        }

        [Test]
        public void Test_with_UseDecorateeReuse_decorators()
        {
            var container = new Container(rules => rules
                .WithUseDecorateeReuseForDecorators()
                .With(FactoryMethod.ConstructorWithResolvableArguments))
                .WithWebApi(new HttpConfiguration());

            Registrations.RegisterTypes(container, false);

            for (var i = 0; i < 10; i++)
                using (var scope = container.OpenScope(Reuse.WebRequestScopeName))
                    scope.Resolve(typeof(EmailController));
        }
    }
}
