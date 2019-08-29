using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue171_Wrong_IContainer_resolved
    {
        [Test]
        public void Should_resolve_correct_registered_container_with_RegisterDelegate()
        {
            var mainContainer = new Container();
            var configureContainer = new Container();

            configureContainer.RegisterDelegate<IContainer>(r => mainContainer, ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            configureContainer.Register<Module>();

            // init code ...
            configureContainer.Resolve<Module>().Register();

            Assert.False(configureContainer.IsRegistered<IFoo>());
            Assert.True(mainContainer.IsRegistered<IFoo>());
            Assert.AreSame(configureContainer.Resolve<IContainer>(), mainContainer);
        }

        [Test]
        public void Should_resolve_correct_registered_container_with_Use()
        {
            var mainContainer = new Container();
            var configureContainer = new Container();

            configureContainer.Use<IContainer>(mainContainer);

            configureContainer.Register<Module>();

            // init code ...
            configureContainer.Resolve<Module>().Register();

            Assert.False(configureContainer.IsRegistered<IFoo>());
            Assert.True(mainContainer.IsRegistered<IFoo>());
            Assert.AreSame(configureContainer.Resolve<IContainer>(), mainContainer);
        }

        class Module
        {
            private readonly IContainer _container;
            public Module(IContainer container) => 
                _container = container;

            public void Register() => 
                _container.Register<IFoo, Foo>();
        }

        public interface IFoo
        {
        }

        public class Foo : IFoo
        {
        }
    }
}
