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
            container.RegisterExports(typeof(FactoryMethods));

            var result = container.Resolve<I<List<int>, Tuple<string, long>, long[]>>();

            Assert.IsInstanceOf<C<long, int>>(result);
        }

        [Export, AsFactory]
        public static class FactoryMethods
        {
            [Export(typeof(I<,,>))]
            public static C<U, T> CreateC<T, U>() // inverted arguments for fun
            {
                return new C<U, T>();
            }
        }

        interface I<T, U, W> { }

        public class C<U, T> : I<List<T>, Tuple<string, U>, long[]> { }
        class D<U> : I<U, U, U> { }
        class E<T, U> : I<U, T, U> { }
        class F<T, U> : I<U, U, U> { }
        class G<T> : I<T, T, int> { }

        interface I { }
        class C : I { }
        class D : I<int, string, long> { }
    }
}
