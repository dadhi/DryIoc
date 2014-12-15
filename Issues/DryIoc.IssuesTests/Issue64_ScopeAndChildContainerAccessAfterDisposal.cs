using System;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using Xunit;

namespace DryIoc.IssuesTests
{
    public class Issue64_ScopeAndChildContainerAccessAfterDisposal
    {
        [Fact]
        public async Task ChildContainerAccessAfterDisposalShouldThrow()
        {
            await ThrowsAsync<ContainerException>(async () =>
            {
                var c = new Container();
                c.Register<XProvider>(Reuse.Singleton);

                Task t;
                using (var b = c.CreateChildContainer())
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

        [Fact]
        public async Task ShouldThrowAfterScopeDisposal()
        {
            await ThrowsAsync<ContainerException>(async () =>
            {
                var c = new Container();
                c.Register<XProvider>(Reuse.Singleton);

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

        [Fact]
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
                        b.ResolvePropertiesAndFields(provider);
                    });

                }
                await t;
            });
        }

        [Fact]
        public async Task ChildContainerAccessAfterDisposalShouldThrow_ForNamedRegistration()
        {
            await ThrowsAsync<ContainerException>(async () =>
            {
                var c = new Container();
                c.Register<XProvider>(Reuse.Singleton, named: 1);

                Task t;
                object result = null;
                using (var b = c.CreateChildContainer())
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

        public sealed class ExecutionFlowScopeContext : IScopeContext
        {
            public static readonly object ROOT_SCOPE_NAME = typeof(ExecutionFlowScopeContext);

            public object RootScopeName { get { return ROOT_SCOPE_NAME; } }

            public IScope GetCurrentOrDefault()
            {
                var scope = (Remote<IScope>)CallContext.LogicalGetData(_key);
                return scope == null ? null : scope.Value;
            }

            public void SetCurrent(Func<IScope, IScope> update)
            {
                var oldScope = GetCurrentOrDefault();
                var newScope = update.ThrowIfNull()(oldScope);
                CallContext.LogicalSetData(_key, new Remote<IScope>(newScope));
            }

            private static readonly string _key = typeof(ExecutionFlowScopeContext).Name;
        }

        public sealed class Remote<T> : MarshalByRefObject
        {
            public readonly T Value;

            public Remote(T value)
            {
                Value = value;
            }
        }

    }
}
