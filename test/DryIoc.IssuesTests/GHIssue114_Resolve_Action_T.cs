using System;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue114_Resolve_Action_T
    {
        [Test]
        public void Test()
        {
            var container = new Container();

            container.RegisterInstance<Action<string>>(s => Console.WriteLine("1-{0}", s), IfAlreadyRegistered.AppendNotKeyed);
            container.RegisterInstance<Action<string>>(s => Console.WriteLine("2-{0}", s), IfAlreadyRegistered.AppendNotKeyed);

            var actions = container.ResolveMany<Action<string>>().ToList();
            Assert.AreEqual(2, actions.Count);

            actions.ForEach(a => a.Invoke("Hello world"));
        }
    }
}
