using System;
using System.ComponentModel.Composition;
using System.Linq.Expressions;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class DelegateFactoryTests
    {
        [Test]
        public void Could_use_factory_interface_for_delegate_factory_registration()
        {
            var container = new Container();

            container.Register<IFactory<Orange>, OrangeFactory>();
            container.RegisterDelegate(r => r.Resolve<IFactory<Orange>>().Create());

            var orange = container.Resolve<Orange>();

            Assert.NotNull(orange);
        }

        [Test]
        public void Could_use_factory2_interface_for_delegate_factory_registration()
        {
            var container = new Container();

            container.Register<IFactory<Orange>, OrangeFactory>();

            var factoryType = typeof(IFactory<Orange>);

            Func<Request, IRegistry, Expression> getExpression = (_, registry) => 
                Expression.Call(registry.GetConstantExpression(registry.Resolve(factoryType), factoryType), "Create", null);

            container.Register(typeof(Orange), new DelegateFactory(getExpression));

            var orange = container.Resolve<Orange>();

            Assert.NotNull(orange);
        }

        [Test]
        public void Could_register_factory_automatically_when_exported()
        {
            var container = new Container(AttributedModel.DefaultSetup);
            container.RegisterExports(typeof(OrangeFactory));

            var orange = container.Resolve<Orange>();

            Assert.NotNull(orange);
        }
    }

    [ExportAll]
    class OrangeFactory : IFactory<Orange>
    {
        public Orange Create()
        {
            return new Orange();
        }
    }

    class Orange {}
}
