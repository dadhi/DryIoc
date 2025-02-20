using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DryIoc.ImTools;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue473_Unable_to_match_service_with_open_generic : ITest
    {
        public int Run()
        {
            Unable_to_match_in_ResolveMany();
            Able_to_match_in_ResolveMany();
            return 2;
        }

        [Test]
        public void Unable_to_match_in_ResolveMany()
        {
            var container = new Container();
            container.RegisterMany(new[] { typeof(MyDictionary<>) });

            var array = container.ResolveMany(typeof(IMyInterface), ResolveManyBehavior.AsFixedArray);
            Assert.IsEmpty(array);

            array = container.ResolveMany(typeof(IMyInterface)).ToArray();
            Assert.IsEmpty(array);
        }

        [Test]
        public void Able_to_match_in_ResolveMany()
        {
            var container = new Container();
            container.RegisterMany(new[] { typeof(MyDictionary<>) });
            container.RegisterMany<MyClass>();

            var array = container.ResolveMany(typeof(IMyInterface), ResolveManyBehavior.AsFixedArray).ToArrayOrSelf();
            Assert.IsInstanceOf<MyClass>(array[0]);

            array = container.ResolveMany(typeof(IMyInterface)).ToArray();
            Assert.IsInstanceOf<MyClass>(array[0]);
        }

        public class MyDictionary<T> : IEnumerable<KeyValuePair<string, T>>
        {
            public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        public interface IMyInterface { }

        public class MyClass : IMyInterface { }
    }
}
