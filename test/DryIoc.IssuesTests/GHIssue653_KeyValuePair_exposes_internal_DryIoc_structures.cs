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
            Resolve_collection_of_KeyValuePairs_with_object_key();
            Resolve_collection_of_KeyValuePairs_with_string_key();
            Resolve_and_filter_collection_of_KeyValuePairs_with_the_same_int_keys();
            Resolve_collection_of_open_generic_services_with_string_keys();
            Resolve_collection_of_open_generic_services_with_same_type_same_string_keys();
            Resolve_collection_of_open_generic_services_with_int_keys_Meta_values();

            return 6;
        }

        [Test]
        public void Resolve_collection_of_KeyValuePairs_with_object_key()
        {
            var services = new ServiceCollection();

            services.AddKeyedScoped<IService, A>("a");
            services.AddKeyedScoped<IService, B>("b");
            services.AddTransient<ConsumerObjectKeys>();

            var p = services.BuildDryIocServiceProvider();

            var consumer = p.GetRequiredService<ConsumerObjectKeys>();
            var xs = consumer.Services.ToArray();
            Assert.AreEqual(2, xs.Length);

            Assert.AreEqual("a", xs[0].Key);
            Assert.IsInstanceOf<A>(xs[0].Value);

            Assert.AreEqual("b", xs[1].Key);
            Assert.IsInstanceOf<B>(xs[1].Value);
        }

        [Test]
        public void Resolve_collection_of_KeyValuePairs_with_string_key()
        {
            var services = new ServiceCollection();

            services.AddKeyedScoped<IService, A>("a");
            services.AddKeyedScoped<IService, B>("b");
            services.AddTransient<ConsumerStringKeys>();

            var p = services.BuildDryIocServiceProvider();

            var consumer = p.GetRequiredService<ConsumerStringKeys>();
            var xs = consumer.Services.ToArray();
            Assert.AreEqual(2, xs.Length);

            Assert.AreEqual("a", xs[0].Key);
            Assert.IsInstanceOf<A>(xs[0].Value);

            Assert.AreEqual("b", xs[1].Key);
            Assert.IsInstanceOf<B>(xs[1].Value);
        }

        [Test]
        public void Resolve_and_filter_collection_of_KeyValuePairs_with_the_same_int_keys()
        {
            var services = new ServiceCollection();

            services.AddKeyedScoped<IService, A>(42);
            services.AddKeyedScoped<IService, A>("a");
            services.AddKeyedScoped<IService, B>(42);
            services.AddTransient<ConsumerIntKeys>();

            var p = services.BuildDryIocServiceProvider();

            var consumer = p.GetRequiredService<ConsumerIntKeys>();
            var xs = consumer.Services.ToArray();
            Assert.AreEqual(2, xs.Length);

            Assert.AreEqual(42, xs[0].Key);
            Assert.IsInstanceOf<A>(xs[0].Value);

            Assert.AreEqual(42, xs[1].Key);
            Assert.IsInstanceOf<B>(xs[1].Value);
        }

        [Test]
        public void Resolve_collection_of_open_generic_services_with_string_keys()
        {
            var services = new ServiceCollection();

            services.AddKeyedScoped(typeof(IService<>), "a", typeof(A<>));
            services.AddKeyedScoped(typeof(IService<>), 42, typeof(C<>));
            services.AddKeyedScoped(typeof(IService<>), "b", typeof(B<>));
            services.AddScoped(typeof(ConsumerStringKeys<>));

            var p = services.BuildDryIocServiceProvider();

            var consumer = p.GetRequiredService<ConsumerStringKeys<int>>();
            var xs = consumer.Services.ToArray();
            Assert.AreEqual(2, xs.Length);

            Assert.AreEqual("a", xs[0].Key);
            Assert.IsInstanceOf<A<int>>(xs[0].Value);

            Assert.AreEqual("b", xs[1].Key);
            Assert.IsInstanceOf<B<int>>(xs[1].Value);
        }

        [Test]
        public void Resolve_collection_of_open_generic_services_with_same_type_same_string_keys()
        {
            var services = new ServiceCollection();

            services.AddKeyedSingleton(typeof(IService<>), "a", typeof(A<>));
            services.AddKeyedSingleton(typeof(IService<>), "a", typeof(A<>));
            services.AddTransient(typeof(ConsumerStringKeys<>));

            var p = services.BuildDryIocServiceProvider();

            var consumer = p.GetRequiredService<ConsumerStringKeys<int>>();
            var xs = consumer.Services.ToArray();
            Assert.AreEqual(2, xs.Length);

            Assert.AreEqual("a", xs[0].Key);
            Assert.IsInstanceOf<A<int>>(xs[0].Value);

            Assert.AreEqual("a", xs[1].Key);
            Assert.IsInstanceOf<A<int>>(xs[1].Value);

            Assert.AreNotSame(xs[0].Value, xs[1].Value);
        }

        [Test]
        public void Resolve_collection_of_open_generic_services_with_int_keys_Meta_values()
        {
            var services = new ServiceCollection();
            services.AddSingleton(typeof(ConsumerStringKeysMetaValues<>));

            var p = services.BuildDryIocServiceProvider();
            var c = p.Container;
            c.Register(typeof(IService<>), typeof(A<>), serviceKey: "a", setup: Setup.With(metadataOrFuncOfMetadata: "foo"));
            c.Register(typeof(IService<>), typeof(A<>), serviceKey: "a", setup: Setup.With(metadataOrFuncOfMetadata: "bar"));

            var consumer = p.GetRequiredService<ConsumerStringKeysMetaValues<int>>();
            var xs = consumer.Services.ToArray();
            Assert.AreEqual(2, xs.Length);

            Assert.AreEqual("a", xs[0].Key);
            Assert.IsInstanceOf<A<int>>(xs[0].Value.Value);
            Assert.AreEqual("foo", xs[0].Value.Metadata);

            Assert.AreEqual("a", xs[1].Key);
            Assert.IsInstanceOf<A<int>>(xs[1].Value.Value);
            Assert.AreEqual("bar", xs[1].Value.Metadata);
        }

        public interface IService { }
        public class A : IService { }
        public class B : IService { }
        public record ConsumerObjectKeys(IEnumerable<KeyValuePair<object, IService>> Services) { }
        public record ConsumerStringKeys(IEnumerable<KeyValuePair<string, IService>> Services) { }
        public record ConsumerIntKeys(IEnumerable<KeyValuePair<int, IService>> Services) { }
        public interface IService<T> { }
        public class A<T> : IService<T> { }
        public class B<T> : IService<T> { }
        public class C<T> : IService<T> { }
        public record ConsumerStringKeys<T>(IEnumerable<KeyValuePair<string, IService<T>>> Services) { }
        public record ConsumerStringKeysMetaValues<T>(IEnumerable<KeyValuePair<string, Meta<IService<T>, string>>> Services) { }
    }
}
