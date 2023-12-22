using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue_HandleVariance : ITest
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

            container.Register(typeof(IContain<>), typeof(ContainBirdBase<>));
            container.Register(typeof(IContain<Bird>), typeof(ContainBird));

            var birds = container.ResolveMany<IContain<Bird>>().ToList();
            Assert.AreEqual(1, birds.Count());

            var birdBases = container.ResolveMany<IContain<BirdBase<string>>>();
            Assert.AreEqual(1, birdBases.Count());

            var birdsArray = container.Resolve<IContain<Bird>[]>();
            Assert.AreEqual(1, birdsArray.Length);

            var birdBasesArray = container.Resolve<IContain<BirdBase<string>>[]>();
            Assert.AreEqual(1, birdBasesArray.Length);
        }

        public interface IContain<in T> { }
        public class BirdBase<T> { }
        public class Bird : BirdBase<string> { } // IBird<string>
        public class ContainBird : IContain<Bird> { }
        public class ContainBirdBase<T> : IContain<BirdBase<T>> { }
    }
}
