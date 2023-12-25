using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue264_IfAlreadyRegisteredReplaceCanSpanMultipleRegistrations : ITest
    {
        public int Run()
        {
            Replace_in_RegisterMany_should_remove_all_service_type_registrations();
            Can_unregister_result_of_register_many();
            return 2;
        }

        [Test]
        public void Replace_in_RegisterMany_should_remove_all_service_type_registrations()
        {
            var container = new Container(rules => rules
                .With(FactoryMethod.ConstructorWithResolvableArguments)
                .WithoutThrowIfDependencyHasShorterReuseLifespan());

            container.RegisterMany<A>(Reuse.Singleton);
            container.RegisterMany<CX>(Reuse.Singleton);

            container.RegisterMany<B>(Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            var ix = container.Resolve<IX[]>();

            Assert.AreEqual(1, ix.Length);
            Assert.IsInstanceOf<B>(ix[0]);
        }

        [Test]
        public void Can_unregister_result_of_register_many()
        {
            var container = new Container();

            container.RegisterMany<A>(Reuse.Singleton);
            container.RegisterMany<CX>(Reuse.Singleton);

            var typesToUnregister = 
                typeof(A).GetImplementedServiceTypes().Union(
                typeof(CX).GetImplementedServiceTypes());

            foreach (var t in typesToUnregister)
                container.Unregister(t);

            container.RegisterMany<B>(Reuse.Singleton);

            var ix = container.Resolve<IX[]>();

            Assert.AreEqual(1, ix.Length);
            Assert.IsInstanceOf<B>(ix[0]);
        }

        public interface IX { }

        public interface IA : IX { }

        public class CX : IX { }

        public class A : IA { }

        public interface IB : IA { }

        public class B : A, IB { }
    }
}
