using System;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ThrowTests : ITest
    {
        public int Run()
        {

            Run(If_arg_null_message_should_say_arg_of_type_is_null);
            Run(If_bad_arg_condition_You_should_specify_error_message);
            Run(If_bad_condition_Then_InvalidOpEx_should_be_thrown);
            Run(Can_log_exception_if_I_want_to);
            Run(Can_wrap_inner_exception);
            return 5;
        }

        private void Run(Action action)
        {
            SetupTestException();
            try 
            {
                action();
            }
            finally
            {
                TearDownTestException();
            }
        }

        private Throw.GetMatchedExceptionHandler _original;

        [SetUp]
        public void SetupTestException()
        {
            _original = Throw.GetMatchedException;
            Throw.GetMatchedException = (check, error, arg0, arg1, arg2, arg3, inner) => new InvalidOperationException(error.ToString(), inner);
        }

        [TearDown]
        public void TearDownTestException()
        {
            Throw.GetMatchedException = _original;
        }

        [Test]
        public void If_arg_null_message_should_say_arg_of_type_is_null()
        {
            string arg = null;

            var ex = Assert.Throws<InvalidOperationException>(() =>
                arg.ThrowIfNull());

            StringAssert.Contains("-1", ex.Message);
        }

        [Test]
        public void If_bad_arg_condition_You_should_specify_error_message()
        {
            string arg = null;

            var ex = Assert.Throws<InvalidOperationException>(() =>
                arg.ThrowIf(arg == null, 3));

            Assert.That(ex.Message, Is.EqualTo("3"));
        }

        [Test]
        public void If_bad_condition_Then_InvalidOpEx_should_be_thrown()
        {
            string arg = null;

            var ex = Assert.Throws<InvalidOperationException>(() =>
                Throw.If(arg == null, 5));

            Assert.AreEqual(ex.Message, "5");
        }

        [Test]
        public void Can_log_exception_if_I_want_to()
        {
            var error = new Exception();
            Exception loggedError = null;
            Action<Exception> logError = ex => { loggedError = ex; };

            Throw.GetMatchedException = (check, e, arg0, arg1, arg2, arg3, inner) =>
            {
                logError(error);
                return error;
            };

            try { Throw.If(true, 4); }
            catch { }

            Assert.That(loggedError, Is.SameAs(error));
        }

        [Test]
        public void Can_wrap_inner_exception()
        {
            var innerException = new ArgumentException();

            var ex = Assert.Throws<InvalidOperationException>(() => 
                Throw.IfThrows<ArgumentException, string>(() => { throw innerException; }, true, 3));

            Assert.AreSame(innerException, ex.InnerException);
        }
    }
}
