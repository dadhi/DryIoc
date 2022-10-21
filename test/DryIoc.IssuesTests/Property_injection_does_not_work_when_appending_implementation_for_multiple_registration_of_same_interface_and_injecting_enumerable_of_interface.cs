using NUnit.Framework;
using System.Collections.Generic;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Property_injection_does_not_work_when_appending_implementation_for_multiple_registration_of_same_interface_and_injecting_enumerable_of_interface
    {
        [Test]
        [Ignore("fixme")]
        public void Test()
        {
            // ARRANGE
            const string testFoo1 = "TF1";
            const string testFoo2 = "TF2";

            var container = new Container();

            container.RegisterMany(new[] { typeof(Foo1), typeof(IFoo) }, typeof(Foo1), reuse: Reuse.Singleton);
            var propertiesAndFieldsSelector = PropertiesAndFields.Of.Name(nameof(IFoo.Test), _ => testFoo1);
            container.RegisterMany(new[] { typeof(Foo1), typeof(IFoo) }, typeof(Foo1), reuse: Reuse.Singleton, made: propertiesAndFieldsSelector, ifAlreadyRegistered: IfAlreadyRegistered.AppendNewImplementation);

            container.RegisterMany(new[] { typeof(Foo2), typeof(IFoo) }, typeof(Foo2), reuse: Reuse.Singleton);
            propertiesAndFieldsSelector = PropertiesAndFields.Of.Name(nameof(IFoo.Test), _ => testFoo2);
            container.RegisterMany(new[] { typeof(Foo2), typeof(IFoo) }, typeof(Foo2), reuse: Reuse.Singleton, made: propertiesAndFieldsSelector, ifAlreadyRegistered: IfAlreadyRegistered.AppendNewImplementation);

            // ACT
            var foo1 = container.Resolve<Foo1>();
            var foo2 = container.Resolve<Foo2>();
            var foos = container.Resolve<IEnumerable<IFoo>>();

            // ASSERT
            Assert.AreEqual(testFoo1, foo1.Test);
            Assert.AreEqual(testFoo2, foo2.Test);

            foreach (var foo in foos)
            {
                switch (foo)
                {
                    case Foo1 foo1FromEnumerable:
                        Assert.AreEqual(testFoo1, foo.Test);
                        Assert.AreEqual(foo1, foo1FromEnumerable);
                        break;
                    case Foo2 foo2FromEnumerable:
                        Assert.AreEqual(testFoo2, foo.Test);
                        Assert.AreEqual(foo2, foo2FromEnumerable);
                        break;
                }
            }
        }

        public interface IFoo
        {
            string Test { get; set; }
        }

        public class Foo1 : IFoo
        {
            public string Test { get; set; }
        }

        public class Foo2 : IFoo
        {
            public string Test { get; set; }
        }
    }
}
