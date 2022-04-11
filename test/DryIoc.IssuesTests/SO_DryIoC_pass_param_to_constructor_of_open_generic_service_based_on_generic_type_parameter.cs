using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class SO_DryIoC_pass_param_to_constructor_of_open_generic_service_based_on_generic_type_parameter
    {
        [Test]
        public void Test()
        {
            var container = new Container();

            container.Register(typeof(ISomeService<>), typeof(SomeService<>), Reuse.Transient,
                made: Parameters.Of.Details((req, p) => 
                    p.ParameterType.GetGenericDefinitionOrNull() == typeof(IStrategy<>) && 
                    p.ParameterType.GetGenericArguments().Any(x => x.IsAssignableTo<IFoo>())
                    ? null                              // the default behavior 
                    : ServiceDetails.Of(value: null))   // otherwise return the `null` value
                );

            container.Register<ICurrentDbContext, MyDbContext>();
            container.Register(typeof(IStrategy<>), typeof(DefaultStrategy<>));

            var s1 = container.Resolve<ISomeService<OtherEntity>>();
            Assert.IsNull(((SomeService<OtherEntity>)s1).Delete);

            var s2 = container.Resolve<ISomeService<FooEntity>>();
            Assert.IsNotNull(((SomeService<FooEntity>)s2).Delete);
        }

        public interface ISomeService<TEntity> {}
        public class SomeService<TEntity> : ISomeService<TEntity>
        {
            public readonly IStrategy<TEntity> Delete;
            public SomeService(ICurrentDbContext context, IStrategy<TEntity> delete = null) => Delete = delete;
        }

        public interface IFoo {}
        public class FooEntity : IFoo {}

        public class OtherEntity {}

        public interface IStrategy<TEntity> {}
        public class DefaultStrategy<TEntity> : IStrategy<TEntity> { }
        public class CustomStrategy<TEntity> : IStrategy<TEntity> { }

        public interface ICurrentDbContext {}
        public class MyDbContext : ICurrentDbContext {}
    }
}
