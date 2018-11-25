using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    class GHIssue45_Consider_expression_interpretation_to_speed_up_first_time_resolution
    {
        [Test]
        public void Test()
        {
            var container = new Container();

            container.Register<ScopedBlah>(Reuse.Scoped);

            container.Register<Parameter1>(Reuse.Transient);
            container.Register<Parameter2>(Reuse.Singleton);

            using (var scope = container.OpenScope())
            {
                var blah = scope.Resolve<ScopedBlah>();
                Assert.IsNotNull(blah);
            }
        }

        internal class Parameter1 { }
        internal class Parameter2 { }
        internal class Parameter3 { }

        internal class ScopedBlah
        {
            public Parameter1 Parameter1 { get; }
            public Parameter2 Parameter2 { get; }

            public ScopedBlah(Parameter1 parameter1, Parameter2 parameter2)
            {
                Parameter1 = parameter1;
                Parameter2 = parameter2;
            }
        }
    }
}
