using System;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ThrowTests : ITest
    {
        public int Run()
        {

            If_arg_null_message_should_say_arg_of_type_is_null();
            If_bad_arg_condition_You_should_specify_error_message();
            If_bad_condition_Then_InvalidOpEx_should_be_thrown();
            Can_wrap_inner_exception();
            return 4;
        }

        [Test]
        public void If_arg_null_message_should_say_arg_of_type_is_null()
        {
            string arg = null;

            var ex = Assert.Throws<ContainerException>(() =>
                arg.ThrowIfNull());

            StringAssert.Contains("code: Error.ErrorCheck", ex.Message);
        }

        [Test]
        public void If_bad_arg_condition_You_should_specify_error_message()
        {
            string arg = null;

            var ex = Assert.Throws<ContainerException>(() =>
                arg.ThrowIf(arg == null, Error.RegisteringImplementationNotAssignableToServiceType));

            StringAssert.Contains("code: Error.RegisteringImplementationNotAssignableToServiceType", ex.Message);
        }

        [Test]
        public void If_bad_condition_Then_InvalidOpEx_should_be_thrown()
        {
            string arg = null;

            var ex = Assert.Throws<ContainerException>(() =>
                Throw.If(arg == null, Error.ImpossibleToRegisterOpenGenericWithRegisterDelegate));

            StringAssert.Contains("code: Error.ImpossibleToRegisterOpenGenericWithRegisterDelegate", ex.Message);
        }

        [Test]
        public void Can_wrap_inner_exception()
        {
            var innerException = new ArgumentException();

            var ex = Assert.Throws<ContainerException>(() =>
                Throw.IfThrows<ArgumentException, string>(() => { throw innerException; }, true, 3));

            Assert.AreSame(innerException, ex.InnerException);
        }
    }
}
