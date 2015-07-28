using System.Linq;
using System.Linq.Expressions;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using ExpressionToCodeLib.Unstable_v2_Api;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class GenerateFactoryDelegateTests
    {
        [Test]
        public void Can_load_types_from_assembly_and_generate_some_resolutions()
        {
            var container = new Container(rules => rules
                .WithoutSingletonOptimization()
                .WithMefAttributedModel());

            var types = typeof(BirdFactory).GetAssembly().GetLoadedTypes();
            container.RegisterExports(types);

            var r = container.GetServiceRegistrations().FirstOrDefault(x => x.ServiceType == typeof(Chicken));
            var factoryExpr = container.Resolve<LambdaExpression>(r.OptionalServiceKey, IfUnresolved.Throw, r.ServiceType);

            Assert.DoesNotThrow(() => ExpressionStringify.With(true, true).ToCode(factoryExpr));
        }

        [Test]
        public void Generate_factory_delegate_for_exported_static_factory_method()
        {
            var container = new Container(rules => rules
                .WithoutSingletonOptimization()
                .WithMefAttributedModel());

            container.RegisterExports(typeof(BirdFactory));

            var r = container.GetServiceRegistrations().FirstOrDefault(x => x.ServiceType == typeof(Chicken));
            var factoryExpr = container.Resolve<LambdaExpression>(r.OptionalServiceKey, IfUnresolved.Throw, r.ServiceType);

            Assert.DoesNotThrow(() => ExpressionStringify.With(true, true).ToCode(factoryExpr));
        }
    }
}
