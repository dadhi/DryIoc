using NUnit.Framework;

namespace DryIoc.Playground
{
    [TestFixture]
    public class IntNTreeTests
    {
        private int _originalN;

        [SetUp]
        public void SetUp()
        {
            _originalN = IntNTree<int>.N;
            IntNTree<int>.N = 2;
        }

        [TearDown]
        public void TearDown()
        {
            IntNTree<int>.N = _originalN;
        }

        [Test]
        public void CanGetSuccessfully()
        {
            var t = IntNTree<int>.Empty
                .AddOrUpdate(1, 11)
                .AddOrUpdate(2, 22)
                .AddOrUpdate(3, 33)
                .AddOrUpdate(0, 0);

            Assert.AreEqual(11, t.GetValueOrDefault(1));
            Assert.AreEqual(22, t.GetValueOrDefault(2));
            Assert.AreEqual(33, t.GetValueOrDefault(3));
            Assert.AreEqual(0, t.GetValueOrDefault(0));
        }

        [Test]
        public void LLCase()
        {
            var t = IntNTree<int>.Empty
                .AddOrUpdate(5, 1)
                .AddOrUpdate(4, 2)
                .AddOrUpdate(3, 3);

            //      4, 5
            //     /
            //   3
            Assert.AreEqual(4, t.LeftKey);
            Assert.AreEqual(5, t.RightKey);
            Assert.AreEqual(3, t.Left.LeftKey);
        }

        [Test]
        public void TreeRemainsBalancedAfterUnbalancedInsertIntoBalancedTree()
        {
            var t = IntNTree<int>.Empty
                .AddOrUpdate(5, 1)
                .AddOrUpdate(4, 2)
                .AddOrUpdate(3, 3)
                .AddOrUpdate(2, 4)
                .AddOrUpdate(1, 5);

            //     2, 3
            //    /    \
            //   1     4, 5
            Assert.AreEqual(2, t.LeftKey);
            Assert.AreEqual(3, t.RightKey);
            Assert.AreEqual(1, t.Left.LeftKey);
            Assert.AreEqual(4, t.Right.LeftKey);
            Assert.AreEqual(5, t.Right.RightKey);
        }

        [Test]
        public void Should_drop_left_key_to_left_branch()
        {
            var t = IntNTree<int>.Empty
                .AddOrUpdate(5, 1)
                .AddOrUpdate(3, 2)
                .AddOrUpdate(4, 3)
                .AddOrUpdate(1, 4);
            
            //      3, 5  =>  4, 5
            //               /
            //            1, 3
            Assert.AreEqual(4, t.LeftKey);
            Assert.AreEqual(5, t.RightKey);
            Assert.AreEqual(1, t.Left.LeftKey);
            Assert.AreEqual(3, t.Left.RightKey);
        }

        [Test]
        public void RRCase()
        {
            var t = IntNTree<int>.Empty
                .AddOrUpdate(3, 1)
                .AddOrUpdate(4, 2)
                .AddOrUpdate(5, 3);

            //      3, 4  
            //          \
            //           5
            Assert.AreEqual(3, t.LeftKey);
            Assert.AreEqual(4, t.RightKey);
            Assert.AreEqual(5, t.Right.RightKey);
        }

        [Test]
        public void RLCase()
        {
            var t = IntNTree<int>.Empty
                .AddOrUpdate(3, 1)
                .AddOrUpdate(6, 2)
                .AddOrUpdate(4, 3)
                .AddOrUpdate(1, 4)
                .AddOrUpdate(5, 5);

            //      3, 6 => 4, 6 => 4, 5
            //             /       /    \
            //          1, 3    1, 3     6
            Assert.AreEqual(4, t.LeftKey);
            Assert.AreEqual(5, t.RightKey);
            Assert.AreEqual(1, t.Left.LeftKey);
            Assert.AreEqual(3, t.Left.RightKey);
            Assert.AreEqual(6, t.Right.RightKey);
        }

        [Test]
        public void Search_in_empty_tree_should_NOT_throw()
        {
            var tree = IntNTree<int>.Empty;

            Assert.DoesNotThrow(
                () => tree.GetValueOrDefault(0));
        }

        [Test]
        public void Search_for_non_existent_key_should_NOT_throw()
        {
            var tree = IntNTree<int>.Empty
                .AddOrUpdate(1, 1)
                .AddOrUpdate(3, 2);

            Assert.DoesNotThrow(
                () => tree.GetValueOrDefault(2));
        }

        [Test]
        public void For_two_same_added_items_height_should_be_one()
        {
            var tree = IntNTree<string>
                .Empty
                .AddOrUpdate(0, "a")
                .AddOrUpdate(1, "x")
                .AddOrUpdate(1, "y");

            Assert.AreEqual("a", tree.GetValueOrDefault(0));
            Assert.AreEqual("y", tree.GetValueOrDefault(1));
        }
    }

    [TestFixture]
    public class Int3TreeTests
    {
        private int _originalN;

        [SetUp]
        public void SetUp()
        {
            _originalN = IntNTree<int>.N;
            IntNTree<int>.N = 5;
        }

        [TearDown]
        public void TearDown()
        {
            IntNTree<int>.N = _originalN;
        }

        [Test]
        public void Should_insert_key_in_current_node()
        {
            var t = IntNTree<int>.Empty
                .AddOrUpdate(5, 1)
                .AddOrUpdate(2, 2)
                .AddOrUpdate(3, 3)
                .AddOrUpdate(0, 4);

            Assert.AreEqual(0, t.LeftKey);
            Assert.AreEqual(5, t.RightKey);
            Assert.AreEqual(1, t.Height);
            Assert.AreEqual(2, t.GetValueOrDefault(2));
            Assert.AreEqual(3, t.GetValueOrDefault(3));
        }
    }
}
