using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue159_Context_based_injection_doesnt_work_with_InjectPropertiesAndFields : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            var c = new Container(Rules.Default.With(
                propertiesAndFields: PropertiesAndFields.Auto));

            c.Register<C>();
            c.Register(
                reuse: Reuse.Transient,
                made: Made.Of(
                    () => new Str { S = Arg.Index<string>(0) },
                    req => req.Parent.IsEmpty ? "Oops!" : req.Parent.ImplementationType.Name));

            var x = c.InjectPropertiesAndFields(new C());
            var y = c.Resolve<C>();

            Assert.AreEqual("C", x.S.S);
            Assert.AreEqual("C", y.S.S); 
        }

        public class C
        {
            public Str S { get; set; }
        }

        public class Str { public string S; }
    }
}
