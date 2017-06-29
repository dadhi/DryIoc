using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.IssuesTests.Interception
{
    [TestFixture]
    public class WrapAsLazyTests
    {
        [Test]
        public void Test_lazy_interception1()
        {
            var c = new Container();
            c.RegisterAsLazy<IAlwaysLazy, LazyService>();

            LazyService.LastValue = "NotCreated";

            var proxy = c.Resolve<IAlwaysLazy>();
            Assert.AreEqual("NotCreated", LazyService.LastValue);

            proxy.Test("Created!");
            Assert.AreEqual("Created!", LazyService.LastValue);
        }

        [Test]
        public void Test_lazy_interception2()
        {
            var c = new Container();
            c.Register<IAlwaysLazy, LazyService>();
            c.ResolveAsLazy<IAlwaysLazy>();

            LazyService.LastValue = "NotCreated";

            var proxy = c.Resolve<IAlwaysLazy>();
            Assert.AreEqual("NotCreated", LazyService.LastValue);

            proxy.Test("Created!");
            Assert.AreEqual("Created!", LazyService.LastValue);
        }
    }

    public interface IAlwaysLazy
    {
        void Test(string x);
    }

    class LazyService : IAlwaysLazy
    {
        public LazyService()
        {
            LastValue = "LazyServiceCreated";
        }

        public static string LastValue { get; set; }

        public void Test(string x)
        {
            LastValue = x;
        }
    }
}
