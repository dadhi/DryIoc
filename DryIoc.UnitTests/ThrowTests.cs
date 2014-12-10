using System;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ThrowTests
    {
        private Throw.GetMatchedExceptionHandler _original;

        [SetUp]
        public void SetupTestException()
        {
            _original = Throw.GetMatchedException;
            Throw.GetMatchedException = (check, error, arg0, arg1, arg2, arg3, inner) => new InvalidOperationException(error.ToString());
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

            Assert.That(ex.Message, Is.StringContaining("-1"));
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
    }
}
