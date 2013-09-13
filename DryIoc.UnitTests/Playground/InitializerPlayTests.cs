using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace DryIoc.UnitTests.Playground
{
    [TestFixture]
    public class InitializerPlayTests
    {
        [Test]
        public void Should_be_able_init_service_registered_with_impl_type()
        {
            var container = new Container();
            container.Register<Boo>(setup: FactorySetup.With(
                x => Expression.MemberInit((NewExpression)x, Expression.Bind(typeof(Boo).GetMember("Flag")[0], Expression.Constant(1)))));

            var boo = container.Resolve<Boo>();

            Assert.That(boo.Flag, Is.EqualTo(1));
        }

        [Test]
        [Ignore]
        public void POC2_Can_set_property_or_fied_with_ExpresstionTree_NET35()
        {
            Func<Boo, Boo> setFlagToOne = _ => _.Do(x => x.Flag = 1);
            var setFlagToOneExpr = Expression.Call(Expression.Constant(setFlagToOne), "Invoke", null, Expression.Constant(GetBoo()));
            var doSet = Expression.Lambda<Func<Boo>>(setFlagToOneExpr, null).Compile();

            var boo = doSet();

            Assert.That(boo.Flag, Is.EqualTo(1));
        }

        [Test]
        public void Should_be_able_init_service_registered_with_lambda()
        {
            var container = new Container();

            Func<Boo, Boo> setFlagToOne = _ => _.Do(x => x.Flag = 1);

            container.RegisterLambda(() => new Boo(), setup: FactorySetup.With(
                x => Expression.Call(Expression.Constant(setFlagToOne), "Invoke", null, x)));

            var boo = container.Resolve<Boo>();

            Assert.That(boo.Flag, Is.EqualTo(1));
        }

        public static Boo GetBoo()
        {
            return new Boo();
        }
    }

    public static class Assign
    {
        public static T Do<T>(this T self, Action<T> effect)
        {
            effect(self);
            return self;
        }
    }

    public class Boo
    {
        public int Flag;
    }
}
