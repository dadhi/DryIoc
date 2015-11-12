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

        /// <summary>Supports emitting of selected expressions, e.g. lambda are not supported yet.
        /// When emitter find not supported expression it will return false from <see cref="TryVisit"/>, so I could fallback
        /// to normal and slow Expression.Compile.</summary>
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
                        if (paramExpr != Container.StateParamExpr) // Note: Only the state (singletons) is handled
                            return false;
                        il.Emit(OpCodes.Ldarg_0); // state is the first argument
                        Debug.WriteLine("Ldarg_0");
                        return true;
                    case ExpressionType.New:
                        return VisitNew((NewExpression)expr, il);
                    case ExpressionType.NewArrayInit:
                        return VisitNewArray((NewArrayExpression)expr, il);
                    case ExpressionType.MemberInit:
                        return VisitMemberInit((MemberInitExpression)expr, il);
                    case ExpressionType.Call:
                        return VisitMethodCall((MethodCallExpression)expr, il);
                    case ExpressionType.MemberAccess:
                        return VisitMemberAccess((MemberExpression)expr, il);
                    default:
                        // Note: not supported yet
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
                if (!ok) return false;

                var obj = il.DeclareLocal(mi.Type);
                il.Emit(OpCodes.Stloc, obj);
                Debug.WriteLine("Stloc " + obj);

                var bindings = mi.Bindings;
                for (int i = 0, n = bindings.Count; i < n; i++)
                {
                    var binding = bindings[i];
                    if (binding.BindingType != MemberBindingType.Assignment)
                        return false;

                    il.Emit(OpCodes.Ldloc, obj);
                    Debug.WriteLine("Ldloc " + obj);

                    ok = TryVisit(((MemberAssignment)binding).Expression, il);
                    if (!ok) return false;

                    var prop = binding.Member as PropertyInfo;
                    if (prop != null)
                    {
                        var setMethod = prop.GetSetMethodOrNull();
                        if (setMethod == null)
                            return false;
                        EmitMethodCall(setMethod, il);
                    }
                    else
                    {
                        var field = binding.Member as FieldInfo;
                        if (field == null)
                            return false;

                        il.Emit(OpCodes.Stfld, field);
                        Debug.WriteLine("Stfld " + field);
                    }
                }

                il.Emit(OpCodes.Ldloc, obj);
                Debug.WriteLine("Ldloc " + obj);
                return true;
            }

            private static bool VisitMethodCall(MethodCallExpression expr, ILGenerator il)
            {
                var ok = true;
                if (expr.Object != null)
                    ok = TryVisit(expr.Object, il);

                if (ok && expr.Arguments.Count != 0)
                    ok = VisitExpressionList(expr.Arguments, il);

                if (ok)
                    EmitMethodCall(expr.Method, il);

                return ok;
            }

            private static bool VisitMemberAccess(MemberExpression expr, ILGenerator il)
            {
                if (expr.Expression != null)
                {
                    var ok = TryVisit(expr.Expression, il);
                    if (!ok) return false;
                }

                var field = expr.Member as FieldInfo;
                if (field != null)
                {
                    if (field.IsStatic())
                    {
                        il.Emit(OpCodes.Ldsfld, field);
                        Debug.WriteLine("Ldsfld " + field);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldfld, field);
                        Debug.WriteLine("Ldfld " + field);
                    }

                    return true;
                }

                var property = expr.Member as PropertyInfo;
                if (property != null)
                {
                    var getMethod = property.GetGetMethod();
                    if (getMethod == null)
                        return false;
                    EmitMethodCall(getMethod, il);
                }

                return true;
            }

            private static void EmitMethodCall(MethodInfo method, ILGenerator il)
            {
                if (method.IsVirtual)
                {
                    il.Emit(OpCodes.Callvirt, method);
                    Debug.WriteLine("Callvirt " + method);
                }
                else
                {
                    il.Emit(OpCodes.Call, method);
                    Debug.WriteLine("Call " + method);
                }
            }
        }
    }
}
