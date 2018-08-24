using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue596_RegisterMany_with_Factory
    {
        [Test]
        public void Test()
        {
            var container = new Container();

            container.Register<INeedDependencyFactory, Factory>();

            container.RegisterMany(
                new[]
                {
                    typeof(IDependency),
                    typeof(IDependencyDepender)
                },
                typeof(Dependency));

            // doesn't work
            container.RegisterMany(
                new[]
                {
                    typeof(INeedDependency)
                },
                made: Made.Of(
                    r => ServiceInfo.Of<INeedDependencyFactory>(),
                    f => f.Create(Arg.Of<IDependency>())
                ),
                nonPublicServiceTypes: true);

            // works
            //container.RegisterMany(
            //    new[]
            //    {
            //        typeof(INeedDependency)
            //    },
            //    typeof(NeedDependency));

            // works
            //container.Register<INeedDependency>(made: Made.Of(
            //        r => ServiceInfo.Of<INeedDependencyFactory>(),
            //        f => f.Create(Arg.Of<IDependency>())));

            // lets try
            container.Resolve<INeedDependency>();
        }

        interface IDependencyDepender { }

        interface IDependency : IDependencyDepender { }

        class Dependency : IDependency { }

        interface INeedDependency { }

        class NeedDependency : INeedDependency
        {
            private readonly IDependency fDependency;

            public NeedDependency(IDependency aDependency) =>
                fDependency = aDependency;
        }

        interface INeedDependencyFactory
        {
            INeedDependency Create(IDependency aDependency);
        }

        class Factory : INeedDependencyFactory
        {
            public INeedDependency Create(IDependency aDependency) =>
                new NeedDependency(aDependency);
        }
    }
}
