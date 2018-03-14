using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue570_ArgumentNullThrownWhenMultipleConstructorsAndArgsDepsProvided
    {
        [Test]
        public void ResolveSouldNotThrowWhenMultipleConstructorsAndArgsDepsProvided()
        {
            var container = new Container(rules => rules.With(made: FactoryMethod.ConstructorWithResolvableArguments));
            var config = new ServiceConfig();
            container.Register<Service>();

            Assert.DoesNotThrow(() => container.Resolve<Service>(args: new object[] { config }));
        }

        [Test]
        public void ResolveSouldNotThrowWhenMultipleConstructorsAndArgsDepsProvided_WithConcreteTypesResolution()
        {
            var container = new Container(rules => rules
                .WithAutoConcreteTypeResolution()
                .With(made: FactoryMethod.ConstructorWithResolvableArguments));
            var config = new ServiceConfig();

            Assert.DoesNotThrow(() => container.Resolve<Service>(args: new object[] { config }));
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
