using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue443_ConventionBasedOnParameterName
    {
        [Test]
        public void Can_globally_use_string_parameter_name_as_service_key()
        {
            var c = new Container(rules => rules
                .With(parameters: Parameters.Of.Type<string>((_, p) => ServiceDetails.Of(serviceKey: p.Name))));

            c.Register<Blah>();

            c.RegisterInstance("a", serviceKey: "a");
            c.RegisterInstance(1);

            var blah = c.Resolve<Blah>();
        }

        public class Blah
        {
            public Blah(int n, string a) {}
        }
    }
}
