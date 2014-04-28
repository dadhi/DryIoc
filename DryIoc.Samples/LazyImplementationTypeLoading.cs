using System;
using System.Reflection;
using NUnit.Framework;

namespace DryIoc.Samples
{
    [TestFixture]
    public class LazyImplementationTypeLoading
    {
        [Test]
        public void Register_implementation_on_demand_from_dynamically_loaded_assembly()
        {
            var container = new Container();

            var addinAssembly = new Lazy<Assembly>(() => Assembly.LoadFrom("DryIoc.Samples.CUT.dll"));

            container.Register<IAddin>(new FactoryProvider(
                (_, __) => new ReflectionFactory(
                    addinAssembly.Value.GetType("DryIoc.Samples.CUT.SomeAddin"), 
                    Reuse.Singleton)));
            
            container.Register<AddinUser>();

            var userOne = container.Resolve<AddinUser>();
            var userTwo = container.Resolve<AddinUser>();

            Assert.That(userOne.Addin, Is.SameAs(userTwo.Addin));
        }
    }

    public interface IAddin {}

    public class AddinUser
    {
        public IAddin Addin { get; set; }

        public AddinUser(IAddin addin)
        {
            Addin = addin;
        }
    }
}
