using NUnit.Framework;
using System;
using DryIoc.FastExpressionCompiler.LightExpression;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue243_Delegate_Factory_Resolving_Incremental_Improvement_over_Func_Wrapper : ITest
    {
        public int Run()
        {
            Register_proxy_to_resolve_custom_delegate_for_Func_no_args();
            return 1;
        }

        [Test]
        public void Register_proxy_to_resolve_custom_delegate_for_Func_no_args()
        {
            var container = new Container();

            // container.Register(
            //     typeof(object),
            //     new ExpressionFactory(
            //         r =>
            //         {
            //             return null;
            //         },
            //         Reuse.Transient,
            //         Setup.DecoratorWith(condition: r => 
            //         {
            //             return r.ServiceType.IsAssignableTo<Delegate>();
            //         })
            //     ));

            container.Register<Hey>(Reuse.Scoped);

            using var scope = container.OpenScope();

            // var hf = scope.Resolve<HeyFactory>();
            var f = scope.Resolve<Func<Hey>>();

            // Assert.IsNotNull(hf());
            // Assert.AreSame(hf(), f());
        }

        public class Hey { }

        public delegate Hey HeyFactory();
    }
}
