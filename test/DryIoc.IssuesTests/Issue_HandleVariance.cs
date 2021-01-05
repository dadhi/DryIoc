using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue_HandleVariance
    {
        [Test, Explicit] // todo: @bug? check why it is failing
        public void CommandHandlers_CanBeResolved_From_IoC()
        {
            var container = new Container();

            container.Register(typeof(IBird<>), typeof(BirdBaseImpl<>));
            container.Register(typeof(IBird<Bird>), typeof(BirdImpl));

            var services = container.ResolveMany<IBird<Bird>>();
            Assert.AreEqual(2, services.Count());

            var servicesArray = container.Resolve<IBird<Bird>[]>();
            Assert.AreEqual(2, servicesArray.Length);
        }

        public interface IBird<in T> { }
        public class BirdBase<T> { }
        public class Bird : BirdBase<string> { } // IBird<string>
        public class BirdImpl : IBird<Bird> { }
        public class BirdBaseImpl<T> : IBird<BirdBase<T>> { }
    }
}
