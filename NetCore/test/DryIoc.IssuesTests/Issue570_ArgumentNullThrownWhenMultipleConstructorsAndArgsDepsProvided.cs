using System;
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
                .WithAutoConcreteTypeResolution()
                .With(made: FactoryMethod.ConstructorWithResolvableArguments));
            
            var config = new ServiceConfig();

            var service = container.Resolve<Service>(new object[] { config });
            Assert.AreSame(config, service.Config);
        }

        [Test]
        public void AutoConcreteTypeResolution_should_ignore_primitive_types_And_in_a_wrapper_too()
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
