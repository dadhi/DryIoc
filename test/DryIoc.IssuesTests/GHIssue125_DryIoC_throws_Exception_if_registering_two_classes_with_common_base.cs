using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue125_DryIoC_throws_Exception_if_registering_two_classes_with_common_base
    {
        [Test]
        public void Test()
        {
            var container = new Container(rules => rules.With(FactoryMethod.ConstructorWithResolvableArguments));

            var fwTypes = typeof(Class1).GetAssembly().GetLoadedTypes()
                .Where(t => !t.IsInterface && !t.IsAbstract && (t.Name.Contains("Class1") || t.Name.Contains("Class2")));

            container.RegisterMany(fwTypes, 
                serviceTypeCondition: t => !t.IsAbstract,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw,
                nonPublicServiceTypes: true);
        }

        public class Class1 : CommonBase, IClass1
        {
            public string Greet() => "Framework";
        }

        public class Class2 : CommonBase, IClass2
        {
            public string Speak() => "Framework";
        }

        interface IClass1
        {
            string Greet();
        }

        interface IClass2
        {
            string Speak();
        }

        public abstract class CommonBase
        {
        }
    }
}
