using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue514_Avoid_using_the_Func_arguments_for_recursive_service_first_found_outside_the_Func : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            var container = new Container();

            container.RegisterInstance(0);
            container.Register<DependencyA>();
            container.Register<DependencyB>();
            container.Register<Parent>();

            var parent = container.Resolve<Parent>();

            Assert.AreEqual(0, parent.Dependency.Value); // 0
            Assert.AreEqual(23, parent.Dependency.Dependency.Value); // 23
            Assert.AreEqual(23, parent.Dependency.Dependency.Parent.Dependency.Value); // 23 --> BUT EXPECTING 0 
        }

        internal class DependencyA
        {
            public int Value { get; }
            public DependencyB Dependency { get; }

            public DependencyA(
                int value,
                Func<int, DependencyB> dependencyB)
            {
                Value = value;
                Dependency = dependencyB(23);
            }
        }


        internal class DependencyB
        {
            private readonly Lazy<Parent> _parentFactory;
            public int Value { get; }
            public Parent Parent => _parentFactory.Value;

            public DependencyB(
                int value,
                Lazy<Parent> parentFactory)
            {
                _parentFactory = parentFactory;
                Value = value;
            }
        }

        internal class Parent
        {
            public DependencyA Dependency { get; }

            public Parent(
                DependencyA dependencyA) =>
                Dependency = dependencyA;
        }
    }
}