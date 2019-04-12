using System;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ActionTests
    {
        [Test]
        public void Can_resolve_void_method_with_no_args_as_Action()
        {
            var container = new Container();

            container.Register(typeof(void),
                made: Made.Of(typeof(ActionTests).SingleMethod(nameof(Act))));

            var act = container.Resolve<Action>();
            act();
        }

        [Test]
        public void Can_resolve_void_method_with_one_arg_as_Action()
        {
            var container = new Container();

            container.Register<IntValue>(Reuse.Singleton);
            container.Register(typeof(void),
                made: Made.Of(typeof(ActionTests).SingleMethod(nameof(Act1))));

            var act = container.Resolve<Action>();
            act();

            Assert.AreEqual(3, container.Resolve<IntValue>().Value);
        }

        [Test]
        public void Can_resolve_void_method_with_one_arg_as_Action_of_one_arg()
        {
            var container = new Container();

            container.Register(typeof(void),
                made: Made.Of(typeof(ActionTests).SingleMethod(nameof(Act1))));

            var act = container.Resolve<Action<IntValue>>();
            var value = new IntValue();
            act(value);

            Assert.AreEqual(3, value.Value);
        }

        [Test]
        public void Can_inject_void_method_with_one_arg_as_Action_of_one_arg()
        {
            var container = new Container();

            container.Register<ActionUser>();
            container.Register(typeof(void),
                made: Made.Of(typeof(ActionTests).SingleMethod(nameof(Act1))));

            var user = container.Resolve<ActionUser>();
            var value = new IntValue();
            user.Change(value);

            Assert.AreEqual(3, value.Value);
        }

        [Test]
        public void Can_inject_Lazy_of_void_method_with_one_arg_as_Action_of_one_arg()
        {
            var container = new Container();

            container.Register<LazyActionUser>();
            container.Register(typeof(void),
                made: Made.Of(typeof(ActionTests).SingleMethod(nameof(Act1))));

            var user = container.Resolve<LazyActionUser>();
            var value = new IntValue();
            user.Change(value);

            Assert.AreEqual(3, value.Value);
        }

        public static void Act()
        {
        }

        public static void Act1(IntValue i) => i.Value = 3;

        public class IntValue
        {
            public int Value { get; set; }
        }

        public class ActionUser
        {
            public Action<IntValue> Change { get; private set; }

            public ActionUser(Action<IntValue> change)
            {
                Change = change;
            }
        }

        public class LazyActionUser
        {
            public Action<IntValue> Change { get; private set; }

            public LazyActionUser(Lazy<Action<IntValue>> change)
            {
                Change = change.Value;
            }
        }
    }
}