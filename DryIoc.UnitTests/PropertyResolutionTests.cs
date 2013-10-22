using System;
using System.ComponentModel.Composition;
using System.Reflection;
using DryIoc.AttributedRegistration;
using DryIoc.UnitTests.CUT;
using DryIoc.UnitTests.Playground;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class PropertyResolutionTests
    {
        [Test]
        public void Resolving_unregistered_property_should_NOT_throw_and_should_preserve_original_property_value()
        {
            var container = new Container();
            container.Register(typeof(PropertyHolder));
            var holder = container.Resolve<PropertyHolder>();
            var dependency = holder.Dependency = new Dependency();

            container.ResolvePropertiesAndFields(holder);

            Assert.That(holder.Dependency, Is.EqualTo(dependency));
        }

        [Test]
        public void Resolving_property_registered_in_container_should_succeed()
        {
            var container = new Container();
            container.Register(typeof(PropertyHolder));
            container.Register(typeof(IDependency), typeof(Dependency));
            var holder = container.Resolve<PropertyHolder>();

            container.ResolvePropertiesAndFields(holder);

            Assert.IsInstanceOf<Dependency>(holder.Dependency);
        }

        [Test]
        public void Resolving_field_registered_in_container_should_succeed()
        {
            var container = new Container();
            container.Register<FieldHolder>();
            container.Register<IDependency, Dependency>();
            var holder = container.Resolve<FieldHolder>();

            container.ResolvePropertiesAndFields(holder);

            Assert.IsInstanceOf<Dependency>(holder.Dependency);
        }

        [Test]
        public void Resolving_property_with_nonpublic_set_should_NOT_throw_and_should_preserve_original_property_value()
        {
            var container = new Container();
            container.Register(typeof(PropertyHolder));
            container.Register(typeof(IBar), typeof(Bar));
            var holder = container.Resolve<PropertyHolder>();

            container.ResolvePropertiesAndFields(holder);

            Assert.IsNull(holder.Bar);
        }

        [Test]
        public void Resolving_property_without_set_should_NOT_throw_and_should_preserve_original_property_value()
        {
            var container = new Container();
            container.Register(typeof(PropertyHolder));
            container.Register(typeof(IBar), typeof(Bar));
            var holder = container.Resolve<PropertyHolder>();

            container.ResolvePropertiesAndFields(holder);

            Assert.IsNull(holder.BarWithoutSet);
        }

        [Test]
        public void Resolving_readonly_field_should_NOT_throw_and_should_preserve_field_original_value()
        {
            var container = new Container();
            container.Register(typeof(FieldHolder));
            container.Register(typeof(IBar), typeof(Bar), Reuse.Singleton);
            var holder = container.Resolve<FieldHolder>();
            var bar = container.Resolve<IBar>();

            container.ResolvePropertiesAndFields(holder);

            Assert.That(holder.Bar, Is.Not.EqualTo(bar));
        }

        [Test]
        public void Can_resolve_property_marked_with_Import()
        {
            var container = new Container();
            container.Register<FunnyChicken>();
            container.Register<Guts>();
            container.Register<Brain>();

            container.ResolutionRules.PropertiesAndFields =
                container.ResolutionRules.PropertiesAndFields.Append(
                    (out object resultKey, MemberInfo propertyOrField, Request request, IRegistry _) =>
                    {
                        resultKey = null;
                        return propertyOrField.GetCustomAttributes(typeof(ImportAttribute), false).Length != 0;
                    });

            var chicken = container.Resolve<FunnyChicken>();

            Assert.That(chicken.SomeGuts, Is.Not.Null);
        }

        [Test]
        public void Can_resolve_field_marked_with_Import()
        {
            var container = new Container();
            container.Register<FunnyChicken>();
            container.Register<Guts>();
            container.Register<Brain>();

            container.ResolutionRules.PropertiesAndFields =
                container.ResolutionRules.PropertiesAndFields.Append(AttributedRegistrator.TryGetPropertyOrFieldServiceKey);

            var chicken = container.Resolve<FunnyChicken>();

            Assert.That(chicken.SomeBrain, Is.Not.Null);
        }

        [Test]
        public void Should_not_throw_on_resolving_readonly_field_marked_with_Import()
        {
            var container = new Container();
            container.Register<FunnyDuckling>();

            container.ResolutionRules.PropertiesAndFields =
                container.ResolutionRules.PropertiesAndFields.Append(AttributedRegistrator.TryGetPropertyOrFieldServiceKey);

            Assert.DoesNotThrow(() =>
                container.Resolve<FunnyDuckling>());
        }

        [Test]
        public void Can_resolve_Func_of_field_marked_with_Import()
        {
            var container = new Container();
            container.Register<FunkyChicken>();
            container.Register<Guts>();

            container.ResolutionRules.PropertiesAndFields =
                container.ResolutionRules.PropertiesAndFields.Append(AttributedRegistrator.TryGetPropertyOrFieldServiceKey);

            var chicken = container.Resolve<FunkyChicken>();

            Assert.That(chicken.SomeGuts, Is.Not.Null);
        }

        [Test]
        public void Can_resolve_named_Lazy_of_property_marked_with_Import()
        {
            var container = new Container();
            container.Register<LazyChicken>();
            container.Register<Guts>(named: "lazy-me");

            container.ResolutionRules.PropertiesAndFields =
                container.ResolutionRules.PropertiesAndFields.Append(AttributedRegistrator.TryGetPropertyOrFieldServiceKey);

            var chicken = container.Resolve<LazyChicken>();

            Assert.That(chicken.SomeGuts, Is.Not.Null);
        }
    }

    #region CUT

    public class PropertyHolder
    {
        public IDependency Dependency { get; set; }

        public IBar Bar { get; private set; }

        public IBar BarWithoutSet
        {
            get { return null; }
        }
    }

    public class FieldHolder
    {
        public IDependency Dependency;

        public readonly IBar Bar = new Bar();
    }

    public class FunnyChicken
    {
        [Import]
        public Guts SomeGuts { get; set; }

        [Import] public Brain SomeBrain;
    }

    public class FunnyDuckling
    {
        [Import] public readonly Brain Brains;
    }

    public class FunkyChicken
    {
        [Import] public Func<Guts> SomeGuts;
    }

    public class LazyChicken
    {
        [Import("lazy-me")]
        public Lazy<Guts> SomeGuts { get; set; }
    }

    public class Guts
    {
    }

    public class Brain
    {
    }

    #endregion
}
