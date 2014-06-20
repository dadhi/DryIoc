using NUnit.Framework;

namespace DryIoc.Playground
{
    [TestFixture]
    public class BitCountTests
    {
        [Test]
        public void Count_iterating()
        {
            Assert.That(BitCount(7), Is.EqualTo(3));
            Assert.That(BitCount(32-1), Is.EqualTo(5));
        }

        public static int BitCount(int n)
        {
            var count = n;
            while (n != 0) count -= (n >>= 1);
            return count;
        }
    }
}
