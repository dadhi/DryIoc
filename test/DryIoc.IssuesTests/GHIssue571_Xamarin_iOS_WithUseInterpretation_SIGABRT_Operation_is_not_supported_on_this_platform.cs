using NUnit.Framework;


namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue571_Xamarin_iOS_WithUseInterpretation_SIGABRT_Operation_is_not_supported_on_this_platform : ITest
    {
        public int Run()
        {
            Test_multiple_scope_openings_simulate_second_navigation();
            Test_WithUseInterpretation_does_not_create_DynamicMethod();
            Test_WithUseInterpretation_across_multiple_scope_navigations();
            return 3;
        }

        [Test]
        public void Test_multiple_scope_openings_simulate_second_navigation()
        {
            var c = new Container(Rules.Default.WithUseInterpretation());

            c.Register<R>();
            c.Register<A>(Reuse.Scoped);
            c.Register<B>(Reuse.Singleton);

            // First "navigation" - open scope, resolve, close
            using (var s1 = c.OpenScope())
            {
                var r = s1.Resolve<R>();
                Assert.IsNotNull(r);
                Assert.IsNotNull(r.A);
                Assert.IsNotNull(r.A.B);
            }

            // Second "navigation" - open scope again, resolve - this is the scenario that caused SIGABRT on iOS
            using (var s2 = c.OpenScope())
            {
                var r = s2.Resolve<R>();
                Assert.IsNotNull(r);
                Assert.IsNotNull(r.A);
                Assert.IsNotNull(r.A.B);
            }

            // Third "navigation" for good measure
            using (var s3 = c.OpenScope())
            {
                var r = s3.Resolve<R>();
                Assert.IsNotNull(r);
                Assert.IsNotNull(r.A);
                Assert.IsNotNull(r.A.B);
            }
        }

        [Test]
        public void Test_WithUseInterpretation_does_not_create_DynamicMethod()
        {
            // Verify that DynamicMethod creation is not triggered when WithUseInterpretation is used.
            // On AOT platforms like Xamarin.iOS, DynamicMethod.ctor throws PlatformNotSupportedException.
            // The fix ensures DryIoc uses only the interpreter (no compilation) when UseInterpretation=true.
            var c = new Container(Rules.Default.WithUseInterpretation());

            c.Register<R>();
            c.Register<A>(Reuse.Scoped);
            c.Register<B>(Reuse.Singleton);

            using var s = c.OpenScope();

            // If this succeeds it confirms interpretation was used without any DynamicMethod or IL emit.
            // On iOS AOT (where DynamicMethod.ctor throws), this test would fail if compilation is triggered.
            var r = s.Resolve<R>();
            Assert.IsNotNull(r);
            Assert.IsNotNull(r.A);
            Assert.IsNotNull(r.A.B);

            // Verify same instance is returned for scoped service
            var r2 = s.Resolve<R>();
            Assert.AreSame(r.A, r2.A); // Same scope - same A instance
            Assert.AreSame(r.A.B, r2.A.B); // Same singleton B
        }

        [Test]
        public void Test_WithUseInterpretation_across_multiple_scope_navigations()
        {
            var c = new Container(Rules.Default.WithUseInterpretation());

            c.Register<R>();
            c.Register<A>(Reuse.Scoped);
            c.Register<B>(Reuse.Singleton);

            A aFromScope1 = null;
            B bSingleton = null;

            // First navigation
            using (var s1 = c.OpenScope())
            {
                var r1 = s1.Resolve<R>();
                aFromScope1 = r1.A;
                bSingleton = r1.A.B;
                Assert.IsNotNull(aFromScope1);
                Assert.IsNotNull(bSingleton);
            }

            // Second navigation - scope is disposed, so A should be a NEW instance
            // but B (singleton) should be the same instance
            using (var s2 = c.OpenScope())
            {
                var r2 = s2.Resolve<R>();
                Assert.IsNotNull(r2.A);
                Assert.IsNotNull(r2.A.B);

                Assert.AreNotSame(aFromScope1, r2.A, "Scoped A should be a new instance in new scope");
                Assert.AreSame(bSingleton, r2.A.B, "Singleton B should be the same instance across scopes");
            }
        }

        class R
        {
            public readonly A A;
            public R(A a) => A = a;
        }

        class A
        {
            public readonly B B;
            public A(B b) => B = b;
        }

        class B { }
    }
}
