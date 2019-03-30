using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue100_ResolveMany_with_Meta_does_NOT_work
    {
        [Test]
        public void Test()
        {
            var c = new Container();

            c.Register<IMany, A>(setup: Setup.With(metadataOrFuncOfMetadata: MetaKey.X));
            c.Register<IMany, B>();
            c.Register<IMany, C>(setup: Setup.With(metadataOrFuncOfMetadata: MetaKey.X));

            var ms2 = c.ResolveMany<Meta<IMany, MetaKey>>().ToList();
            Assert.AreEqual(2, ms2.Count);

            var ms1 = c.Resolve<IList<Meta<IMany, MetaKey>>>();
            Assert.AreEqual(2, ms2.Count);
        }

        public interface IMany { }
        public class A : IMany { }
        public class B : IMany { }
        public class C : IMany { }

        public enum MetaKey { X }
    }
}
