using System.Reflection;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class RegisterAssemblyTests
    {
        [Test, Ignore]
        public void Can_register_assembly_and_resolve_impl_type_from_it()
        {
            var container = new Container();

            container.RegisterAssemblies(GetType().Assembly);

            var service = container.Resolve<InAsm>();
        }

        public class InAsm
        {
             
        }
    }

    public static class MyClass
    {
        public static void RegisterAssemblies(this IRegistrator registrator, params Assembly[] assemblies)
        {

        }
    }
}
