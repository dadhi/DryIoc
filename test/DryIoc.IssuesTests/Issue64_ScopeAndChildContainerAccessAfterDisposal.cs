using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    public class Issue64_ScopeAndChildContainerAccessAfterDisposal : ITest
    {
        public int Run()
        {
            ChildContainerAccessAfterDisposalShouldThrow().GetAwaiter().GetResult();
            ShouldThrowAfterScopeDisposal().GetAwaiter().GetResult();
            ShouldThrowAfterScopeDisposal_WhenPropertiesAreResolved().GetAwaiter().GetResult();
            ChildContainerAccessAfterDisposalShouldThrow_ForNamedRegistration().GetAwaiter().GetResult();
            return 4;
        }

        [Test]
        public async Task ChildContainerAccessAfterDisposalShouldThrow()
        {
            await ThrowsAsync<ContainerException>(async () =>
            {
                var c = new Container();
                c.Register<XProvider>(Reuse.Singleton);

                Task t;
                using (var b = c.CreateFacade())
                {

                    t = Task.Run(async () =>
                    {
                        await Task.Delay(100);
                        b.Resolve<XProvider>();
                    });

                }
                await t;
            });
        }

        [Test]
        public async Task ShouldThrowAfterScopeDisposal()
        {
            await ThrowsAsync<ContainerException>(async () =>
            {
                var c = new Container();
                c.Register<XProvider>(Reuse.InCurrentScope);

                Task t;
                using (var b = c.OpenScope())
                {

                    t = Task.Run(async () =>
                    {
                        await Task.Delay(100);
                        b.Resolve<XProvider>();
                    });

                }
                await t;
            });
        }

        [Test]
        public async Task ShouldThrowAfterScopeDisposal_WhenPropertiesAreResolved()
        {
            await ThrowsAsync<ContainerException>(async () =>
            {
                var c = new Container();
                c.Register<YGoodness>(Reuse.InCurrentScope);

                Task t;
                using (var b = c.OpenScope())
                {
                    t = Task.Run(async () =>
                    {
                        await Task.Delay(100);
                        var provider = new XProvider();
                        b.InjectPropertiesAndFields(provider);
                    });

                }
                await t;
            });
        }

        [Test]
        public async Task ChildContainerAccessAfterDisposalShouldThrow_ForNamedRegistration()
        {
            await ThrowsAsync<ContainerException>(async () =>
            {
                var c = new Container();
                c.Register<XProvider>(Reuse.Singleton, serviceKey: 1);

                Task t;
                object result = null;
                using (var b = c.CreateFacade())
                {

                    t = Task.Run(async () =>
                    {
                        await Task.Delay(100);
                        result = b.Resolve<XProvider>(1);
                    });

                }
                await t;
            });
        }

        public async static Task<T> ThrowsAsync<T>(Func<Task> testCode) where T : Exception
        {
            try
            {
                await testCode();
                Assert.Throws<T>(() => { }); // Use xUnit's default behavior.
            }
            catch (T exception)
            {
                return exception;
            }
            return null;
        }

        public class XProvider
        {
            public YGoodness Goodness { get; set; }
        }

        public class YGoodness { }
    }
}
