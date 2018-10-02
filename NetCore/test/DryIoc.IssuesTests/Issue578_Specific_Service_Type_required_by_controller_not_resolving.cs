using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue578_Specific_Service_Type_required_by_controller_not_resolving
    {
        [Test]
        public void Test()
        {
            var c = new Container();

            c.Register<IService, Implementation1>(Reuse.InCurrentScope, serviceKey: ServiceKeys.Implementation1);
            c.Register<IService, Implementation2>(Reuse.InCurrentScope, serviceKey: ServiceKeys.Implementation2);
            
            // consumer is unable to select IService from two keyed registrations
            //c.Register<IConsumer, Consumer>(Reuse.InCurrentScope);

            // the fix:
            c.Register<IConsumer, Consumer>(Reuse.InCurrentScope, 
                Parameters.Of.Type<IService>(serviceKey: ServiceKeys.Implementation1));

            c.Register<Consumer>(Reuse.InCurrentScope, 
                Parameters.Of.Type<IService>(serviceKey: ServiceKeys.Implementation2));

            using (var s = c.OpenScope(Reuse.WebRequestScopeName))
            {
                var consumer = s.Resolve<IConsumer>();
                Assert.IsNotNull(consumer.Service);
            }
        }

        enum ServiceKeys
        {
            Implementation1,
            Implementation2
        }

        public interface IService { }

        public class Implementation1 : IService { }
        public class Implementation2 : IService { }

        public interface IConsumer 
        {
            IService Service { get; }
        }

        public class Consumer : IConsumer
        {
            public IService Service { get; }

            public Consumer(IService service)
            {
                Service = service;
            }
        }
    }
}
