using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue473_Unable_to_match_service_with_open_generic
    {
        [Test, Ignore("open issue, not sure does it need to be fixed")]
        public void Unable_to_match_in_ResolveMany_of_array()
        {
            var container = new Container();
            container.RegisterMany(new[] { typeof(MyDictionary<>) });

            var array = container.ResolveMany(typeof(IMyInterface), ResolveManyBehavior.AsFixedArray);
            Assert.IsEmpty(array);
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

        public interface IMyInterface {}
    }
}
