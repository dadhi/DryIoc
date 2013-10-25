using System;
using System.Diagnostics;

namespace DryIoc.UnitTests.Performance
{
    public static class ClosureFieldsAccessSpeed
    {
        static Func<Func<T, X>, X> Create<T>(Func<T> getT)
        {
            var t = getT();
            return func => func(t);
        }

        public static void Test()
        {
            var a = new A();
            var b = new B();
            var c = new C();

            Func<X> one = () => new X(a, b, c, new A1(a), new B1(b), new C1(c));
            Func<X> two = () =>
            {
                var a_ = a;
                var b_ = b;
                var c_ = c;
                return new X(a_, b_, c_, new A1(a_), new B1(b_), new C1(c_));
            };

            Func<X> three = () => new Func<A, B, C, X>((arg1, arg2, arg3) =>
                new X(arg1, arg2, arg3, new A1(arg1), new B1(arg2), new C1(arg3))).Invoke(a, b, c);

            var hah = Create(() => new { a = new A(), b = new B(), c = new C() });
            Func<X> four = () => hah(_ => new X(_.a, _.b, _.c, new A1(_.a), new B1(_.b), new C1(_.c)));

            var times = 1 * 1000 * 1000;
            Console.WriteLine("Testing {0:n0} times in for cycle: ", times);

            X x = null;

            GC.Collect();
            var timer = Stopwatch.StartNew();
            for (var t = 0; t < times; t++)
            {
                x = one();
            }
            timer.Stop();
            Console.WriteLine("One took {0} milliseconds to complete.", timer.ElapsedMilliseconds);

            GC.Collect();
            timer = Stopwatch.StartNew();
            for (var t = 0; t < times; t++)
            {
                x = two();
            }
            timer.Stop();
            Console.WriteLine("Two took {0} milliseconds to complete.", timer.ElapsedMilliseconds);

            GC.Collect();
            timer = Stopwatch.StartNew();
            for (var t = 0; t < times; t++)
            {
                x = three();
            }
            timer.Stop();
            Console.WriteLine("Three took {0} milliseconds to complete.", timer.ElapsedMilliseconds);

            GC.Collect();
            timer = Stopwatch.StartNew();
            for (var t = 0; t < times; t++)
            {
                x = four();
            }
            timer.Stop();
            Console.WriteLine("Four took {0} milliseconds to complete.", timer.ElapsedMilliseconds);

            GC.KeepAlive(x);
        }
    }

    class X
    {
        public A A { get; set; }
        public B B { get; set; }
        public C C { get; set; }

        public A1 A1 { get; set; }
        public B1 B1 { get; set; }
        public C1 C1 { get; set; }

        public X(A a, B b, C c, A1 a1, B1 b1, C1 c1)
        {
            A = a;
            B = b;
            C = c;

            A1 = a1;
            B1 = b1;
            C1 = c1;
        }
    }

    class A { }
    class B { }
    class C { }

    class A1
    {
        public A A { get; set; }

        public A1(A a)
        {
            A = a;
        }
    }

    class B1
    {
        public B B { get; set; }

        public B1(B b)
        {
            B = b;
        }
    }

    class C1
    {
        public C C { get; set; }

        public C1(C c)
        {
            C = c;
        }
    }
}
