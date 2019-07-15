using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests.Samples
{
    [TestFixture]
    public class PrismXamarinForms
    {
        [Test]
        public void Test()
        {
            const string navigationServiceName = "navigationService";

            var container = new Container();

            container.Register<INavigationService, PageNavigationService>(serviceKey: navigationServiceName);
            container.Register(
                Made.Of(() => SetPage(Arg.Of<INavigationService>(navigationServiceName), Arg.Of<Page>())),
                setup: Setup.Decorator);//..With(r => navigationServiceName.Equals(r.ServiceKey)));

            container.Register<ViewModel>(
                made: Parameters.Of.Type<INavigationService>(serviceKey: navigationServiceName));

            var getVM = container.Resolve<Func<Page, object>>(typeof(ViewModel));
            var result = getVM(new Page());

            Assert.IsNotNull(result);
        }

        internal static INavigationService SetPage(INavigationService navigationService, Page page)
        {
            var pageAware = navigationService as IPageAware;
            if (pageAware != null)
                pageAware.Page = page;

            return navigationService;
        }

        internal interface INavigationService { }

        internal class Page { }

        internal interface IPageAware
        {
            Page Page { get; set; }
        }

        internal class PageNavigationService : INavigationService, IPageAware
        {
            public Page Page { get; set; }
        }

        internal class ViewModel
        {
            public INavigationService NavService { get; private set; }
            public ViewModel(INavigationService navigationService)
            {
                NavService = navigationService;
            }
        }

        [Test]
        public void Replace_singleton_dependency_with_asResolutionCall()
        {
            var c = new Container(rules => rules.WithoutEagerCachingSingletonForFasterAccess());

            c.Register<Foo>();
            //c.Register<Foo>(Reuse.Singleton); // !!! If the consumer of replaced dependency is singleton, it won't work
                                                // cause the consumer singleton should be replaced too

            c.Register<IBar, Bar>(Reuse.Singleton,
                setup: Setup.With(asResolutionCall: true));        // required

            var foo = c.Resolve<Foo>();
            Assert.IsInstanceOf<Bar>(foo.Bar);

            c.Register<IBar, Bar2>(Reuse.Singleton,
                setup: Setup.With(asResolutionCall: true),         // required
                ifAlreadyRegistered: IfAlreadyRegistered.Replace); // required

            var foo2 = c.Resolve<Foo>();
            Assert.IsInstanceOf<Bar2>(foo2.Bar);
        }

        [Test]
        public void Replace_singleton_dependency_with_instance()
        {
            var c = new Container();

            c.Register<Foo>();
            //c.Register<Foo>(Reuse.Singleton); // !!! If the consumer of replaced dependency is singleton, it won't work
                                                // cause the consumer singleton should be replaced too
            c.RegisterInstance<IBar>(new Bar(), setup: Setup.With(asResolutionCall: true));
            var foo = c.Resolve<Foo>();
            Assert.IsInstanceOf<Bar>(foo.Bar);

            c.RegisterInstance<IBar>(new Bar2(), IfAlreadyRegistered.Replace);
            var foo2 = c.Resolve<Foo>();
            Assert.IsInstanceOf<Bar2>(foo2.Bar);
        }

        public class Foo
        {
            public IBar Bar { get; private set; }
            public Foo(IBar bar) { Bar = bar; }
        }

        public interface IBar {}
        public class Bar : IBar {}
        public class Bar2 : IBar { }
    }
}
