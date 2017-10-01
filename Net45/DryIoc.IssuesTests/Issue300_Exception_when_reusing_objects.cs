using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue300_Exception_when_reusing_objects
    {
        [Test]
        public void Test()
        {
            var factory = new Container(rules => rules.WithDefaultReuse(Reuse.InCurrentScope));

            factory.Register<IContractRepository, ContractRepository>();
            factory.Register<IRepositoryEntity<ntw_contract>, ContractRepository>();
            factory.Register<IPPERepository, PPERepository>();
            factory.Register<IRepositoryEntity<ntw_ppe>, PPERepository>();

            var provider = new Provider();
            factory.RegisterInitializer<object>(
                (serviceObj, resolver) =>
                {
                    if (serviceObj is IRepositoryEntity)
                    {
                        (serviceObj as IRepositoryEntity).Initialize(provider);
                    }
                });

            //factory.Register<Provider>(Reuse.Singleton);
            //factory.Register<object>(
            //    made: Made.Of(GetType().GetSingleMethodOrNull(nameof(InitRepoEntity))),
            //    setup: Setup.DecoratorWith(r => r.ImplementationType.IsAssignableTo(typeof(IRepositoryEntity))));

            using (var scope = factory.OpenScope())
            {
                var enRep11 = scope.Resolve<IContractRepository>(IfUnresolved.Throw);
                var enRep21 = scope.Resolve<IRepositoryEntity<ntw_contract>>(IfUnresolved.Throw);
                var enRep31 = scope.Resolve<IRepositoryEntity<ntw_ppe>>(IfUnresolved.Throw); // < ---Here the exception is risen
            }   
        }

        public static T InitRepoEntity<T>(T repoEntity, Provider provider) where T : IRepositoryEntity
        {
            repoEntity.Initialize(provider);
            return repoEntity;
        }

        public interface IContractRepository { }

        public class ContractRepository : IContractRepository, IRepositoryEntity<ntw_contract> {
            public void Initialize(Provider provider)
            {
            }
        }

        public interface IRepositoryEntity {
            void Initialize(Provider provider);
        }
        public interface IRepositoryEntity<T> : IRepositoryEntity { }

        public class ntw_contract { }
        public class IPPERepository { }
        public class PPERepository : IPPERepository, IRepositoryEntity<ntw_ppe> {
            public void Initialize(Provider provider)
            {
                
            }
        }
        public class ntw_ppe { }
        public class Provider { }
    }

}
