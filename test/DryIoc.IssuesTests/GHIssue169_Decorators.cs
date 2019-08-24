using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [Ignore("fix me")]
    [TestFixture]
    public class GHIssue169_Decorators
    {
        [Test]
        public void Decorator_should_be_correctly_resolved_FEC_SingletonDecorator()
        {
            var c = new Container();

            c.Register<Controller, Controller>(Reuse.InWebRequest);
            c.Register<Controller2, Controller2>(Reuse.InWebRequest);
            c.RegisterMany(
                new List<Type> { typeof(Repo), typeof(AService), typeof(BService) },
                Reuse.Singleton,
                serviceTypeCondition: s => s.IsInterface
            );

            // This is same test but Decorator is singleton
            c.Register<IRepository, RepoDecorator>(Reuse.Singleton, setup: Setup.Decorator,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);


            using (var scope = c.OpenScope(Reuse.WebRequestScopeName))
            {
                var a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());

                var b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
            }

            using (var scope = c.OpenScope(Reuse.WebRequestScopeName))
            {
                var a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());

                var b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
            }
        }

        [Test]
        public void Decorator_should_be_correctly_resolved_FEC()
        {
            var c = new Container();

            c.Register<Controller, Controller>(Reuse.InWebRequest);
            c.Register<Controller2, Controller2>(Reuse.InWebRequest);
            c.RegisterMany(
                new List<Type> {typeof(Repo), typeof(AService), typeof(BService)},
                Reuse.Singleton,
                serviceTypeCondition: s => s.IsInterface
            );

            c.Register<IRepository, RepoDecorator>(setup: Setup.Decorator,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);


            using (var scope = c.OpenScope(Reuse.WebRequestScopeName))
            {
                var a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());

                var b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
            }

            using (var scope = c.OpenScope(Reuse.WebRequestScopeName))
            {
                var a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());

                var b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
            }
        }

        [Test]
        public void Decorator_should_be_correctly_resolved_Interpret()
        {
            var c = new Container(rules => rules.WithUseInterpretation());

            c.Register<Controller, Controller>(Reuse.InWebRequest);
            c.Register<Controller2, Controller2>(Reuse.InWebRequest);
            c.RegisterMany(
                new List<Type> {typeof(Repo), typeof(AService), typeof(BService)},
                Reuse.Singleton,
                serviceTypeCondition: s => s.IsInterface
            );

            c.Register<IRepository, RepoDecorator>(setup: Setup.Decorator,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);


            using (var scope = c.OpenScope(Reuse.WebRequestScopeName))
            {
                var a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());

                var b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
            }

            using (var scope = c.OpenScope(Reuse.WebRequestScopeName))
            {
                var a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());

                var b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
            }
        }

        [Test]
        public void Decorator_should_be_correctly_resolved_withoutFEC()
        {
            var c = new Container(rules => rules.WithoutFastExpressionCompiler());

            c.Register<Controller, Controller>(Reuse.InWebRequest);
            c.Register<Controller2, Controller2>(Reuse.InWebRequest);
            c.RegisterMany(
                new List<Type> {typeof(Repo), typeof(AService), typeof(BService)},
                Reuse.Singleton,
                serviceTypeCondition: s => s.IsInterface
            );

            c.Register<IRepository, RepoDecorator>(setup: Setup.Decorator,
                ifAlreadyRegistered: IfAlreadyRegistered.Throw);


            using (var scope = c.OpenScope(Reuse.WebRequestScopeName))
            {
                var a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());

                var b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
            }

            using (var scope = c.OpenScope(Reuse.WebRequestScopeName))
            {
                var a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());
                a = scope.Resolve<Controller>();
                Assert.AreEqual("REPO_DECORATOR__A_Controller", a.Identify());

                var b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<Controller2>();
                Assert.AreEqual("REPO_DECORATOR__B_Controller2", b.Identify());
                b = scope.Resolve<Controller2>();
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

            public RepoDecorator(IRepository repo)
            {
                this.repo = repo;
            }

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

            public AService(IRepository repo)
            {
                this.repo = repo;
            }

            public string Identify() => repo.Identify() + "_A_";
        }

        public class BService : IBService
        {
            private readonly IRepository repo;

            public BService(IRepository repo)
            {
                this.repo = repo;
            }

            public string Identify() => repo.Identify() + "_B_";
        }

        public class Controller2
        {
            private readonly IBService service;

            public Controller2(IBService service)
            {
                this.service = service;
            }

            public string Identify() => service.Identify() + "Controller2";
        }

        public class Controller
        {
            private readonly IAService service;

            public Controller(IAService service)
            {
                this.service = service;
            }

            public string Identify() => service.Identify() + "Controller";
        }
    }
}