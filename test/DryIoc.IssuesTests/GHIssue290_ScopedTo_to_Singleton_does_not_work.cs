using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue290_ScopedTo_to_Singleton_does_not_work
    {
        [Test]
        public void Test()
        {
            var container = new Container();
            container.Register<IDep, Dep1>(Reuse.ScopedTo<Owner1>());
            container.Register<IDep, Dep2>(Reuse.ScopedTo<Owner2>());

            container.RegisterMany<Owner1>(Reuse.Singleton, setup: Setup.With(openResolutionScope: true));
            container.RegisterMany<Owner2>(Reuse.Singleton, setup: Setup.With(openResolutionScope: true));

            var owner1 = container.Resolve<Owner1>();
            var owner2 = container.Resolve<Owner2>();
            Assert.AreNotSame(owner1.Dep, owner2.Dep);
            Assert.IsInstanceOf<Dep1>(owner1.Dep);
            Assert.IsInstanceOf<Dep2>(owner2.Dep);
        }

        public interface IDep { }

        public interface IOwner
        {
            IDep Dep { get; }
        }

        public class Dep1 : IDep { }
        public class Dep2 : IDep { }

        public class Owner1 : IOwner
        {
            public Owner1(IDep dep)
            {
                Dep = dep;
            }

            public IDep Dep { get; }
        }

        public class Owner2 : IOwner
        {
            public Owner2(IDep dep)
            {
                Dep = dep;
            }

            public IDep Dep { get; }
        }
    }
}
