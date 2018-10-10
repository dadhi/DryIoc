using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue566_Named_service_not_replaced
    {
        [Test]
        public void Can_be_replaced()
        {
            var c = new Container();

            const string serviceKey = "aaa";

            c.Register<IBlah, Blah>(serviceKey: serviceKey);
            c.Register<IBlah, Blah2>(serviceKey: serviceKey, ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            Assert.IsInstanceOf<Blah2>(c.Resolve<IBlah>(serviceKey));
        }

        [Test]
        public void Can_be_unregistered()
        {
            var c = new Container();

            const string serviceKey = "aaa";

            c.Register<IBlah, Blah>(serviceKey: serviceKey);
            c.Unregister<IBlah>(serviceKey);
            c.Register<IBlah, Blah2>(serviceKey: serviceKey);

            Assert.IsInstanceOf<Blah2>(c.Resolve<IBlah>(serviceKey));
        }
    }

    interface IBlah {}
    class Blah : IBlah {}
    class Blah2 : IBlah { }
}
