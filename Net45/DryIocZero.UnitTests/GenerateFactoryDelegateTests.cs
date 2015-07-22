using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using ExpressionToCodeLib.Unstable_v2_Api;
using DryIoc;
using DryIoc.MefAttributedModel;
using DryIoc.MefAttributedModel.UnitTests.CUT;


namespace DryIocZero.UnitTests
{
    [TestFixture]
    public class GenerateFactoryDelegateTests
    {
        [Test]
        public void Can_load_types_from_assembly_and_generate_some_resolutions()
        {
            var container = new DryIoc.Container(rules => rules
                .WithoutSingletonOptimization()
                .WithMefAttributedModel());

            var types = typeof(BirdFactory).GetAssembly().GetLoadedTypes();
            container.RegisterExports(types);

            var r = container.GetServiceRegistrations().FirstOrDefault(x => x.ServiceType == typeof(Chicken));
            var factoryExpr = container.Resolve<LambdaExpression>(r.OptionalServiceKey, DryIoc.IfUnresolved.Throw, r.ServiceType);

            Assert.DoesNotThrow(() => ExpressionStringify.With(true, true).ToCode(factoryExpr));
        }

        [Test]
        public void Generate_factory_delegate_for_exported_static_factory_method()
        {
            var container = new DryIoc.Container(rules => rules
                .WithoutSingletonOptimization()
                .WithMefAttributedModel());

            container.RegisterExports(typeof(BirdFactory));

            var r = container.GetServiceRegistrations().FirstOrDefault(x => x.ServiceType == typeof(Chicken));
            var factoryExpr = container.Resolve<LambdaExpression>(r.OptionalServiceKey, DryIoc.IfUnresolved.Throw, r.ServiceType);

            Assert.DoesNotThrow(() => ExpressionStringify.With(true, true).ToCode(factoryExpr));
        }
    }
}
