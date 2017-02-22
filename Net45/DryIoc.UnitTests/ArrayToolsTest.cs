using System.Linq;
using ImTools;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ArrayToolsTest
    {
        [Test]
        public void Where_should_filter_some_items_with_first_non_matching_item()
        {
            var a = new[] { 1, 2, 3, 4, 5 };

            var b = Enumerable.Where(a, n => n % 2 == 0).ToArray();
            var b1 = a.Match(n => n % 2 == 0);

            CollectionAssert.AreEqual(new[] { 2, 4 }, b);
            CollectionAssert.AreEqual(b, b1);
        }

        [Test]
        public void Where_should_filter_some_items_with_last_matching_item()
        {
            var a = new[] { 1, 2, 3, 4, 5 };

            var b = a.Match(n => n % 2 != 0);

            CollectionAssert.AreEqual(new[] { 1, 3, 5 }, b);
        }

        [Test]
        public void Where_should_filter_all_items()
        {
            var a = new[] { 1, 2, 3, 4, 5 };

            var b = a.Match(n => n < 0);

            Assert.IsEmpty(b);
        }

        [Test]
        public void Where_should_filter_none_items_and_return_original_array()
        {
            var a = new[] { 1, 2, 3, 4, 5 };

            var b = a.Match(n => n > 0);

            Assert.AreSame(a, b);
        }

        [Test]
        public void Where_should_return_the_original_empty_array()
        {
            var a = new int[0];

            var b = a.Match(n => n > 0);

            Assert.AreSame(a, b);
        }

        [Test]
        public void Where_should_return_the_original_null_array()
        {
            int[] a = null;

            var b = a.Match(n => n > 0);

            Assert.IsNull(b);
        }
    }
}
