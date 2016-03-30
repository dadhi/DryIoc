using NUnit.Framework;

namespace DryIocZero.UnitTests
{
    public class AttributedModelTestsBase
    {
        protected Container _container;

        [SetUp]
        public void Init()
        {
            _container = new Container();
        }

        [TearDown]
        public void Dispose()
        {
            _container.Dispose();
        }
    }
}