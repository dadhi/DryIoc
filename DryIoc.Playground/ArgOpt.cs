using NUnit.Framework;

namespace DryIoc.Playground
{
    [TestFixture]
    public class OptTests
    {
        [Test]
        public void Test()
        {
            var o1 = new DataObject(100, "Value1", new DataObject2(10m, "CleanPrice"));
            var o2 = o1.With(500, obj: o1.Object.With(500));
            Assert.That(o2.Id, Is.EqualTo(500));
            Assert.That(o2.Object.Price, Is.EqualTo(500));
        }
    }

    /// <summary>Helper structure which allows to distinguish null value from the default value for optional parameter.</summary>
    /// <typeparam name="T">Type of parameter</typeparam>
    public struct ArgOpt<T>
    {
        /// <summary>Allows to transparently convert parameter argument to opt structure.</summary>
        /// <param name="value">Argument value to wrap, may be null.</param>
        public static implicit operator ArgOpt<T>(T value)
        {
            return new ArgOpt<T>(value);
        }

        /// <summary>Argument value.</summary>
        public readonly T Value;

        /// <summary>Indicates that value is passed.</summary>
        public readonly bool HasValue;

        /// <summary>Wraps passed value in structure. Sets the flag that value is present.</summary>
        /// <param name="value"></param>
        public ArgOpt(T value)
        {
            HasValue = true;
            Value = value;
        }

        /// <summary>Helper to get value or default value if value is not present.</summary>
        /// <param name="defaultValue">(optional) Default value.</param>
        /// <returns>Value or default.</returns>
        public T OrDefault(T defaultValue = default(T))
        {
            return HasValue ? Value : defaultValue;
        }
    }

    public class DataObject
    {
        public int Id { get; private set; }
        public string Value { get; private set; }
        public DataObject2 Object { get; private set; }

        public DataObject(int id, string value, DataObject2 o)
        {
            Id = id;
            Value = value;
            Object = o;
        }

        private DataObject() {}

        public DataObject With(ArgOpt<int> id = default(ArgOpt<int>), ArgOpt<string> value = default(ArgOpt<string>), ArgOpt<DataObject2> obj = default(ArgOpt<DataObject2>))
        {
            return new DataObject
            {
                Id = id.OrDefault(Id),
                Value = value.OrDefault(Value),
                Object = obj.OrDefault(Object)
            };
        }
    }

    public class DataObject2
    {
        public decimal Price { get; private set; }
        public string PriceType { get; private set; }

        public DataObject2(decimal price, string priceType)
        {
            Price = price;
            PriceType = priceType;
        }

        private DataObject2()
        {
        }

        public DataObject2 With(ArgOpt<decimal> price = default(ArgOpt<decimal>), ArgOpt<string> priceType = default(ArgOpt<string>))
        {
            return new DataObject2
            {
                Price = price.OrDefault(Price),
                PriceType = priceType.OrDefault(PriceType)
            };
        }
    }
}
