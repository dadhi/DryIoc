using System;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class RegisterPlaceholderTests
    {
        [Test]
        public void Can_register_service_without_impl()
        {
            var c = new Container();

            c.Register<M>();
            c.RegisterPlaceholder(typeof(IN));

            var m = c.Resolve<Lazy<M>>();
            Assert.IsNotNull(m);
        }

        [Test]
        public void Can_register_service_without_impl_and_fill_it_in_later()
        {
            var c = new Container();

            c.Register<M>();
            c.RegisterPlaceholder(typeof(IN));

            var m = c.Resolve<Lazy<M>>();
            c.Register<IN, N1>(Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            Assert.IsInstanceOf<N1>(m.Value.N);
        }

        [Test]
        public void Should_throw_when_resolving_placeholder_service()
        {
            var c = new Container();

            c.Register<M>();
            c.RegisterPlaceholder(typeof(IN));

            var m = c.Resolve<Lazy<M>>();

            IN n = null;
            var ex = Assert.Throws<ContainerException>(() => n = m.Value.N);

            Assert.AreEqual(
                Error.NameOf(Error.NoImplementationForPlaceholder),
                Error.NameOf(ex.Error));
        }

        [Test]
        public void Can_register_service_without_impl_resolve_it_in_lazy_Wrapper()
        {
            var c = new Container();

            c.RegisterPlaceholder<IN>();

            var funcN = c.Resolve<Func<IN>>();
            Assert.IsNotNull(funcN);

            var arrFuncN = c.Resolve<Func<IN>[]>();
            Assert.AreEqual(1, arrFuncN.Length);

            c.Register<IN, N1>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            Assert.IsInstanceOf<N1>(arrFuncN[0]());
        }

        [Test]
        public void Can_register_multiple_placeholders()
        {
            var c = new Container();

            c.RegisterPlaceholder<IN>();
            c.RegisterPlaceholder<IN>();

            var arrFuncN = c.Resolve<Func<IN>[]>();
            Assert.AreEqual(2, arrFuncN.Length);

            c.Register<IN, N1>(ifAlreadyRegistered: IfAlreadyRegistered.Replace, serviceKey: DefaultKey.Of(0));
            c.Register<IN, N2>(ifAlreadyRegistered: IfAlreadyRegistered.Replace, serviceKey: DefaultKey.Of(1));
            Assert.IsInstanceOf<N1>(arrFuncN[0]());
            Assert.IsInstanceOf<N2>(arrFuncN[1]());
        }

        [Test]
        public void Can_register_the_decorator_without_the_actual_implementation_using_the_placeholder()
        {
            var c = new Container();

            c.Register<M>();
            c.RegisterPlaceholder<IN>();
            c.Register<IN, D>(setup: Setup.Decorator);

            var m = c.Resolve<M>();

            Assert.IsInstanceOf<D>(m.N);
        }

        [Test]
        public void Can_register_the_decorator_without_the_actual_implementation_using_the_placeholder_2()
        {
            var c = new Container();

            c.Register<M>();
            c.RegisterPlaceholder<IN>();
            c.Register<IN, D2>(setup: Setup.Decorator);

            var m = c.Resolve<M>();

            Assert.IsInstanceOf<D2>(m.N);
        }

        [Test]
        public void Can_register_the_decorator_without_the_actual_implementation_using_the_placeholder_2_and_replace_implementation_later()
        {
            var c = new Container();

            c.Register<M>();
            c.RegisterPlaceholder<IN>();
            c.Register<IN, D2>(setup: Setup.Decorator);

            var m = c.Resolve<M>();
            Assert.IsInstanceOf<D2>(m.N);
            var ex = Assert.Throws<ContainerException>(() => ((D2)m.N).Fn());
            Assert.AreEqual(Error.NameOf(Error.NoImplementationForPlaceholder), ex.ErrorName);

            c.Register<IN, N1>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            var m2 = c.Resolve<M>();
            Assert.IsInstanceOf<D2>(m.N);
            //Assert.IsInstanceOf<N1>(((D2)m.N).Fn());              // todo: does not work
            //Assert.IsInstanceOf<N1>(((D2)((D2)m.N).Fn()).Fn());   // todo: does not work
        }

        class M
        {
            public IN N;
            public M(IN n) => N = n;
        }

        interface  IN {}
        class N1 : IN {}
        class N2 : IN {}
        class  D : IN {}
        class  D2 : IN 
        {
            public Func<IN> Fn;
            public D2(Func<IN> fn) => Fn = fn;
        }
    }
}
