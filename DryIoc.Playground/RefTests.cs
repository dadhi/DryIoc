using System.Threading;
using NUnit.Framework;

namespace DryIoc.Playground
{
    [TestFixture][Ignore]
    public class RefTests
    {
        [Test]
        public void Test()
        {
            var maxValue = int.MaxValue;
            int blah = maxValue + 1;
        }
    }
}

public sealed class Ref<T> where T : class
{
    public T Value { get { return _value; } }

    public T Swap(T value)
    {
        return Interlocked.Exchange(ref _value, value);
    }

    private T _value;
}
