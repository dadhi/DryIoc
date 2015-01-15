using System.Linq;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class RegisterAssemblyTests
    {
        [Test]
        public void Can_register_service_with_implementations_found_in_assemblies()
        {
            var container = new Container();

            container.RegisterBatch<IBlah>(new[] { typeof(Blah), typeof(AnotherBlah) });

            var services = container.Resolve<IBlah[]>();

            CollectionAssert.AreEquivalent(
                new[] { typeof(Blah), typeof(AnotherBlah) },
                services.Select(s => s.GetType()));
        }

        [Test]
        public void Can_register_genric_service_with_implementations_found_in_assemblies()
        {
            var container = new Container();

            container.RegisterBatch(typeof(IBlah<,>), new[] { typeof(IBlah<,>).GetAssembly() });

            var services = container.Resolve<IBlah<string, bool>[]>();

            CollectionAssert.AreEquivalent(
                new[] { typeof(Blah<string, bool>), typeof(AnotherBlah<bool>) },
                services.Select(s => s.GetType()));
        }

        public interface IBlah { }
        public class Blah : IBlah { }
        public class AnotherBlah : IBlah { }

        public interface IBlah<T0, T1> { }
        public class Blah<T0, T1> : IBlah<T0, T1> { }
        public class AnotherBlah<T> : IBlah<string, T> { }
    }
}
