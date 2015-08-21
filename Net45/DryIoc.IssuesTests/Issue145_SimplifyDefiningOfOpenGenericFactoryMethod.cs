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

        [Export, AsFactory]
        public static class CFactory
        {
            [Export(typeof(I<,,>))]
            public static C<U, T> CreateC<T, U>() // inverted arguments for fun
            {
                return new C<U, T>();
            }
        }

        [Export, AsFactory]
        public static class DFactory
        {
            [ExportMany] // magic export
            public static D<A> CreateD<A>()
            {
                return new D<A>();
            }
        }

        [Export, AsFactory]
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
