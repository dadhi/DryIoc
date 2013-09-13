using System;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ThrowTests
    {
        private Func<string, Exception> _original;

        [SetUp]
        public void SetupTestException()
        {
            _original = Throw.GetException;
            Throw.GetException = s => new InvalidOperationException(s);
        }

        [TearDown]
        public void TearDownTestException()
        {
            Throw.GetException = _original;
        }

        [Test]
        public void If_arg_null_message_should_say_arg_of_type_is_null()
        {
            string arg = null;

            var ex = Assert.Throws<InvalidOperationException>(() =>
                arg.ThrowIfNull());

            Assert.That(ex.Message, Is.StringContaining("null"));
        }

        [Test]
        public void If_bad_arg_condition_You_should_specify_error_message()
        {
            string arg = null;

            var ex = Assert.Throws<InvalidOperationException>(() =>
                arg.ThrowIf(arg == null));

            Assert.That(ex.Message, Is.StringContaining("Argument of type System.String"));
        }

        [Test]
        public void If_bad_condition_Then_InvalidOpEx_should_be_thrown()
        {
            string arg = null;

            var ex = Assert.Throws<InvalidOperationException>(() =>
                Throw.If(arg == null, "Argument is null"));

            Assert.That(ex.Message, Is.EqualTo("Argument is null"));
        }

        [Test]
        public void Can_log_exception_if_I_want_to()
        {
            var error = new Exception();
            Exception loggedError = null;
            Action<Exception> logError = ex => { loggedError = ex; };

            Throw.GetException = message =>
            {
                logError(error);
                return error;
            };

            try { Throw.If(true, "Error!"); } catch {}
 
            Assert.That(loggedError, Is.SameAs(error));
        }
    }
}
