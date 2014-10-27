using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    class Issue_SkipIndexerOnAllPropertiesInjection
    {
        [Test]
        public void Test()
        {
            var container = new Container();
            container.Register<FooWithIndexer>(setup: Setup.With(
                propertiesAndFields: PropertiesAndFields.All(IfUnresolved.Throw)));

            Assert.DoesNotThrow(() => 
                container.Resolve<FooWithIndexer>()
            );
        }

        public class FooWithIndexer
        {
            public IService this[int index]
            {
                get
                {
                    return null;
                }
                set
                {

                }
            }
        }

        internal interface IService {}
    }
}
