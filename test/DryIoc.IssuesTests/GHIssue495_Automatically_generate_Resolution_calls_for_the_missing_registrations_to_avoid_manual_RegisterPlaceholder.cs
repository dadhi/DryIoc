using DryIoc.FastExpressionCompiler.LightExpression;
using NUnit.Framework;
using System;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue495_Automatically_generate_Resolution_calls_for_the_missing_registrations_to_avoid_manual_RegisterPlaceholder : ITest
    {
        public int Run()
        {
            Missing_dependency_test();
            Missing_Func_dependency_test();
            return 2;
        }

        [Test]
        public void Missing_dependency_test()
        {
            var c = new Container(Rules.Default.WithGenerateResolutionCallForMissingDependency());

            c.Register<A>();
            c.Register<C>();

            var expr = c.Resolve<LambdaExpression, A>();

            var cs = expr.ToCSharpString();

            StringAssert.Contains("r.Resolve(", cs);
        }

        [Test]
        public void Missing_Func_dependency_test()
        {
            var container = new Container(Rules.Default.WithGenerateResolutionCallForMissingDependency());

            container.Register<F>();
            container.Register<B>();

            var f = container.Resolve<F>();
            Assert.IsNotNull(f);
            Assert.IsNotNull(f.Fc);
            Assert.Throws<ContainerException>(() => f.Fc());

            container.Register<C>();

            var c = f.Fc();
            Assert.IsNotNull(c);
        }

        public class B { }
        public class C { }

        public class A
        {
            public readonly B B;
            public readonly C C;
            public A(B b, C c)
            {
                B = b;
                C = c;
            }
        }

        public class F
        {
            public readonly B B;
            public readonly Func<C> Fc;
            public F(B b, Func<C> fc)
            {
                B = b;
                Fc = fc;
            }
        }
    }
}
