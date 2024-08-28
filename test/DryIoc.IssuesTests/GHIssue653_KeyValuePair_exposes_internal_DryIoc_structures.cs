using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using DryIoc.Microsoft.DependencyInjection;

#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
#endif

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue653_KeyValuePair_exposes_internal_DryIoc_structures : ITest
    {
        public int Run()
        {
            Resolve_collection_of_KeyValuePairs();
            return 1;
        }

        [Test] // todo: @fixme
        public void Resolve_collection_of_KeyValuePairs()
        {
            var services = new ServiceCollection();

            services.AddKeyedScoped<IService, A>("a");
            services.AddKeyedScoped<IService, B>("b");
            services.AddTransient<Consumer>();

            var p = services.BuildDryIocServiceProvider();

            var consumer = p.GetRequiredService<Consumer>();
            var xs = consumer.Services.ToArray();
            Assert.AreEqual(2, xs.Length);

            Assert.AreEqual("a", xs[0].Key);
            Assert.IsInstanceOf<A>(xs[0].Value);

            Assert.AreEqual("b", xs[1].Key);
            Assert.IsInstanceOf<B>(xs[1].Value);
        }

        public interface IService { }
        public class A : IService { }
        public class B : IService { }
        public record Consumer(IEnumerable<KeyValuePair<object, IService>> Services) { }
    }
}
