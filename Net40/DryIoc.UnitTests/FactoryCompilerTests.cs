using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace DryIoc.Playground
{
    [TestFixture]
    public class FactoryCompilerTests
    {
        [Test]
        public void Compile_delegate_with_nested_lambda()
        {
            Expression<FactoryDelegate> factory = 
                _ => new Func<Scope, object>(x => x.GetOrAdd(0, () => "a")).Invoke(new Scope());

            var factoryDelegate = factory.Body.CompileToDelegate();
            var result = (string)factoryDelegate.Invoke(null);

            Assert.That(result, Is.EqualTo("a"));
        }
    }
}
