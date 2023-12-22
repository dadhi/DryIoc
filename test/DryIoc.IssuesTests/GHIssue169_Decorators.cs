using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue169_Decorators : ITest
    {
        public int Run()
        {
            Singleton_Decorator_should_be_correctly_resolved_in_Scoped_service_FEC();
            Singleton_Decorator_should_be_correctly_resolved_in_NamedScope_FEC();
            Transient_Decorator_should_be_correctly_resolved_in_NamedScope_FEC();
            Transient_Decorator_should_be_correctly_resolved_in_NamedScope_Interpreted();
            return 4;
        }

        [Test]
        public void Singleton_Decorator_should_be_correctly_resolved_in_Scoped_service_FEC()
        {
            var c = new Container();

            c.Register<ControllerA>(Reuse.Scoped);
            c.Register<ControllerB>(Reuse.Scoped);
            c.RegisterMany(
                new List<Type> { typeof(Repo), typeof(AService), typeof(BService) },
                Reuse.Singleton,
                serviceTypeCondition: s => s.IsInterface
            );

            // This is same test but Decorator is singleton
            c.Register<IRepository, RepoDecorator>(Reuse.Singleton, setup: Setup.Decorator);

            using (var scope = c.OpenScope())
            {
                var a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());

                var b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
            }

            using (var scope = c.OpenScope())
            {
                var a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());

                var b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
            }
        }

        [Test]
        public void Singleton_Decorator_should_be_correctly_resolved_in_NamedScope_FEC()
        {
            var c = new Container();

            c.Register<ControllerA>(Reuse.InWebRequest);
            c.Register<ControllerB>(Reuse.InWebRequest);
            c.RegisterMany(
                new[] { typeof(Repo), typeof(AService), typeof(BService) },
                Reuse.Singleton,
                serviceTypeCondition: s => s.IsInterface
            );

            // This is same test but Decorator is singleton
            c.Register<IRepository, RepoDecorator>(Reuse.Singleton, setup: Setup.Decorator);

            using (var scope = c.OpenScope(Reuse.WebRequestScopeName))
            {
                var a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());

                var b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
            }

            using (var scope = c.OpenScope(Reuse.WebRequestScopeName))
            {
                var a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());

                var b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
            }
        }

        [Test]
        public void Transient_Decorator_should_be_correctly_resolved_in_NamedScope_FEC()
        {
            var c = new Container();

            c.Register<ControllerA, ControllerA>(Reuse.InWebRequest);
            c.Register<ControllerB, ControllerB>(Reuse.InWebRequest);
            c.RegisterMany(
                new List<Type> { typeof(Repo), typeof(AService), typeof(BService) },
                Reuse.Singleton,
                serviceTypeCondition: s => s.IsInterface
            );

            c.Register<IRepository, RepoDecorator>(setup: Setup.Decorator);

            using (var scope = c.OpenScope(Reuse.WebRequestScopeName))
            {
                var a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());

                var b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
            }

            using (var scope = c.OpenScope(Reuse.WebRequestScopeName))
            {
                var a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());

                var b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
            }
        }

        [Test]
        public void Transient_Decorator_should_be_correctly_resolved_in_NamedScope_Interpreted()
        {
            var c = new Container(rules => rules.WithUseInterpretation());

            c.Register<ControllerA, ControllerA>(Reuse.InWebRequest);
            c.Register<ControllerB, ControllerB>(Reuse.InWebRequest);
            c.RegisterMany(
                new List<Type> { typeof(Repo), typeof(AService), typeof(BService) },
                Reuse.Singleton,
                serviceTypeCondition: s => s.IsInterface
            );

            c.Register<IRepository, RepoDecorator>(setup: Setup.Decorator);

            using (var scope = c.OpenScope(Reuse.WebRequestScopeName))
            {
                var a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());

                var b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
            }

            using (var scope = c.OpenScope(Reuse.WebRequestScopeName))
            {
                var a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<ControllerA>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());

                var b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<ControllerB>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
            }
        }

        public interface IRepository
        {
            string Identify();
        }

        public class Repo : IRepository
        {
            public string Identify() => "REPO";
        }

        public class RepoDecorator : IRepository
        {
            protected readonly IRepository repo;
            public RepoDecorator(IRepository repo) => this.repo = repo;
            public string Identify() => repo.Identify() + "_DECORATOR_";
        }

        public interface IAService
        {
            string Identify();
        }

        public interface IBService
        {
            string Identify();
        }

        public class AService : IAService
        {
            private readonly IRepository repo;
            public AService(IRepository repo) => this.repo = repo;
            public string Identify() => repo.Identify() + "_A_";
        }

        public class BService : IBService
        {
            private readonly IRepository repo;
            public BService(IRepository repo) => this.repo = repo;
            public string Identify() => repo.Identify() + "_B_";
        }

        public class ControllerB
        {
            private readonly IBService service;
            public ControllerB(IBService service) => this.service = service;
            public string Identify() => service.Identify() + "Controller2";
        }

        public class ControllerA
        {
            private readonly IAService service;
            public ControllerA(IAService service) => this.service = service;
            public string Identify() => service.Identify() + "Controller";
        }
    }
}