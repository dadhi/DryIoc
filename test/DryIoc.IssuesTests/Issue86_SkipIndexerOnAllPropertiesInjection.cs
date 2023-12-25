using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    public class Issue86_SkipIndexerOnAllPropertiesInjection : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            var container = new Container();
            container.Register<FooWithIndexer>(made: PropertiesAndFields.All(ifUnresolved: IfUnresolved.Throw));

            Assert.DoesNotThrow(() => 
                container.Resolve<FooWithIndexer>()
            );
        }

        internal class FooWithIndexer
        {
            public IService this[int index]
            {
                get { return null; }
                set { }
            }
        }

        internal interface IService {}
    }
}
