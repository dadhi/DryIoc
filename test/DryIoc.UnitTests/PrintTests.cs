using NUnit.Framework;
using System;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class PrintTests : ITest
    {
        public int Run()
        {
            For_empty_array_it_should_output_the_empty_initializer();
            For_flags_collection_it_should_correctly_output_the_flags();
            return 2;
        }

        [Test]
        public void For_empty_array_it_should_output_the_empty_initializer()
        {
            Assert.AreEqual("new object[]{}", new object[]{}.Print());
        }

        [Flags] enum A { X = 1, Y = 1 << 1 }

        [Test]
        public void For_flags_collection_it_should_correctly_output_the_flags()
        {
            var flags = new[] { A.Y, A.X|A.Y, default };
#if DEBUG
            Assert.AreEqual("new A[]{A.Y, A.X|A.Y, (A)0}", flags.Print());
#else
            Assert.AreEqual("new DryIoc.UnitTests.PrintTests.A[]{DryIoc.UnitTests.PrintTests.A.Y, DryIoc.UnitTests.PrintTests.A.X|DryIoc.UnitTests.PrintTests.A.Y, (DryIoc.UnitTests.PrintTests.A)0}", flags.Print());
#endif
        }
    }
}
