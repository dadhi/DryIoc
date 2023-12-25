using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue367_MefRulesBreakMadeParameters : ITest
    {
        public int Run()
        {
            DryIoc_supports_made_parameters();
            DryIoc_supports_made_factory_method_with_parameters();
            DryIoc_WithMef_supports_made_parameters();
            DryIoc_WithMef_supports_made_factory_method_with_parameters();
            return 4;
        }

        [Test]
        public void DryIoc_supports_made_parameters()
        {
            // standard settings
            var container = new Container();
            container.Register<MadeParametersTest>(made: Parameters.Of.Type(r => "a"));

            var instance = container.Resolve<MadeParametersTest>();
            Assert.IsNotNull(instance);
            Assert.AreEqual("a", instance.Name);
        }

        [Test]
        public void DryIoc_supports_made_factory_method_with_parameters()
        {
            // standard settings + factory method
            var container = new Container();
            container.Register(typeof(MadeParametersTest), made: factoryMethod);

            var instance = container.Resolve<MadeParametersTest>();
            Assert.IsNotNull(instance);
            Assert.AreEqual("b", instance.Name);
        }

        [Test]
        public void DryIoc_WithMef_supports_made_parameters()
        {
            // MEF settings
            var container = new Container().WithMef();
            container.Register<MadeParametersTest>(made: Parameters.Of.Type(r => "a"));

            var instance = container.Resolve<MadeParametersTest>();
            Assert.IsNotNull(instance);
            Assert.AreEqual("a", instance.Name);
        }

        [Test]
        public void DryIoc_WithMef_supports_made_factory_method_with_parameters()
        {
            // MEF settings + factory method
            var container = new Container().WithMef();
            container.Register(typeof(MadeParametersTest), made: factoryMethod);

            var instance = container.Resolve<MadeParametersTest>();
            Assert.IsNotNull(instance);
            Assert.AreEqual("b", instance.Name);
        }

        public class MadeParametersTest
        {
            public MadeParametersTest(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }

        public static MadeParametersTest FactoryMethod(string name)
        {
            return new MadeParametersTest(name);
        }

        private static Made factoryMethod = Made.Of(
            typeof(Issue367_MefRulesBreakMadeParameters).Method("FactoryMethod", typeof(string)),
            parameters: Parameters.Of.Type<string>(r => "b"));
    }
}
