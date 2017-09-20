using System;
using System.ComponentModel.Composition;
using System.Linq.Expressions;
using DryIoc.MefAttributedModel;
using NUnit.Framework;
using DryIocAttributes;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue377_Support_custom_IReuse_with_MEF_attributes
    {
        [Test]
        public void Test()
        {
            var container = new Container().WithMef();
            container.RegisterExports(typeof(A), typeof(B));

            var a = container.Resolve<A>();

            Assert.AreSame(a, container.Resolve<A>());
        }

        [Export, Reuse(typeof(CustomReuse))]
        public class A { }

        /// <summary>Singleton with modified lifespan to pass the check.</summary>
        public class CustomReuse : IReuse
        {
            public static readonly CustomReuse Value = new CustomReuse();

            public int Lifespan { get { return 0; } }

            /// <inheritdoc />
            public object Name { get { return null; } }

            public Expression Apply(Request request, Expression serviceFactoryExpr)
            {
                return Reuse.Singleton.Apply(request,serviceFactoryExpr);
            }

            public bool CanApply(Request request)
            {
                return true;
            }

            private static Expression _valueExpr = Expression.Field(null, typeof(CustomReuse), "Value");
            public Expression ToExpression(Func<object, Expression> fallbackConverter)
            {

                return _valueExpr;
            }

            #region Obsolete

            public IScope GetScopeOrDefault(Request request)
            {
                throw new NotSupportedException();
            }

            public Expression GetScopeExpression(Request request)
            {
                throw new NotSupportedException();
            }

            public int GetScopedItemIdOrSelf(int factoryID, Request request)
            {
                throw new NotSupportedException();
            }

            #endregion
        }
    }
}
