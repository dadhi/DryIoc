using System.Linq;
using ImTools;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ArrayToolsTest
    {
        [Test]
        public void For_first_not_matching_item_it_should_exclude_the_item_from_result_array()
        {
            var a = new[] { 1, 2, 3, 4, 5 };

            var b = Enumerable.Where(a, n => n % 2 == 0).ToArray();
            var b1 = a.Match(n => n % 2 == 0);

            CollectionAssert.AreEqual(new[] { 2, 4 }, b);
            CollectionAssert.AreEqual(b, b1);
        }

        [Test]
        public void For_last_matching_item_it_should_include_the_item_into_result_array()
        {
            var a = new[] { 1, 2, 3, 4, 5 };

            var b = a.Match(n => n % 2 != 0);

            CollectionAssert.AreEqual(new[] { 1, 3, 5 }, b);
        }

        [Test]
        public void For_all_not_matched_items_it_should_return_empty_array()
        {
            var a = new[] { 1, 2, 3, 4, 5 };

            var b = a.Match(n => n < 0);

            Assert.IsEmpty(b);
        }

        [Test]
        public void For_all_matched_items_it_should_return_original_array()
        {
            var a = new[] { 1, 2, 3, 4, 5 };

            var b = a.Match(n => n > 0);

            Assert.AreSame(a, b);
        }

        [Test]
        public void Matched_iten_in_one_item_array_should_return_original_array()
        {
            var a = new[] { 1 };

            var b = a.Match(n => n > 0);

            Assert.AreSame(a, b);
        }

        [Test]
        public void Not_matched_item_in_one_item_array_should_return_original_array()
        {
            var a = new[] { 1 };

            var b = a.Match(n => n < 0);

            Assert.IsEmpty(b);
        }

        [Test]
        public void Match_of_empty_array_should_return_the_original_array()
        {
            var a = new int[0];

            var b = a.Match(n => n > 0);

            Assert.AreSame(a, b);
        }

        [Test]
        public void Match_of_null_array_should_return_null()
        {
            int[] a = null;

            var b = a.Match(n => n > 0);

            Assert.IsNull(b);
        }
    }
}
