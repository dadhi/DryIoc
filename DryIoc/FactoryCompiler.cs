using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace DryIoc
{
    public static partial class FactoryCompiler
    {
        [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Result is write only by design")]
        static partial void CompileToDelegate(Expression expression, ref FactoryDelegate result)
        {
            var method = new DynamicMethod("_dryioc_factory",
                MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard,
                typeof(object), 
                new[] { typeof(object[]), typeof(IResolverContext), typeof(IScope) },
                typeof(Container),
                skipVisibility: true);
            var il = method.GetILGenerator();

            var emitted = EmittingVisitor.TryVisit(expression, il);
            if (emitted)
            {
                il.Emit(OpCodes.Ret);
                result = (FactoryDelegate)method.CreateDelegate(typeof(FactoryDelegate));
            }
        }

        private static class EmittingVisitor
        {
            public static bool TryVisit(Expression expr, ILGenerator il)
            {
                switch (expr.NodeType)
                {
                    case ExpressionType.Convert:
                        return VisitConvert((UnaryExpression)expr, il);
                    case ExpressionType.ArrayIndex:
                        return VisitArrayIndex((BinaryExpression)expr, il);
                    case ExpressionType.Constant:
                        return VisitConstant((ConstantExpression)expr, il);
                    case ExpressionType.Parameter:
                        var paramExpr = (ParameterExpression)expr;
                        if (paramExpr != Container.StateParamExpr) // Note: For only state (singletons) is handled
                            return false;
                        il.Emit(OpCodes.Ldarg_0); // state is the first argument
                        Debug.WriteLine("Ldarg_0");
                        return true;
                    case ExpressionType.New:
                        return VisitNew((NewExpression)expr, il);
                    case ExpressionType.NewArrayInit:
                        return VisitNewArray((NewArrayExpression)expr, il);
                    //case ExpressionType.MemberInit:
                    //    return VisitMemberInit((MemberInitExpression)expr, il);
                    default:
                        // not supported nodes
                        return false;
                }
            }

            private static bool VisitBinary(BinaryExpression b, ILGenerator il)
            {
                var ok = TryVisit(b.Left, il);
                if (ok)
                    ok = TryVisit(b.Right, il);
                // skips TryVisit(b.Conversion) for NodeType.Coalesce (?? operation)
                return ok;
            }

            private static bool VisitExpressionList(IList<Expression> eList, ILGenerator state)
            {
                var ok = true;
                for (int i = 0, n = eList.Count; i < n && ok; i++)
                    ok = TryVisit(eList[i], state);
                return ok;
            }

            private static bool VisitConvert(UnaryExpression node, ILGenerator il)
            {
                var ok = TryVisit(node.Operand, il);
                if (ok)
                {
                    var convertTargetType = node.Type;
                    if (convertTargetType != typeof(object)) // cast to object is not required
                    {
                        il.Emit(OpCodes.Castclass, convertTargetType);
                        Debug.WriteLine("Castclass " + convertTargetType);
                    }
                    else
                    {
                        ok = false;
                    }
                }
                return ok;
            }

            private static bool VisitConstant(ConstantExpression node, ILGenerator il)
            {
                var value = node.Value;
                if (value == null)
                {
                    il.Emit(OpCodes.Ldnull);
                    Debug.WriteLine("Ldnull");
                }
                else if (value is int || value.GetType().IsEnum())
                {
                    il.Emit(OpCodes.Ldc_I4, (int)value);
                    Debug.WriteLine("Ldc_I4 " + value);
                }
                else if (value is string)
                {
                    il.Emit(OpCodes.Ldstr, (string)value);
                    Debug.WriteLine("Ldstr " + value);
                }
                else
                {
                    return false;
                }

                return true;
            }

            private static bool VisitNew(NewExpression node, ILGenerator il)
            {
                var ok = VisitExpressionList(node.Arguments, il);
                if (ok)
                {
                    il.Emit(OpCodes.Newobj, node.Constructor);
                    Debug.WriteLine("Newobj " + node.Constructor.DeclaringType);
                }
                return ok;
            }

            private static bool VisitNewArray(NewArrayExpression node, ILGenerator il)
            {
                var elems = node.Expressions;
                var arrType = node.Type;
                var elemType = arrType.GetArrayElementTypeOrNull();
                var isElemOfValueType = elemType.IsValueType();

                var arrVar = il.DeclareLocal(arrType);

                il.Emit(OpCodes.Ldc_I4, elems.Count);
                Debug.WriteLine("Ldc_I4 " + elems.Count);
                il.Emit(OpCodes.Newarr, elemType);
                Debug.WriteLine("Newarr " + elemType);
                il.Emit(OpCodes.Stloc, arrVar);
                Debug.WriteLine("Stloc_0");

                var ok = true;
                for (int i = 0, n = elems.Count; i < n && ok; i++)
                {
                    il.Emit(OpCodes.Ldloc, arrVar);
                    Debug.WriteLine("Ldloc array");
                    
                    il.Emit(OpCodes.Ldc_I4, i);
                    Debug.WriteLine("Ldc_I4 " + i);

                    if (isElemOfValueType)
                    {
                        il.Emit(OpCodes.Ldelema, elemType); // loading element address for later copying of value into it.
                        Debug.WriteLine("Ldelema " + elemType);
                    }

                    ok = TryVisit(elems[i], il);
                    if (ok)
                    {
                        if (isElemOfValueType)
                        {
                            il.Emit(OpCodes.Stobj, elemType); // store element of value type by array element address
                            Debug.WriteLine("Stobj " + elemType);
                        }
                        else
                        {
                            il.Emit(OpCodes.Stelem_Ref);
                            Debug.WriteLine("Stelem_Ref");
                        }
                    }
                }

                il.Emit(OpCodes.Ldloc, arrVar);
                Debug.WriteLine("Ldloc_0");

                return ok;
            }

            private static bool VisitArrayIndex(BinaryExpression node, ILGenerator il)
            {
                var ok = VisitBinary(node, il);
                if (ok)
                {
                    il.Emit(OpCodes.Ldelem_Ref);
                    Debug.WriteLine("Ldelem_Ref");
                }
                return ok;
            }

            private static bool VisitMemberInit(MemberInitExpression mi, ILGenerator il)
            {
                var ok = VisitNew(mi.NewExpression, il);
                if (ok)
                    ok = VisitBindingList(mi.Bindings, il);
                return ok;
            }

            private static bool VisitBindingList(IList<MemberBinding> bindings, ILGenerator il)
            {
                var ok = true;
                for (int i = 0, n = bindings.Count; i < n && ok; i++)
                {
                    var binding = bindings[i];
                    ok = binding.BindingType == MemberBindingType.Assignment
                         && TryVisit(((MemberAssignment)binding).Expression, il);
                }
                return ok;
            }
        }
    }
}
