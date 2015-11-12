using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue181__RegisterInstanceUnregister
    {
        [Test]
        public void Test_without_Unregister()
        {
            var container = new Container();

            container.Register(typeof(Printer));

            var test = new Test { N = 1 };
            container.RegisterInstance<ITest>(test);
            var printer = container.Resolve<IPrinter>(typeof(Printer));
            Assert.AreEqual("1", printer.Print()); // prints '1' as expected

            test = new Test { N = 2 };
            container.RegisterInstance<ITest>(test, ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            Assert.AreEqual(2, container.Resolve<ITest>().N); // does not throws, replaced dependency

            printer = container.Resolve<IPrinter>(typeof(Printer));
            Assert.AreEqual("2", printer.Print()); // prints '1', I would expect this to print '2'
        }

        [Test, Ignore]
        public void Test_with_Unregister()
        {
            var container = new Container();

            container.Register(typeof(Printer));

            var test = new Test { N = 1 };
            container.RegisterInstance<ITest>(test);
            var printer = container.Resolve<IPrinter>(typeof(Printer));
            Assert.AreEqual("1", printer.Print()); // prints '1' as expected

            container.Unregister(typeof(ITest));

            Assert.Throws<ContainerException>(() => 
                container.Resolve<ITest>());

            printer = container.Resolve<IPrinter>(typeof(Printer));
            printer.Print(); // prints '1', I would expect this to throw

            test = new Test { N = 2 };
            container.RegisterInstance<ITest>(test, ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            printer = container.Resolve<IPrinter>(typeof(Printer));
            Assert.AreEqual("2", printer.Print()); // prints '1', I would expect this to print '2'
        }

        private class Printer : IPrinter
        {
            private readonly ITest _test;

            public Printer(ITest test)
            {
                _test = test;
            }

            public string Print()
            {
                return _test.N.ToString();
            }
        }

        private interface IPrinter
        {
            string Print();
        }

        private interface ITest
        {
            int N { get; }
        }

        private class Test : ITest
        {
            public int N { get; set; }
        }
    }
}
