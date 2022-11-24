using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue546_Generic_type_constraint_resolution_doesnt_see_arrays_as_IEnumerable : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test][Ignore("todo: fixme")]
        public void Test()
        {
            var container = new Container();

            container.Register(typeof(IMyThing<>), typeof(MyThing<,>), Reuse.ScopedOrSingleton);

            // Works - constraint of IEnumerable<TValue> brings int from List<int> into the 2nd generic parameter.
            var x = container.Resolve<IMyThing<List<int>>>();
            Assert.IsInstanceOf<MyThing<List<int>, int>>(x);

            // Works - type is compliant with the generic constraints - would not compile otherwise.
            var q = new MyThing<int[], int>();

            // Fails when we try to resolve it through the container
            var y = container.Resolve<IMyThing<int[]>>();
            Assert.IsInstanceOf<MyThing<int[], int>>(y);
        }

        public interface IMyThing<T> { }
        public interface IMyThing<T, TValue> : IMyThing<T>
            where T : IEnumerable<TValue> { }

        public class MyThing<T, TValue> : IMyThing<T, TValue>
            where T : IEnumerable<TValue> { }
    }
}
