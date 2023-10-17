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
            // Register_proxy_to_resolve_custom_delegate_for_Func_no_args();
            return 1;
        }

        // [Test] // todo: @wip
        public void Register_proxy_to_resolve_custom_delegate_for_Func_no_args()
        {
            var container = new Container();

            // container.RegisterDelegate<Func<Hey>, HeyFactory>(f => f.Invoke);

            var serviceType = typeof(HeyFactory);
            var funcType = typeof(Func<,>).MakeGenericType(typeof(Func<Hey>), typeof(object));

            var method = Made.Of(FactoryMethod.OfFunc(funcType.GetMethod("Invoke"), (object f) => f));

            var factory = ReflectionFactory.OfTypeAndMadeNoValidation(serviceType, method);

            container.Register(factory, serviceType, null, null, isStaticallyChecked: true);

            container.Register<Hey>();

            var hf = container.Resolve<HeyFactory>();
            var f = container.Resolve<Func<Hey>>();

            Assert.IsInstanceOf<Hey>(f());
            Assert.IsInstanceOf<Hey>(hf());

            var hfExpr = container.Resolve<LambdaExpression, HeyFactory>();
            var hf2 = container.Resolve<HeyFactory>(); // compiled factory
            Assert.IsInstanceOf<Hey>(hf());
        }

        public class Hey { }

        public delegate Hey HeyFactory();
    }
}
