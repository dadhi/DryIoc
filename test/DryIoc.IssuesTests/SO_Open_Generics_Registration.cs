using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class SO_Open_Generics_Registration : ITest
    {
        public int Run()
        {
            Main();
            return 1;
        }

        [Test]
        public void Main()
        {
            var container = new Container();

            container.Register<Setup>(Reuse.Singleton);

            container.Register(typeof(Configuration<>), Reuse.Singleton,
                FactoryMethod.ConstructorWithResolvableArgumentsIncludingNonPublic);

            container.Register(typeof(Process<>), Reuse.Singleton,
                FactoryMethod.ConstructorWithResolvableArgumentsIncludingNonPublic);

            var p = container.Resolve<Process<EventArgs>>();

            Assert.IsNotNull(p);
        }

        public class Setup { }

        public class Configuration<T> where T : class
        {
            internal Configuration(Setup setup) { }
        }

        public class Process<T> where T : class
        {
            internal Process(Configuration<T> configuration) { }
        }
    }
}
