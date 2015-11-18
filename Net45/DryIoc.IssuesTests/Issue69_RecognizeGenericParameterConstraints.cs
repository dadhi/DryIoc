using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    class Issue69_RecognizeGenericParameterConstraints
    {
        [Test]
        public void Should_support_generic_parameters_constraints()
        {
            var container = new Container();
            container.Register(typeof(IGeneric<,,,>), typeof(GenericWithConstraints<,,,>));

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<IGeneric<SomeClass, string, ServiceDerivedClass, ServiceTDerivedClass<string>>>());

            Assert.AreEqual(Error.NoMatchedGenericParamConstraints, ex.Error);
        }

        internal interface IGeneric<T1, T2, T3, T4> { }

        internal class GenericWithConstraints<T1, T2, T3, T4> : IGeneric<T1, T2, T3, T4>
            where T1 : new()
            where T2 : struct
            where T3 : IService
            where T4 : IService<T1>
        { }

        internal interface IService { }

        internal interface IService<T>
        {
            T Dependency { get; }
        }

        internal class SomeClass
        {
            public SomeClass(IService blah) {}
        }

        internal struct SomeStruct { }

        internal class ServiceDerivedClass : IService { }

        internal class ServiceTDerivedClass<T> : IService<T>
        {
            public T Dependency { get; set; }
        }
    }
}
