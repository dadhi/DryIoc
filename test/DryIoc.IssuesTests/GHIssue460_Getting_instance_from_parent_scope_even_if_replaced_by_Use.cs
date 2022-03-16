using System;
using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue460_Getting_instance_from_parent_scope_even_if_replaced_by_Use : ITest
    {
        public int Run()
        {
            Should_use_data_from_the_scope_when_resolving_in_scope_with_asResolutionCall();
            Should_use_data_from_the_scope_when_resolving_in_scope_with_original_Use();
            Auto_dynamic_registration_for_the_concrete_types_should_work();
            Should_use_data_from_the_scope_when_resolving_in_scope();
            Should_use_data_from_the_scope_when_resolving_in_scope_without_the_first_resolve();
            return 4;
        }

        [Test]
        public void Should_use_data_from_the_scope_when_resolving_in_scope_with_asResolutionCall()
        {
            var container = new Container();
            container.RegisterInstance(new Data { Text = "parent" }, setup: Setup.With(asResolutionCall: true));
            container.Register<IDataDependent, DataDependent>();
            container.Register<DataDependentIndirectly>();

            // if this is removed, the test passes
            var outside = container.Resolve<DataDependentIndirectly>();

            using (var scope = container.OpenScope())
            {
                scope.Use(new Data { Text = "child" });
                var inScope = scope.Resolve<DataDependentIndirectly>();
                Assert.AreEqual("child", inScope.Dependent.Data1.Text);
            }
        }

        [Test]
        public void Should_use_data_from_the_scope_when_resolving_in_scope_with_original_Use()
        {
            var container = new Container();
            container.Use(new Data { Text = "parent" });
            container.Register<IDataDependent, DataDependent>();
            container.Register<DataDependentIndirectly>();

            // if this is removed, the test passes
            var outside = container.Resolve<DataDependentIndirectly>();

            using (var scope = container.OpenScope())
            {
                scope.Use(new Data { Text = "child" });
                var inScope = scope.Resolve<DataDependentIndirectly>();
                Assert.AreEqual("child", inScope.Dependent.Data1.Text);
            }
        }

        [Test]
        public void Auto_dynamic_registration_for_the_concrete_types_should_work()
        {
            var container = new Container(rules => rules
                .WithConcreteTypeDynamicRegistrations((type, key) => type != typeof(Data)));

            container.Use(new Data { Text = "whatever" });

            container.Register<IDataDependent, DataDependent>();
            container.Register<DataDependentIndirectly>();

            // if this is removed, the test passes
            var outside = container.Resolve<DataDependentIndirectly>();

            using (var scope = container.OpenScope())
            {
                scope.Use(new Data { Text = "child" });
                var inScope = scope.Resolve<DataDependentIndirectly>();
                Assert.AreEqual("child", inScope.Dependent.Data1.Text);
            }
        }

        [Test]
        public void Should_use_data_from_the_scope_when_resolving_in_scope()
        {
            var container = new Container(rules => rules.WithFuncAndLazyWithoutRegistration());

            container.RegisterInstance(new Data { Text = "parent" }, setup: Setup.With(asResolutionCall: true));
            container.Register<IDataDependent, DataDependent>();
            container.Register<DataDependentIndirectly2>();

            // now it fails even if this is removed
            var outside = container.Resolve<DataDependentIndirectly2>();

            using (var scope = container.OpenScope())
            {
                scope.Use(new Data { Text = "child" });
                var inScope = scope.Resolve<DataDependentIndirectly2>();
                Assert.AreEqual("child", inScope.Dependent.Data1.Text); // fails, the value is "parent"
            }
        }

        [Test]
        public void Should_use_data_from_the_scope_when_resolving_in_scope_without_the_first_resolve()
        {
            var container = new Container(rules => rules.WithFuncAndLazyWithoutRegistration());

            container.RegisterInstance(new Data { Text = "parent" }, setup: Setup.With(asResolutionCall: true));
            container.Register<IDataDependent, DataDependent>();
            container.Register<DataDependentIndirectly2>();

            using (var scope = container.OpenScope())
            {
                scope.Use(new Data { Text = "child" });
                var inScope = scope.Resolve<DataDependentIndirectly2>();
                Assert.AreEqual("child", inScope.Dependent.Data1.Text); // fails, the value is "parent"
            }
        }

        public class Data
        {
            public string Text { get; set; }
        }

        public interface IDataDependent
        {
            Data Data1 { get; }
        }

        public class DataDependent : IDataDependent
        {
            public Data Data1 { get; }

            public DataDependent(Data data)
            {
                Data1 = data;
            }
        }

        public class DataDependentIndirectly
        {
            public IDataDependent Dependent { get; }

            public DataDependentIndirectly(IDataDependent dataDependent)
            {
                Dependent = dataDependent;
            }
        }

        public class DataDependentIndirectly2
        {
            private Lazy<IDataDependent> _lazyDependent;

            public IDataDependent Dependent => _lazyDependent.Value;

            public DataDependentIndirectly2(Lazy<IDataDependent> dataDependent)
            {
                _lazyDependent = dataDependent;
            }
        }
    }
}