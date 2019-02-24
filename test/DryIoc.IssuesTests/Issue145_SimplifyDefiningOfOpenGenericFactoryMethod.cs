using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel;
using DryIocAttributes;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue145_SimplifyDefiningOfOpenGenericFactoryMethod
    {
        [Test]
        public void Register_manually()
        {
            var container = new Container();
            container.Register(typeof(Factory<>));
            container.Register(typeof(IService<,>), 
                made: Made.Of(typeof(Factory<>).SingleMethod(nameof(Factory<object>.Create)), 
                ServiceInfo.Of(typeof(Factory<>))));

            container.RegisterMany(new [] { typeof(Foo) });

            container.Resolve<IService<Foo, string>>();
        }

        public class Factory<A>
        {
            [Export]
            public IService<A, B> Create<B>(A a)
            {
                var service = new ServiceImpl<A, B>();
                service.Initialize(a);
                return service;
            }
        }

        public class Foo { }
        public class Bar { }

        public interface IService<T, R> {}

        private class ServiceImpl<T, R> : IService<T, R>
        {
            public void Initialize(T t) { }
        }

        [Test]
        public void Should_map_nested_generic_parameters()
        {
            var container = new Container();
            container.RegisterExports(typeof(CFactory));

            var result = container.Resolve<I<List<int>, Tuple<string, long>, long[]>>();

            Assert.IsInstanceOf<C<long, int>>(result);
        }

        [Test]
        public void Should_map_repeated_generic_parameters()
        {
            var container = new Container();
            container.RegisterExports(typeof(DFactory));

            var d = container.Resolve<I<int, int, int>>();
            Assert.IsInstanceOf<D<int>>(d);
        }

        [Test]
        public void Should_map_repeated_generic_parameters_and_other_parameters()
        {
            var container = new Container();
            container.RegisterExports(typeof(EFactory));

            var e = container.Resolve<I<int, string, int>>();
            Assert.IsInstanceOf<E<string, int>>(e);
        }

        [Test]
        public void Should_map_static_open_generic_class_with_generic_factory_method()
        {
            var container = new Container();
            container.RegisterExports(typeof(X<>));

            container.RegisterInstance<int>(1);
            var y = container.Resolve<Y<int, double>>();

            Assert.AreEqual(1, y.Blah);
        }

        [Test]
        public void Should_map_open_generic_class_with_generic_factory_method()
        {
            var container = new Container();
            container.RegisterExports(typeof(Z<>));

            container.RegisterInstance<string>("1");
            var y = container.Resolve<Y<string, string>>();

            Assert.AreEqual("1", y.Blah);
        }

        [Test]
        public void Should_throw_when_mapping_service_with_incompatible_type_arguments()
        {
            var container = new Container();
            container.RegisterExports(typeof(X<>));

            var ex = Assert.Throws<ContainerException>(() => 
                container.Resolve<IY<int, string>>());

            Assert.AreEqual(Error.NoMatchedImplementedTypesWithServiceType, ex.Error);
        }

        [Test]
        public void Should_throw_when_mapping_factory_mathod_declaring_type_with_incompatible_type_arguments()
        {
            var container = new Container();
            container.RegisterExports(typeof(X<>));

            var ex = Assert.Throws<ContainerException>(() => 
                container.Resolve<IY<List<string>, Tuple<int, string>>>());

            Assert.AreEqual(Error.NoMatchedFactoryMethodDeclaringTypeWithServiceTypeArgs, ex.Error);
        }

        [Test]
        public void Should_Not_throw_with_IfUnresolvedReturnDefault_when_mapping_factory_mathod_declaring_type_with_incompatible_type_arguments()
        {
            var container = new Container();
            container.RegisterExports(typeof(X<>));

            var y = container.Resolve<IY<List<string>, Tuple<int, string>>>(IfUnresolved.ReturnDefault);

            Assert.IsNull(y);
        }

        [Test]
        public void Should_throw_when_mapping_factory_method_with_incompatible_type_arguments()
        {
            var container = new Container();
            container.RegisterExports(typeof(XX<>));

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<Y<string, int>>());

            Assert.AreEqual(Error.NoMatchedFactoryMethodWithServiceTypeArgs, ex.Error);
        }

        [Test]
        public void Should_map_factory_field_with_compatible_type_arguments()
        {
            var container = new Container();
            container.RegisterExports(typeof(X<>));

            var y = container.Resolve<IY<List<int>, Tuple<string, int>>>();

            Assert.IsInstanceOf<Y<int, string>>(y);
        }

        [Test]
        public void Should_map_factory_property_with_compatible_type_arguments()
        {
            var container = new Container();
            container.RegisterExports(typeof(X<>));

            var y = container.Resolve<IY<List<int>, Tuple<string, int>>>("property");

            Assert.IsInstanceOf<Y<int, string>>(y);
        }

        public static class X<A>
        {
            [Export]
            public static Y<A, B> Get<B>(A a) { return new Y<A, B>(a); }

            [Export(typeof(IY<,>))]
            public static Y<int, A> YField = new Y<int, A>(1);

            [Export("property", typeof(IY<,>))]
            public static Y<int, A> YProperty { get { return YField; } }
        }

        public static class XX<A>
        {
            [Export]
            public static Y<A, IList<B>> Get<B>(A a)
            {
                return new Y<A, IList<B>>(a);
            }
        }

        public interface IZ<T> {}

        [Export(typeof(IZ<>))]
        public class Z<A> : IZ<A>
        {
            // Deceiving method with the same signature as the closed-generic Get<string> below.
            public Y<A, string> Get(string a) { return new Y<A, string>(default(A)); }

            [Export]
            public Y<A, B> Get<B>(A a) { return new Y<A, B>(a); }
        }

        public interface IY<T, T1> {}

        public class Y<T, R> : IY<List<T>, Tuple<R, T>>
        { 
            public T Blah;

            public Y(T t)
            {
                Blah = t;
            }
        }

        public static class CFactory
        {
            [Export(typeof(I<,,>))]
            public static C<U, T> CreateC<T, U>() // inverted arguments for fun
            {
                return new C<U, T>();
            }
        }

        [Export]
        public static class DFactory
        {
            [ExportMany] // magic export
            public static D<A> CreateD<A>()
            {
                return new D<A>();
            }
        }

        public static class EFactory
        {
            [ExportMany]
            public static E<Y, X> CreateE<X, Y>()
            {
                return new E<Y, X>();
            }
        }

        public interface I<T, U, W> { }

        public class C<U, T> : I<List<T>, Tuple<string, U>, long[]> { }

        public class D<U> : I<U, U, U> { }

        public class E<T, U> : I<U, T, U> { }

        public class F<T, U> : I<U, U, U> { }

        public class G<T> : I<T, T, int> { }

        public interface I { }

        public class C : I { }

        public class D : I<int, string, long> { }
    }
}
