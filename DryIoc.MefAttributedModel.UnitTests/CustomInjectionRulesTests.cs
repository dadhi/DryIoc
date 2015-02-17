using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class CustomInjectionRulesTests
    {
        [Test]
        public void Can_combine_MEF_Imports_with_custom_Injection_rules_for_parameters()
        {
            var container = new Container().WithMefAttributedModel();
            container.Register<ClientWithPrimitiveParameter>(with: Parameters.Of.Name("message", "hell"));
            container.RegisterExports(typeof(KeyService));

            var client = container.Resolve<ClientWithPrimitiveParameter>();

            Assert.That(client.Message, Is.EqualTo("hell"));
        }

        [Test]
        public void Can_combine_MEF_Imports_with_custom_Injection_rules_for_properties()
        {
            var container = new Container().WithMefAttributedModel();
            container.Register<ClientWithServiceAndPrimitiveProperty>(
                with: PropertiesAndFields.Of.Name("Message", "hell"));

            container.RegisterExports(typeof(KeyService));

            var client = container.Resolve<ClientWithServiceAndPrimitiveProperty>();

            Assert.That(client.Message, Is.EqualTo("hell"));
        }
    }
}
