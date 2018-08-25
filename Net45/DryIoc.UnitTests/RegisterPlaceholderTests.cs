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

        class M
        {
            public IN N;
            public M(IN n)
            {
                N = n;
            }
        }

        interface IN { }
        class N1 : IN {}
        class N2 : IN {}
    }
}
