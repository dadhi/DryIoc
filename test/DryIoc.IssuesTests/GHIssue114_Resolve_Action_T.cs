using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue114_Resolve_Action_T : ITest
    {
        public int Run() 
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            var container = new Container();

            var messages = new List<string>();
            container.RegisterInstance<Action<string>>(s => messages.Add($"1-{s}"), IfAlreadyRegistered.AppendNotKeyed);
            container.RegisterInstance<Action<string>>(s => messages.Add($"2-{s}"), IfAlreadyRegistered.AppendNotKeyed);

            var actions = container.ResolveMany<Action<string>>().ToList();
            Assert.AreEqual(2, actions.Count);

            actions.ForEach(a => a.Invoke("Hello world"));
            CollectionAssert.AreEquivalent(new[] { "1-Hello world", "2-Hello world" }, messages);
        }
    }
}
