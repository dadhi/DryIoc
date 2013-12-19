using System;
using System.ComponentModel.Composition;
using System.Linq.Expressions;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class ExportFactoryTests
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
        public void Could_dynamically_register_IFactory_method_with_delegate_factory()
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

        [Test]
        public void Could_register_multi_factory_automatically_when_exported()
        {
            var container = new Container(AttributedModel.DefaultSetup);
            container.RegisterExports(typeof(FruitFactory));

            var orange = container.Resolve<Orange>();
            var apple = container.Resolve<Apple>();

            Assert.NotNull(orange);
            Assert.NotNull(apple);
        }

        [Test]
        public void Could_register_multi_factory_with_separate_named_Exports()
        {
            var container = new Container(AttributedModel.DefaultSetup);
            container.RegisterExports(typeof(NamedFruitFactory));

            var orange = container.Resolve<Orange>("orange");
            var apple = container.Resolve<Apple>("apple");

            Assert.NotNull(orange);
            Assert.NotNull(apple);
        }

        [Test]
        public void Reuse_should_be_applied_for_factory_created_services()
        {
            var container = new Container(AttributedModel.DefaultSetup);
            container.RegisterExports(typeof(OrangeFactory));

            var one = container.Resolve<Orange>();
            var another = container.Resolve<Orange>();

            Assert.That(one, Is.SameAs(another));
        }

        [Test]
        public void Specified_transient_reuse_should_be_applied_for_factory_created_services()
        {
            var container = new Container(AttributedModel.DefaultSetup);
            container.RegisterExports(typeof(TransientOrangeFactory));

            var one = container.Resolve<Orange>();
            var another = container.Resolve<Orange>();

            Assert.That(one, Is.Not.SameAs(another));
        }
    }

    [ExportAll]
    public class OrangeFactory : IFactory<Orange>
    {
        public Orange Create()
        {
            return new Orange();
        }
    }

    [ExportAll]
    public class FruitFactory : IFactory<Orange>, IFactory<Apple>
    {
        Orange IFactory<Orange>.Create()
        {
            return new Orange();
        }

        Apple IFactory<Apple>.Create()
        {
            return new Apple();
        }
    }

    [Export("orange", typeof(IFactory<Orange>))]
    [Export("apple", typeof(IFactory<Apple>))]
    public class NamedFruitFactory : IFactory<Orange>, IFactory<Apple>
    {
        [Export("orange")]
        public Orange Create()
        {
            return new Orange();
        }

        [Export("apple")]
        Apple IFactory<Apple>.Create()
        {
            return new Apple();
        }
    }

    [ExportAll]
    public class TransientOrangeFactory : IFactory<Orange>
    {
        [CreationPolicy(CreationPolicy.NonShared)]
        public Orange Create()
        {
            return new Orange();
        }
    }

    public class Orange {}
    public class Apple {}
}
