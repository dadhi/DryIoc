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

            container.Register(typeof(Orange), new ExpressionFactory((request, registry) =>
                Expression.Call(
                    request.ResolutionState.GetExpression(registry.Resolve(factoryType), factoryType), 
                    "Create", null)));

            var orange = container.Resolve<Orange>();

            Assert.NotNull(orange);
        }

        [Test]
        public void Could_register_factory_automatically_when_exported()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(OrangeFactory));

            var orange = container.Resolve<Orange>();

            Assert.NotNull(orange);
        }

        [Test]
        public void Could_register_multi_factory_automatically_when_exported()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(FruitFactory));

            var orange = container.Resolve<Orange>();
            var apple = container.Resolve<Apple>();

            Assert.NotNull(orange);
            Assert.NotNull(apple);
        }

        [Test]
        public void Could_register_multi_factory_with_separate_named_Exports()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(NamedFruitFactory));

            var orange = container.Resolve<Orange>("orange");
            var apple = container.Resolve<Apple>("apple");

            Assert.NotNull(orange);
            Assert.NotNull(apple);
        }

        [Test]
        public void Reuse_should_be_applied_for_factory_created_services()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(OrangeFactory));

            var one = container.Resolve<Orange>();
            var another = container.Resolve<Orange>();

            Assert.That(one, Is.SameAs(another));
        }

        [Test]
        public void Specified_transient_reuse_should_be_applied_for_factory_created_services()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(TransientOrangeFactory));

            var one = container.Resolve<Orange>();
            var another = container.Resolve<Orange>();

            Assert.That(one, Is.Not.SameAs(another));
        }

        [Test]
        public void Could_export_Func_with_parameters_as_Factory()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(FuncFactory));

            var factory = container.Resolve<Func<string, Orange>>();

            Assert.NotNull(factory("hey"));
        }

        [Test]
        public void You_should_attribute_Create_with_Export_to_register_Otherwise_only_factory_itself_will_be_registered()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(BareFactory));

            container.Resolve<IFactory<Apple>>();

            Assert.Throws<ContainerException>(() =>
                container.Resolve<Apple>());
        }
    }

    [ExportAll]
    public class BareFactory : IFactory<Apple>
    {
        public Apple Create()
        {
            return new Apple();
        }
    }

    [ExportAll]
    public class OrangeFactory : IFactory<Orange>
    {
        [Export]
        public Orange Create()
        {
            return new Orange();
        }
    }

    [ExportAll]
    public class FruitFactory : IFactory<Orange>, IFactory<Apple>
    {
        [Export]
        Orange IFactory<Orange>.Create()
        {
            return new Orange();
        }

        [Export]
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
        [Export, CreationPolicy(CreationPolicy.NonShared)]
        public Orange Create()
        {
            return new Orange();
        }
    }

    [ExportAll]
    public class FuncFactory : IFactory<Func<string, Orange>>
    {
        [Export]
        public Func<string, Orange> Create()
        {
            return s => new Orange();
        }
    }

    public class Orange { }
    public class Apple { }
}
