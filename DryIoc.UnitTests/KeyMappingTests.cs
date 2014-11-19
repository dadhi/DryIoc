using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class KeyMappingTests
    {
        [Test]
        public void Can_map_specific_key_to_default()
        {


            var container = new Container(rules => rules
                .WithKeyMapping((serviceKey, _) => serviceKey ?? 1));

            container.Register<Me>(named: 1);

            container.Resolve<Me>();
        }

        public class Me {}
    }
}
