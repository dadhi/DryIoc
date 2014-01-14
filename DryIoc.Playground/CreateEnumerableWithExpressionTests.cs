using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;

namespace DryIoc.Playground
{
    [TestFixture]
    public class CreateEnumerableWithExpressionTests
    {
        [Test]
        public void Can_create_method_yielding_items()
        {
            Expression<Func<IEnumerable<IItem>>> getItems = () => GetItems();

            //Func<Expression[], Expression<Func<IEnumerable<IItem>>>> getItems2 = items =>
            //{
                 
            //};
        }

        IEnumerable<IItem> GetItems()
        {
            yield return new SomeItem();
            yield return new AnotherItem();
        }

        IEnumerable<IItem> ResolveItems()
        {
            //yield return ResolveItem();
            //yield return ResolveNextItem();
            return null;
        }
    }

    internal class SomeItem : IItem {}
    internal class AnotherItem : IItem {}

    internal interface IItem {}
}
