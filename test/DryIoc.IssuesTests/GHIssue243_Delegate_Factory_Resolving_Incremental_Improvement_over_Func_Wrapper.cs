using NUnit.Framework;
using System;
using DryIoc.FastExpressionCompiler.LightExpression;
using DryIoc.ImTools;

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

            container.RegisterDelegate<Func<Hey>, HeyFactory>(f => f.Invoke, Reuse.Scoped);
            // container.RegisterFunc(
            //     typeof(HeyFactory), typeof(Func<,>).MakeGenericType(typeof(Func<Hey>), typeof(object)),
            //     (Func<object, object>)(f => f.Invoke).ToFuncWithObjParams,
            //     Reuse.Scoped);

            container.Register<Hey>(Reuse.Scoped);

            using var scope = container.OpenScope();

            var hf = scope.Resolve<HeyFactory>();
            var f = scope.Resolve<Func<Hey>>();

            Assert.IsNotNull(hf());
            Assert.AreSame(hf(), f());
        }

        public class Hey { }

        public delegate Hey HeyFactory();
    }
}
