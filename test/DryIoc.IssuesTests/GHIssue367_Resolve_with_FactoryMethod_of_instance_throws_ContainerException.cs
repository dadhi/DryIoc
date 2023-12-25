using NUnit.Framework;
using System.Linq;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue367_Resolve_with_FactoryMethod_of_instance_throws_ContainerException : ITest
    {
        public int Run()
        {
            Test_StaticFactory();
            Test_Factory_as_Service();
            Test_Factory_as_Instance();
            return 3;
        }

        public interface IAbstraction<T>
        {
            string Description { get; }
        }

        public class Abstraction<T> : IAbstraction<T>
        {
            public string Description { get; } = typeof(T).Name;
        }

        public class Factory
        {
            public IAbstraction<T> Create<T>()
            {
                return new Abstraction<T>();
            }
        }

        public class StaticFactory
        {
            public static IAbstraction<T> Create<T>()
            {
                return new Abstraction<T>();
            }
        }

        [Test]
        public void Test_StaticFactory()
        {
            var container = new Container();
            var factoryMethod = FactoryMethod.Of(typeof(StaticFactory).GetMethods()
                .Single(m => m.Name == "Create" && m.IsGenericMethodDefinition));
            
            container.Register(typeof(IAbstraction<>), made: Made.Of(factoryMethod: factoryMethod));

            var abs = container.Resolve<IAbstraction<string>>();
            Assert.IsNotNull(abs);
            Assert.AreEqual("String", abs.Description);
        }

        [Test]
        public void Test_Factory_as_Service()
        {
            var container = new Container();
    
            var factory = new Factory();
            container.RegisterInstance(factory);

            var factoryMethod = FactoryMethod.Of(typeof(Factory).GetMethods()
                .Single(m => m.Name == "Create" && m.IsGenericMethodDefinition), 
                ServiceInfo.Of<Factory>());
    
            container.Register(typeof(IAbstraction<>), made: Made.Of(factoryMethod: factoryMethod));
            
            var abs = container.Resolve<IAbstraction<string>>();
            Assert.IsNotNull(abs);
            Assert.AreEqual("String", abs.Description);
        }

        [Test]
        public void Test_Factory_as_Instance()
        {
            var container = new Container();
    
            var factory = new Factory();
            var factoryMethod = FactoryMethod.Of(typeof(Factory).GetMethods()
                .Single(m => m.Name == "Create" && m.IsGenericMethodDefinition), 
                factoryInstance: factory);

            container.Register(typeof(IAbstraction<>), made: Made.Of(factoryMethod: factoryMethod));

            var abs = container.Resolve<IAbstraction<string>>();

            Assert.IsNotNull(abs);
            Assert.AreEqual("String", abs.Description);
        }
    }
}