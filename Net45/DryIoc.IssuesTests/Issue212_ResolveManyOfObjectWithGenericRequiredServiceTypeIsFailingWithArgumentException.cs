using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue212_ResolveManyOfObjectWithGenericRequiredServiceTypeIsFailingWithArgumentException
    {
        [Test]
        public void Generic_with_non_matching_constraints_should_be_skipped_in_result_collection()
        {
            var container = new Container();

            container.Register(typeof(IGenericService<>), typeof(GenericService<>));
            container.Register(typeof(IGenericService<>), typeof(GenericServiceWithIService2Constraint<>));

            var resolved = container.ResolveMany<object>(typeof(IGenericService<IndependentService>)).ToArray();

            Assert.AreEqual(1, resolved.Length);
            Assert.IsInstanceOf<GenericService<IndependentService>>(resolved[0]);
        }

        [Test]
        public void Generic_with_non_matching_constraints_should_be_skipped_in_result_array()
        {
            var container = new Container();

            container.Register(typeof(IGenericService<>), typeof(GenericService<>));
            container.Register(typeof(IGenericService<>), typeof(GenericServiceWithIService2Constraint<>));

            var resolved = container.ResolveMany<object>(typeof(IGenericService<IndependentService>), ResolveManyBehavior.AsFixedArray).ToArray();

            Assert.AreEqual(1, resolved.Length);
            Assert.IsInstanceOf<GenericService<IndependentService>>(resolved[0]);
        }

        public interface IService { }

        public interface IGenericService<T> { }

        public class GenericService<T> : IGenericService<T> { }

        public interface IService2 { }

        public class GenericServiceWithIService2Constraint<T> : IGenericService<T> where T : IService2 { }

        public class IndependentService : IService { }

    }
}
