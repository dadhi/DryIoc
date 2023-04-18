using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue572_Dynamic_Service_Keys
    {
        [Test]
        public void Can_provide_argument_both_with_the_factory_and_via_Resolve()
        {
            var c = new Container();

            c.RegisterInstance(new DataStore { Accounts = new[] { new Account { IsDefault = true, Token = "aaa" } }});

            c.Register(Made.Of(() => CreateClient(Arg.Of<DataStore>(), null)));

            // alternative ways of ISomeServiceClient registrations
            //c.Register(Made.Of(() => CreateClient(Arg.Of<DataStore>(), Arg.Of<string>(IfUnresolved.ReturnDefault))));
            //c.Register<ISomeServiceClient>(made: Made.Of(GetType().SingleMethod(nameof(CreateClient))));

            var x = c.Resolve<ISomeServiceClient>();
            Assert.AreEqual("aaa", x.ServiceToken);

            var y = c.Resolve<ISomeServiceClient>(args: new object[] {"bbb"});
            Assert.AreEqual("bbb", y.ServiceToken);
        }

        public static ISomeServiceClient CreateClient(DataStore dataStore, string forceToken = null)
        {
            if (!string.IsNullOrEmpty(forceToken))
                return new MyClient(forceToken);
            var defaultClient = dataStore.Accounts.First(a => a.IsDefault);
            return new MyClient(defaultClient.Token);
        }

        // note: alternatively you may wrap the string token in Type, to provide it with a typed identity - helps in registrations to distinguish from string.
        //public class Token
        //{
        //    public string Value;
        //}

        public interface ISomeServiceClient
        {
            string ServiceToken { get; }
        }

        public class MyClient : ISomeServiceClient
        {
            public string ServiceToken { get; }

            public MyClient(string serviceToken)
            {
                ServiceToken = serviceToken;
            }
        }

        public class DataStore
        {
            public IEnumerable<Account> Accounts;
        }

        public class Account 
        {
            public bool IsDefault { get; set; }
            public string Token { get; set; }
        }
    }
}
