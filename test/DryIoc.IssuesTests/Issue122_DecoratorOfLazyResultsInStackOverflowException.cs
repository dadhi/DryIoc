using DryIoc.MefAttributedModel;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue122_DecoratorOfLazyResultsInStackOverflowException : ITest
    {
        public int Run()
        {
            Decorator_may_be_applied_to_decorated_Lazy();
            return 1;
        }

        [Test]
        public void Decorator_may_be_applied_to_decorated_Lazy()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(DecoratedResult), typeof(DynamicDecorator));

            var me = container.Resolve<IDecoratedResult>();
            var result = me.GetResult();

            Assert.AreEqual(2, result);
        }
    }
}
