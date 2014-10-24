using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    class NewIssue
    {
        [Test]
        public void Test()
        {
            var container = new Container();
            container.Register<FooWithIndexer>(setup: Setup.With(
                propertiesAndFields: PropertiesAndFields.All()));

            var indexer = container.Resolve<FooWithIndexer>();
        }

        public class FooWithIndexer
        {
            public object this[int index]
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
    }
}
