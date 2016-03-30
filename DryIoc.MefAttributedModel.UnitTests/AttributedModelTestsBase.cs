using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    public class AttributedModelTestsBase
    {
        protected IContainer _container;

        [SetUp]
        public void Init()
        {
            _container = new Container().WithMefAttributedModel();
            var cutAssembly = typeof(DependentService).GetAssembly();
            _container.RegisterExports(new[] { cutAssembly });
        }

        [TearDown]
        public void Dispose()
        {
            _container.Dispose();
        }
    }
}