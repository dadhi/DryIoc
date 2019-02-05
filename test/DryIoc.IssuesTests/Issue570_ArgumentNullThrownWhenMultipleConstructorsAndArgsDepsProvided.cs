using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue570_ArgumentNullThrownWhenMultipleConstructorsAndArgsDepsProvided
    {
        [Test]
        public void ResolveShouldNotThrowWhenMultipleConstructorsAndArgsDepsProvided()
        {
            var container = new Container(rules => rules
                .With(made: FactoryMethod.ConstructorWithResolvableArguments));

            container.Register<Service>();

            var config = new ServiceConfig();
            var service = container.Resolve<Service>(new object[] {config});

            Assert.AreSame(config, service.Config);
        }

        [Test]
        public void ResolveShouldNotThrowWhenMultipleConstructorsAndArgsDepsProvided_WithConcreteTypesResolution()
        {
            var container = new Container(rules => rules
                .WithAutoConcreteTypeResolution());
            
            var config = new ServiceConfig();

            var service = container.Resolve<Service>(new object[] { config });
            Assert.AreSame(config, service.Config);
        }

        [Test]
        public void AutoConcreteTypeResolution_should_be_able_to_create_with_default_ctor()
        {
            var container = new Container(rules => rules
                .WithAutoConcreteTypeResolution());

            var dict = container.Resolve<Dictionary<Type, object>>();

            Assert.IsNotNull(dict);
        }

        [Test]
        public void WithConcreteTypeDynamicRegistrations_should_be_able_to_create_with_default_ctor()
        {
            var container = new Container(rules => rules
                .WithConcreteTypeDynamicRegistrations());

            var dict = container.Resolve<Dictionary<Type, object>>();

            Assert.IsNotNull(dict);
        }

        [Test]
        public void Should_work_with_the_Dictionary_is_right()
        {
            var container = new Container(rules => rules
                .WithAutoConcreteTypeResolution()
                .With(made: FactoryMethod.ConstructorWithResolvableArguments));

            container.Register<Koa>();

            var koa = container.Resolve<Koa>();
            Assert.IsNotNull(koa);
            Assert.AreEqual(-1, koa.X);
        }

        public class Koa
        {
            public readonly int X = -1;
            public Koa() { }
            public Koa(int x) { X = x; }
            public Koa(Func<int> x) { X = x(); }
        }

        public class ServiceConfig
        {
        }

        public class Service
        {
            public Service()
            {
            }

            public Service(ServiceConfig config)
            {
                Config = config;
            }

            public ServiceConfig Config { get; }
        }
    }
}
