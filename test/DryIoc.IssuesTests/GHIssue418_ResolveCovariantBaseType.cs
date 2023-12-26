using NUnit.Framework;
using System.Collections.Generic;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue418_ResolveCovariantBaseType : ITest
    {
        public int Run()
        {
            RegisterCovariantImplementationTypeWithMapping();
            RegisterCovariantInterfaceTypeWithMapping();
            RegisterCovariantListTypeDirect();
            RegisterClosedGenericImplementationAsOpenGeneric();
            RegisterCovariantImplementationTypeWithDelegete();
            return 5;
        }

        /// <summary>
        ///     Currently, resolving the list as <see cref="IReadOnlyList{BaseClass}" /> fails in this test.
        ///     The list is registered as its implementation and mapped to the interface.
        /// </summary>
        [Test]
        public void RegisterCovariantImplementationTypeWithMapping()
        {
            var container = new Container();
            
            // Register the implementation as itself
            container.Register(Made.Of(() => new List<SubClass>()));

            // Register the interface with base type parameter
            container.RegisterMapping<IReadOnlyList<BaseClass>, List<SubClass>>();

            // Resolving IReadOnlyList<BaseClass> crashes here
            var baseClassList = container.Resolve<IReadOnlyList<BaseClass>>();
            Assert.IsInstanceOf<List<SubClass>>(baseClassList);
        }

        /// <summary>
        ///     This works. The difference is that the list is registered as <see cref="IReadOnlyList{SubClass}"/> and
        ///     then it is mapped.
        /// </summary>
        [Test]
        public void RegisterCovariantInterfaceTypeWithMapping()
        {
            var container = new Container();
            container.Register(Made.Of<IReadOnlyList<SubClass>>(() => new List<SubClass>()));
            container.RegisterMapping<IReadOnlyList<BaseClass>, IReadOnlyList<SubClass>>();

            var baseClassList = container.Resolve<IReadOnlyList<BaseClass>>();
            Assert.IsInstanceOf<List<SubClass>>(baseClassList);
        }

        /// <summary>
        ///     This also works, because there is no mapping here.
        /// </summary>
        [Test]
        public void RegisterCovariantListTypeDirect()
        {
            var container = new Container();
            container.Register<IReadOnlyList<BaseClass>, List<SubClass>>(Made.Of(() => new List<SubClass>()));

            var baseClassList = container.Resolve<IReadOnlyList<BaseClass>>();
            Assert.IsInstanceOf<List<SubClass>>(baseClassList);
        }

        /// <summary>
        ///     This works, but the registration code is weird, because we assign a closed generic implementation to an open
        ///     generic service.
        /// </summary>
        [Test]
        public void RegisterClosedGenericImplementationAsOpenGeneric()
        {
            var container = new Container();
            container.Register(typeof(IReadOnlyList<>), typeof(List<SubClass>),
                               made: Made.Of(FactoryMethod.ConstructorWithResolvableArguments));

            var baseClassList = container.Resolve<IReadOnlyList<BaseClass>>();
            Assert.IsInstanceOf<List<SubClass>>(baseClassList);
        }

        /// <summary>
        ///     This is a workaround by using a delegate instead of a mapping.
        /// </summary>
        [Test]
        public void RegisterCovariantImplementationTypeWithDelegete()
        {
            var container = new Container();
            container.Register(Made.Of(() => new List<SubClass>()));
            container.RegisterDelegate<IReadOnlyList<BaseClass>>(c => c.Resolve<List<SubClass>>());

            var baseClassList = container.Resolve<IReadOnlyList<BaseClass>>();
            Assert.IsInstanceOf<List<SubClass>>(baseClassList);
        }

        private class BaseClass
        {
        }

        private class SubClass : BaseClass
        {
        }
    }
}
