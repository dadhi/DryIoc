using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue404_ConstructorWithResolvableArguments_does_not_take_into_account_parameter_service_key
    {
        [Test]
        public void Test()
        {
            var container = new Container();

            container.RegisterInstance("hello world", serviceKey: "x");

            container.Register(typeof(ConnSet),
                made: Made.Of(propertiesAndFields: PropertiesAndFields.Of.Name("Name", serviceKey: "x")),
                reuse: Reuse.Transient,
                serviceKey: "somekey");

            container.Register(typeof(ICloud), typeof(Cloud),
                made: Made.Of(parameters: Parameters.Of.Name("conn", serviceKey: "somekey"),
                              factoryMethod: FactoryMethod.ConstructorWithResolvableArguments));

            var cloud = container.Resolve<ICloud>();
            Assert.IsNotNull(cloud);
        }

        public interface ICloud
        {
            string Name { get; }
        }

        public class Cloud : ICloud
        {
            private readonly ConnSet _conn;

            public Cloud(ConnSet conn)
            {
                _conn = conn;
            }

            public Cloud(string xx)
            {
                _conn = new ConnSet { Name = xx };
            }

            public string Name => _conn.Name;
        }

        public class ConnSet
        {
            public string Name { get; set; }
        }
    }
}
