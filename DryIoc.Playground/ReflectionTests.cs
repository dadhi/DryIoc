using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;

namespace DryIoc.Playground
{
    [TestFixture]
    public class ReflectionTests
    {
        [Test]
        public void Get_property_GetSetMethod_with_ExpressionTree()
        {
            var pSetMethod = typeof(PropertyInfo).GetTypeInfo().DeclaredMethods
                .FirstOrDefault(m => m.Name == "GetSetMethod" && m.GetParameters().Length == 0);
            Assert.NotNull(pSetMethod);

            var pParamExpr = Expression.Parameter(typeof(PropertyInfo), "p");
            var pGetSetMethodExpr = Expression.Call(pParamExpr, pSetMethod);
            var getSetMethodExpr = Expression.Lambda<Func<PropertyInfo, MethodInfo>>(pGetSetMethodExpr, pParamExpr);
            
            var getSetMethod = getSetMethodExpr.Compile();
            Assert.NotNull(getSetMethod);

            Assert.NotNull(getSetMethod(typeof(WithProperty).GetPropertyOrNull("P")));
            Assert.Null(getSetMethod(typeof(WithProperty).GetPropertyOrNull("PGet")));
            Assert.Null(getSetMethod(typeof(WithProperty).GetPropertyOrNull("PPrivSet")));
        }

        internal class WithProperty
        {
            public string P { get; set; }
            public string PGet { get { return "hey"; } }
            public string PPrivSet { get; private set; }
        }
    }
}
