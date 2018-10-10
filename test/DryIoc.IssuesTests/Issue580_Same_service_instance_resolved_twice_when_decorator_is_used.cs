using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue580_Same_service_instance_resolved_twice_when_decorator_is_used
    {
        [Test]
        public void Main()
        {
            var container = new Container();

            container.RegisterMany(new[] { typeof(IGenericService<>).GetAssembly() },
                z => z.IsInterface && z.IsConstructedGenericType
                    ? z.GetGenericTypeDefinition() == typeof(IGenericService<>)
                    : z == typeof(IGenericService<>),
                Reuse.Transient);

            container.Register<object>(
                made: Made.Of(r => typeof(DecoratorFactory)
                    .SingleMethod(nameof(DecoratorFactory.Decorate)).MakeGenericMethod(r.ServiceType)),
                setup: Setup.Decorator);

            var xs = container.ResolveMany<IGenericService<string>>().Select(x => x.GetType()).ToList();
            CollectionAssert.AreEquivalent(new[] { typeof(GenericOpenImpl<string>), typeof(GenericClosedImpl) }, xs);

            var xs2 = container.ResolveMany<IGenericService<string>>().Select(x => x.GetType()).ToList();
            CollectionAssert.AreEquivalent(new[] { typeof(GenericOpenImpl<string>), typeof(GenericClosedImpl) }, xs2);
        }

        static class DecoratorFactory
        {
            public static T Decorate<T>(T service) { return service; }
        }

        public interface IGenericService<T> { }
        public class GenericOpenImpl<T> : IGenericService<T> { }
        public class GenericClosedImpl : IGenericService<string> { }
    }
}
