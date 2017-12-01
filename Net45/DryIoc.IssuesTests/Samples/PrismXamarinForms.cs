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
    }
}
