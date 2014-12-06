using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace DryIoc.Playground
{
    [TestFixture]
    public class ExpressionTreeTests
    {
        Type _type = typeof(ExpressionTreeTests);
        MyClass _obj = new MyClass("A");

        [Test]
        public void Expression_constant_test()
        {
            var typeExpr = Expression.Constant(_type, typeof(Type));
            var getType = Expression.Lambda<Func<object>>(typeExpr, null).Compile();
            _type = typeof(String);
            Assert.That(getType(), Is.InstanceOf<Type>());

            var objExpr = Expression.Constant(_obj, typeof(MyClass));
            var getObj = Expression.Lambda<Func<MyClass>>(objExpr, null).Compile();
            Assert.That(getObj().Message, Is.EqualTo("A"));
            _obj = new MyClass("B");
            Assert.That(getObj().Message, Is.EqualTo("A"));
        }

        internal class MyClass
        {
            public string Message { get; set; }

            public MyClass(string message)
            {
                Message = message;
            }
        }
    }
}
