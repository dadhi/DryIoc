using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue369_Child_container_and_openResolutionScope_in_one_test : ITest
    {
        public int Run()
        {
            Should_resolve_context_from_the_new_scope();
            return 1;
        }

        public interface IServiceLocator
        {
            IResolverContext Resolver { get; }
        }

        public class DryIocServiceLocator : IServiceLocator
        {
            public DryIocServiceLocator(IResolverContext resolver)
            {
                Resolver = resolver;
            }

            public IResolverContext Resolver { get; }
        }


        public interface IContext
        {
            Session Session { get; }
        }


        public class Context : IContext
        {

            public Context(Session session)
            {
                Session = session;
            }

            public Session Session { get; }
        }
        
        public class Session
        {
            public int Code { get; set; }
            public Session()
            {
                Code = new System.Random().Next(99999999);
            }
        }

        [Test]
        public void Should_resolve_context_from_the_new_scope()
        {
            var container = new Container();

            container.Register<IServiceLocator, DryIocServiceLocator>();
            container.RegisterInstance<Session>(new Session());

            using (var nested = container.CreateChild(ifAlreadyRegistered: IfAlreadyRegistered.Replace))
            {
                nested.RegisterInstance<Session>(new Session());

                using (var inner = nested.OpenScope())
                {
                    var session1 = nested.Resolve<Session>();
                    var session2 = nested.Resolve<IServiceLocator>().Resolver.Resolve<Session>();

                    Assert.AreSame(session2, session1);
                }
            }
        }
    }
}