using System.Web.Http;
using DryIoc;
using DryIoc.WebApi;
using NUnit.Framework;
using Web.Rest.API;

namespace LoadTest
{
    /*
     * Reproduces https://github.com/dadhi/DryIoc/issues/139
     */
    [TestFixture]
    public class ReducedLoadTest
    {
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
