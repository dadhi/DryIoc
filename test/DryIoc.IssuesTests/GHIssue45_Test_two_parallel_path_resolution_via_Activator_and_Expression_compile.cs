using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    class GHIssue45_Test_two_parallel_path_resolution_via_Activator_and_Expression_compile
    {
        [Test]
        public void Test()
        {
            var container = new Container();

            container.Register<ScopedBlah>(Reuse.Scoped);

            container.Register<Parameter1>(Reuse.Transient);
            container.Register<Parameter2>(Reuse.Singleton);
            container.Register<Parameter3>(Reuse.Scoped);

            using (var scope = container.OpenScope())
            {
                var blah = scope.Resolve<ScopedBlah>();
                Assert.IsNotNull(blah);
            }

            Assert.Fail();
        }

        internal class Parameter1 { }
        internal class Parameter2 { }
        internal class Parameter3 { }

        internal class ScopedBlah
        {
            public Parameter1 Parameter1 { get; }
            public Parameter2 Parameter2 { get; }
            public Parameter3 Parameter3 { get; }

            public ScopedBlah(Parameter1 parameter1, Parameter2 parameter2, Parameter3 parameter3)
            {
                Parameter1 = parameter1;
                Parameter2 = parameter2;
                Parameter3 = parameter3;
            }
        }
    }
}
