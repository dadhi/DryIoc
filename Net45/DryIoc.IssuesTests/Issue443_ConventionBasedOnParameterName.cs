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
                .With(parameters: Parameters.Of.Details(
                    (_, p) => p.ParameterType == typeof(string) ? ServiceDetails.Of(serviceKey: p.Name) : ServiceDetails.Default)));

            c.Register<Blah>();

            c.UseInstance("a", serviceKey: "a");
            c.UseInstance(1);

            var blah = c.Resolve<Blah>();
        }

        public class Blah
        {
            public Blah(int n, string a) {}
        }
    }
}
