using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue_SupportForDynamicKeyword
    {
        [Test]
        public void Can_resolve_collection_of_dynamics()
        {
            var container = new Container();

            container.Register<I, X>();
            container.Register<I, Y>();

            var xy = container.Resolve<IEnumerable<dynamic>>(typeof(I));

            Assert.IsTrue(xy.Any());
        }

        [Test]
        public void Can_inject_collection_of_dynamics()
        {
            var container = new Container();

            container.Register<I, X>();
            container.Register<I, Y>();
            container.Register(Made.Of(() => new Z(Arg.Of<IEnumerable<dynamic>, I>())));

            var z = container.Resolve<Z>();
            Assert.IsTrue(z.XY.Any());
        }

        public interface I { }

        public class X : I { }

        public class Y : I { }

        public class Z
        {
            public readonly IEnumerable<dynamic> XY;

            public Z(IEnumerable<dynamic> xy)
            {
                XY = xy;
            }
        }
    }
}
