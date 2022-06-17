using DryIoc.FastExpressionCompiler.LightExpression;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue495_Automatically_generate_Resolution_calls_for_the_missing_registrations_to_avoid_manual_RegisterPlaceholder : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            var c = new Container(Rules.Default.WithGenerateResolutionCallForMissingDependency());

            c.Register<A>();
            c.Register<C>();

            var expr = c.Resolve<LambdaExpression, A>();

            var cs = expr.ToCSharpString();

            StringAssert.Contains("r.Resolve(", cs);
        }

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

        public class B { }
        public class C { }
    }
}
