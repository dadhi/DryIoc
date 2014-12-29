using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    class Issue86_SkipIndexerOnAllPropertiesInjection
    {
        [Test]
        public void Test()
        {
            var container = new Container();
            container.Register<FooWithIndexer>(with: PropertiesAndFields.All(ifUnresolved: IfUnresolved.Throw));

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
