using System;
using Moq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue355_Auto_mocking_feature_for_unit_testing
    {
        [Test]
        public void TestCase()
        {
            // this container is comprised of concrete type of tested unit,
            // Mock instances of its dependencies,
            // and instances of its dependencies taken from Mock<IDep>().Object
            // all registration are Singleton
            using (var container = new TestContainer())
            {
                // Arrangements
                const bool expected = true;

                container.Resolve<Mock<IDep>>()
                    .Setup(instance => instance.Method())
                    .Returns(expected);

                // Get concrete type instance of tested unit 
                // all dependencies are fulfilled with mocked instances
                var unit = container.Create<UnitOfWork>();

                // Action
                var actual = unit.InvokeDep();

                // Assertion
                Assert.AreEqual(expected, actual);
                container.Resolve<Mock<IDep>>()
                    .Verify(instance => instance.Method());
                Assert.AreEqual(1, container.MockAttempts);
            }
        }

        [Test]
        public void TestCase_Should_not_the_call_the_callback_for_the_registered_dependency()
        {
            // this container is comprised of concrete type of tested unit,
            // Mock instances of its dependencies,
            // and instances of its dependencies taken from Mock<IDep>().Object
            // all registration are Singleton
            var c = new Container();
            c.Register<IDep, Dep1>();

            using (var container = new TestContainer(c))
            {
                // Arrangements
                const bool expected = true;

                // Get concrete type instance of tested unit 
                // all dependencies are fulfilled with mocked instances
                var unit = container.Create<UnitOfWork>();

                // Action
                var actual = unit.InvokeDep();

                // Assertion
                Assert.AreEqual(expected, actual);
                Assert.AreEqual(0, container.MockAttempts);
            }
        }

        [Test]
        public void TestCase_with_dynamic_registration()
        {
            var testDynamicResolutionAttempts = 0;

            var prodContainer = new Container();

            using (var container = prodContainer.CreateChild(IfAlreadyRegistered.Replace,
                prodContainer.Rules.WithDynamicRegistration(
                    (serviceType, serviceKey) =>
                    {
                        ++testDynamicResolutionAttempts;
                        if (serviceType.IsInterface && serviceType.IsOpenGeneric() == false)
                        {
                            var mockType = typeof(Mock<>).MakeGenericType(serviceType);
                            return new[]
                            {
                                new DynamicRegistration(
                                    new DelegateFactory(r => ((Mock)r.Resolve(mockType)).Object, Reuse.Singleton, Setup.Default, serviceType),
                                    IfAlreadyRegistered.Keep),
                            };
                        }
                        return null;
                    },
                    DynamicRegistrationFlags.Service | DynamicRegistrationFlags.AsFallback)
                ))
            {
                container.Register(typeof(Mock<>), Reuse.Singleton, made: FactoryMethod.DefaultConstructor());

                container.Register<UnitOfWork>(Reuse.Singleton);

                // Arrangements
                const bool expected = true;

                container.Resolve<Mock<IDep>>()
                    .Setup(instance => instance.Method())
                    .Returns(expected);

                // Get concrete type instance of tested unit 
                // all dependencies are fulfilled with mocked instances
                var unit = container.Resolve<UnitOfWork>();

                // Action
                var actual = unit.InvokeDep();

                // Assertion
                Assert.AreEqual(expected, actual);
                container.Resolve<Mock<IDep>>()
                    .Verify(instance => instance.Method());

                Assert.AreEqual(1, testDynamicResolutionAttempts);
            }
        }

        [Test]
        public void TestCase_with_dynamic_registration_full_example()
        {
            var prodContainer = new Container();

            using (var container = CreateTestContainer(prodContainer))
            {
                container.Register<UnitOfWork>(Reuse.Singleton);

                // Arrangements
                const bool expected = true;

                container.Resolve<Mock<IDep>>()
                    .Setup(instance => instance.Method())
                    .Returns(expected);

                // Get concrete type instance of tested unit 
                // all dependencies are fulfilled with mocked instances
                var unit = container.Resolve<UnitOfWork>();

                // Action
                var actual = unit.InvokeDep();

                // Assertion
                Assert.AreEqual(expected, actual);
                container.Resolve<Mock<IDep>>()
                    .Verify(instance => instance.Method());
            }
        }

        public static IContainer CreateTestContainer(IContainer container)
        {
            var c = container.CreateChild(IfAlreadyRegistered.Replace,
                container.Rules.WithDynamicRegistration((serviceType, serviceKey) =>
                {
                    // ignore services with non-default key
                    if (serviceKey != null)
                        return null;

                    if (serviceType == typeof(object))
                        return null;

                    // get the Mock object for the abstract class or interface
                    if (serviceType.IsInterface || serviceType.IsAbstract)
                    {
                        // except for the open-generic ones
                        if (serviceType.IsGenericType && serviceType.IsOpenGeneric())
                            return null;

                        var mockType = typeof(Mock<>).MakeGenericType(serviceType);

                        var mockFactory = new DelegateFactory(r => ((Mock)r.Resolve(mockType)).Object, Reuse.Singleton);

                        return new[] { new DynamicRegistration(mockFactory, IfAlreadyRegistered.Keep) };
                    }

                    // concrete types
                    var concreteTypeFactory = ReflectionFactory.Of(serviceType, Reuse.Singleton,
                        FactoryMethod.ConstructorWithResolvableArgumentsIncludingNonPublic);

                    return new[] { new DynamicRegistration(concreteTypeFactory) };
                }, 
                DynamicRegistrationFlags.Service | DynamicRegistrationFlags.AsFallback));

            c.Register(typeof(Mock<>), Reuse.Singleton, FactoryMethod.DefaultConstructor());

            return c;
        }

        public interface IDep
        {
            bool Method();
        }

        public class Dep1 : IDep 
        {
            public bool Method() => true;
        }

        public class UnitOfWork : IDisposable
        {
            public readonly IDep Dep;
            public UnitOfWork(IDep d) => Dep = d;
            public void Dispose() { }

            public bool InvokeDep() => Dep.Method();
        }

        public class TestContainer : IDisposable
        {
            private readonly IContainer _container;
            public int MockAttempts { get; private set; } // to test if the dynamic registration callback is called 

            public T Create<T>() where T : class
            {
                _container.Register<T>(Reuse.Singleton);
                return _container.Resolve<T>();
            }

            public TD Resolve<TD>() where TD : class => _container.Resolve<TD>();

            public TestContainer(IContainer container = null)
            {
                _container = (container ?? new Container()).With(rules => rules.WithDynamicRegistration(
                    (serviceType, serviceKey) =>
                    {
                        ++MockAttempts;
                        if (serviceType.IsInterface && serviceType.IsOpenGeneric() == false)
                        {
                            var mockType = typeof(Mock<>).MakeGenericType(serviceType);
                            return new[]
                            {
                                new DynamicRegistration(
                                    new DelegateFactory(r => ((Mock)r.Resolve(mockType)).Object, Reuse.Singleton, null, serviceType),
                                    IfAlreadyRegistered.Keep),
                            };
                        }
                        return null;
                    },
                    DynamicRegistrationFlags.Service | DynamicRegistrationFlags.AsFallback
                ));

                _container.Register(typeof(Mock<>), Reuse.Singleton, made: FactoryMethod.DefaultConstructor());
            }

            public void Dispose() =>_container.Dispose();
        }
    }
}
