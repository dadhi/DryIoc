using System;
using System.Linq;
using System.Text;
using System.Web.Http;
using Web.Rest.API;

using DryIoc;
using DryIoc.WebApi;
using DryIoc.FastExpressionCompiler.LightExpression;
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
                .WithoutInterpretationForTheFirstResolution() // compile on the first iteration
                .WithUseDecorateeReuseForDecorators()
                .With(FactoryMethod.ConstructorWithResolvableArguments))
                .WithWebApi(new HttpConfiguration());

            Registrations.RegisterTypes(container, false);

            using (var scope = container.OpenScope(Reuse.WebRequestScopeName))
            {
                var service = scope.Resolve(typeof(EmailController));
                Assert.IsNotNull(service);

                var expr = scope.Resolve<LambdaExpression>(typeof(EmailController));
                Assert.IsNotNull(expr);

                var code = expr.ToCSharpString(new StringBuilder(100000), 2, true, Abbreviate).ToString();
                var nestedLambdas = code.Count(c => c == '$');

                // the number when split by `dependencyCount >= 256`
                Assert.AreEqual(67, nestedLambdas);
            }
        }

        [Test]
        public void Test_with_UseDecorateeReuse_decorators_Examine_expression_and_the_split_graph_without_FEC()
        {
            var container = new Container(rules => rules
                .WithoutFastExpressionCompiler()
                .WithoutInterpretationForTheFirstResolution() // compile on the first iteration
                .WithUseDecorateeReuseForDecorators()
                .With(FactoryMethod.ConstructorWithResolvableArguments))
                .WithWebApi(new HttpConfiguration());

            Registrations.RegisterTypes(container, false);

            using (var scope = container.OpenScope(Reuse.WebRequestScopeName))
            {
                var service = scope.Resolve(typeof(EmailController));
                Assert.IsNotNull(service);

                var expr = scope.Resolve<LambdaExpression>(typeof(EmailController));
                Assert.IsNotNull(expr);

                var code = expr.ToCSharpString(new StringBuilder(100000), 2, true, Abbreviate).ToString();
                var nestedLambdas = code.Count(c => c == '$');
                Assert.AreEqual(2, nestedLambdas);

                StringAssert.Contains(".Resolve", code);
            }
        }

        private static string Abbreviate(Type t, string s)
        {
            if (t.Namespace == "DryIoc" || t.Namespace.StartsWith("System") ||
                s.EndsWith("Controller") || s.EndsWith("Decorator"))
                return s;

            var abbr = string.Empty;
            foreach (var c in s)
            {
                if (char.IsUpper(c))
                    abbr += c;
            }

            return abbr;
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
