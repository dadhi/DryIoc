using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue500_Rule_WithConcreteTypeDynamicRegistrations_disables_allowDisposableTransient
    {
        class DbContext : IDisposable
        {
            public void Dispose() { }
        }

        class MyDbContext : DbContext { }

        // Base class for testing service using Disposable DbContext.
        abstract class ServiceBase
        {
            public string ServiceTypeName { get; set; }
            public abstract void UseDbContext();
        }

        class ServiceWithFuncInjection : ServiceBase
        {
            private readonly Func<DbContext> _createDbContext;

            public ServiceWithFuncInjection(Func<DbContext> createDbContext)
            {
                _createDbContext = createDbContext;
            }

            public override void UseDbContext()
            {
                using (var context = _createDbContext())
                {
                    ServiceTypeName = context.GetType().Name;
                }
            }
        }

        class ServiceWithResolve : ServiceBase
        {
            private readonly Container _container;

            public ServiceWithResolve(Container container)
            {
                _container = container;
            }

            public override void UseDbContext()
            {
                using (var context = _container.Resolve<DbContext>())
                {
                    ServiceTypeName = context.GetType().Name;
                }
            }
        }

        [Test] // Passes
        public void ContainerWithNoRules_ServiceWithResolve()
        {
            // Arrange
            var sut = new Container();
            sut.Register<DbContext, MyDbContext>(setup: Setup.With(allowDisposableTransient: true));
            sut.Register<ServiceBase, ServiceWithResolve>();
            sut.RegisterInstance(sut);

            // Act
            var controller = sut.Resolve<ServiceBase>();
            controller.UseDbContext();

            // Assert
            Assert.AreEqual(typeof(MyDbContext).Name, controller.ServiceTypeName);
        }

        [Test]
        public void ContainerWithConcreteTypeDynamicRegistrations_ServiceWithResolve()
        {
            // Arrange
            var sut = new Container(rules => rules.WithConcreteTypeDynamicRegistrations());
            sut.Register<DbContext, MyDbContext>(setup: Setup.With(allowDisposableTransient: true));
            sut.Register<ServiceBase, ServiceWithResolve>();
            sut.RegisterInstance(sut);

            // Act
            var controller = sut.Resolve<ServiceBase>(); // Throws: Registered Disposable Transient service blah blah
            controller.UseDbContext();

            // Assert
            Assert.AreEqual(typeof(MyDbContext).Name, controller.ServiceTypeName);
        }

        [Test]  // Passes
        public void ContainerWithNoRules_ServiceWithFuncInjection()
        {
            // Arrange
            var sut = new Container();
            sut.Register<DbContext, MyDbContext>(setup: Setup.With(allowDisposableTransient: true));
            sut.Register<ServiceBase, ServiceWithFuncInjection>();

            // Act
            var controller = sut.Resolve<ServiceBase>();
            controller.UseDbContext();

            // Assert
            Assert.AreEqual(typeof(MyDbContext).Name, controller.ServiceTypeName);
        }

        [Test]// Fixed!
        public void ContainerWithConcreteTypeDynamicRegistrations_ServiceWithFuncInjection()
        {
            // Arrange
            var sut = new Container(rules => rules.WithConcreteTypeDynamicRegistrations());
            sut.Register<DbContext, MyDbContext>(setup: Setup.With(allowDisposableTransient: true));
            sut.Register<ServiceBase, ServiceWithFuncInjection>();

            // Act
            var controller = sut.Resolve<ServiceBase>(); // Throws:Registered Disposable Transient service blah blah
            controller.UseDbContext();

            // Assert
            Assert.AreEqual(typeof(MyDbContext).Name, controller.ServiceTypeName);
        }
    }
}
