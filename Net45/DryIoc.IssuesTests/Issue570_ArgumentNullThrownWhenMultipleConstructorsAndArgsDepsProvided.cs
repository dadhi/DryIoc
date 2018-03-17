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
