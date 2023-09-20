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

            // var serviceType = typeof(HeyFactory);
            // var funcType = typeof(Func<,>).MakeGenericType(typeof(Func<Hey>), typeof(object));

            // var method = Made.Of(FactoryMethod.OfFunc(funcType.GetMethod("Invoke"), (object f) => ((Func<Hey>)f).Invoke));

            // var factory = ReflectionFactory.OfTypeAndMadeNoValidation(serviceType, method, Reuse.Scoped);

            // container.Register(factory, serviceType, null, null, isStaticallyChecked: true);

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
