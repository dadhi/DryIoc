using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIocZero.UnitTests
{
    [TestFixture]
    public class GeneratedResolutionsTests
    {
        [Test]
        public void Export_condition_should_be_evaluated()
        {
            var container = new Container();

            Assert.IsInstanceOf<ExportConditionalObject1>(container.Resolve<ImportConditionObject1>().ExportConditionInterface);
            Assert.IsInstanceOf<ExportConditionalObject2>(container.Resolve<ImportConditionObject2>().ExportConditionInterface);
            Assert.IsInstanceOf<ExportConditionalObject3>(container.Resolve<ImportConditionObject3>().ExportConditionInterface);
        }
    }
}
