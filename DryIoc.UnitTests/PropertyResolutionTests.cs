using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DryIoc.UnitTests.CUT;
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
            var container = new Container(ResolutionRules.Default
                .WithPropertyAndFieldSelector((type, req, reg) =>
                     type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        .Where(p => p.GetSetMethod() != null).Cast<MemberInfo>().Concat(
                     type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        .Where(f => !f.IsInitOnly).Cast<MemberInfo>())
                    .Select(m =>
                    {
                        var attributes = m.GetCustomAttributes(typeof(ImportAttribute), false);
                        if (attributes.Length == 0)
                            return null;
                        var importAttr = (ImportAttribute)attributes[0];
                        return ServiceInfo.Of(m).ApplyCustom(importAttr.ContractType, importAttr.ContractName);
                    })));

            container.Register<FunnyChicken>();
            container.Register<Guts>();
            container.Register<Brain>();

            var chicken = container.Resolve<FunnyChicken>();

            Assert.That(chicken.SomeGuts, Is.Not.Null);
        }

        [Test]
        public void Can_resolve_field_marked_with_Import()
        {
            var container = new Container(ResolutionRules.Default.WithPropertyAndFieldSelector(SelectPropertiesAndFieldsWithImportAttribute));

            container.Register<FunnyChicken>();
            container.Register<Guts>();
            container.Register<Brain>();

            var chicken = container.Resolve<FunnyChicken>();

            Assert.That(chicken.SomeBrain, Is.Not.Null);
        }

        [Test]
        public void Should_not_throw_on_resolving_readonly_field_marked_with_Import()
        {
            var container = new Container(ResolutionRules.Default.WithPropertyAndFieldSelector(SelectPropertiesAndFieldsWithImportAttribute));

            container.Register<FunnyDuckling>();

            Assert.DoesNotThrow(() =>
                container.Resolve<FunnyDuckling>());
        }

        [Test]
        public void Can_resolve_Func_of_field_marked_with_Import()
        {
            var container = new Container(ResolutionRules.Default.WithPropertyAndFieldSelector(SelectPropertiesAndFieldsWithImportAttribute));

            container.Register<FunkyChicken>();
            container.Register<Guts>();

            var chicken = container.Resolve<FunkyChicken>();

            Assert.That(chicken.SomeGuts, Is.Not.Null);
        }

        [Test]
        public void Can_resolve_named_Lazy_of_property_marked_with_Import()
        {
            var container = new Container(ResolutionRules.Default.WithPropertyAndFieldSelector(SelectPropertiesAndFieldsWithImportAttribute));

            container.Register<LazyChicken>();
            container.Register<Guts>(named: "lazy-me");

            var chicken = container.Resolve<LazyChicken>();

            Assert.That(chicken.SomeGuts, Is.Not.Null);
        }

        private static IEnumerable<ServiceInfo> SelectPropertiesAndFieldsWithImportAttribute(Type type, Request req, IRegistry reg)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            
            var properties = type.GetProperties(flags).Where(p => p.GetSetMethod() != null);
            var fields = type.GetFields(flags).Where(f => !f.IsInitOnly);
            var members = properties.Cast<MemberInfo>().Concat(fields.Cast<MemberInfo>());
            
            return members.Select(m =>
            {
                var import = GetSingleAttributeOrDefault<ImportAttribute>(m.GetCustomAttributes(false));
                return import == null ? null : ServiceInfo.Of(m).ApplyCustom(import.ContractType, import.ContractName);
            });
        }

        private static TAttribute GetSingleAttributeOrDefault<TAttribute>(object[] attributes) where TAttribute : Attribute
        {
            TAttribute attr = null;
            for (var i = 0; i < attributes.Length && attr == null; i++)
                attr = attributes[i] as TAttribute;
            return attr;
        }
    }

    #region CUT

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class ImportAttribute : Attribute
    {
        public ImportAttribute() { }

        public ImportAttribute(string contractName, Type contractType = null)
        {
            ContractName = contractName;
            ContractType = contractType;
        }

        public Type ContractType { get; set; }
        public string ContractName { get; set; }
    }

    public class PropertyHolder
    {
        public IDependency Dependency { get; set; }

// ReSharper disable UnusedAutoPropertyAccessor.Local
        public IBar Bar { get; private set; }
// ReSharper restore UnusedAutoPropertyAccessor.Local

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

        [Import]
        public Brain SomeBrain;
    }

    public class FunnyDuckling
    {
        [Import]
        public readonly Brain Brains;

        public FunnyDuckling()
        {
            Brains = null;
        }
    }

    public class FunkyChicken
    {
        [Import]
        public Func<Guts> SomeGuts;
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
