using System;
using System.Collections.Generic;
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
            var method = new DynamicMethod(string.Empty,
                MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard,
                typeof(object), _factoryDelegateArgTypes,
                typeof(Container), skipVisibility: true);

            var il = method.GetILGenerator();

            var vars = ImTreeMap<int, LocalBuilder>.Empty;
            var emitted = EmittingVisitor.TryVisit(expression, il, ref vars);
            if (emitted)
            {
                il.Emit(OpCodes.Ret);
                result = (FactoryDelegate)method.CreateDelegate(typeof(FactoryDelegate));
            }
        }

        private static readonly Type[] _factoryDelegateArgTypes = new[] { typeof(object[]), typeof(IResolverContext), typeof(IScope) };

        /// <summary>Supports emitting of selected expressions, e.g. lambda are not supported yet.
        /// When emitter find not supported expression it will return false from <see cref="TryVisit"/>, so I could fallback
        /// to normal and slow Expression.Compile.</summary>
        private static class EmittingVisitor
        {
            public static bool TryVisit(Expression expr, ILGenerator il, ref ImTreeMap<int, LocalBuilder> vars)
            {
                switch (expr.NodeType)
                {
                    case ExpressionType.Convert:
                        return VisitConvertAndUseStateVarIfDeclared(expr, il, ref vars);
                    case ExpressionType.ArrayIndex:
                        return VisitArrayIndex((BinaryExpression)expr, il, ref vars);
                    case ExpressionType.Constant:
                        return VisitConstant((ConstantExpression)expr, il);
                    case ExpressionType.Parameter:
                        return VisitFactoryDelegateParameters(expr, il);
                    case ExpressionType.New:
                        return VisitNew((NewExpression)expr, il, ref vars);
                    case ExpressionType.NewArrayInit:
                        return VisitNewArray((NewArrayExpression)expr, il, ref vars);
                    case ExpressionType.MemberInit:
                        return VisitMemberInit((MemberInitExpression)expr, il, ref vars);
                    case ExpressionType.Call:
                        return VisitMethodCall((MethodCallExpression)expr, il, ref vars);
                    case ExpressionType.MemberAccess:
                        return VisitMemberAccess((MemberExpression)expr, il, ref vars);
                    default:
                        // Not supported yet: nested lambdas (Invoke)
                        return false;
                }
            }

            private static bool VisitFactoryDelegateParameters(Expression expr, ILGenerator il)
            {
                var paramExpr = (ParameterExpression)expr;
                if (paramExpr == Container.StateParamExpr)
                    il.Emit(OpCodes.Ldarg_0);
                else if (paramExpr == Container.ResolverContextParamExpr)
                    il.Emit(OpCodes.Ldarg_1);
                else if (paramExpr == Container.ResolutionScopeParamExpr)
                    il.Emit(OpCodes.Ldarg_2);
                return true;
            }

            private static bool VisitConvertAndUseStateVarIfDeclared(Expression expr, ILGenerator il, ref ImTreeMap<int, LocalBuilder> vars)
            {
                var convertExpr = (UnaryExpression)expr;
                var stateItemIndex = -1;
                if (convertExpr.Operand.NodeType == ExpressionType.ArrayIndex)
                {
                    var operandExpr = (BinaryExpression)convertExpr.Operand;
                    if (operandExpr.Left == Container.StateParamExpr)
                        stateItemIndex = (int)((ConstantExpression)operandExpr.Right).Value;
                }

                if (stateItemIndex != -1)
                {
                    var itemVar = vars.GetValueOrDefault(stateItemIndex);
                    if (itemVar != null)
                    {
                        il.Emit(OpCodes.Ldloc, itemVar);
                        return true;
                    }
                }

                var ok = VisitConvert(convertExpr, il, ref vars);
                if (ok && stateItemIndex != -1)
                {
                    var itemVar = il.DeclareLocal(convertExpr.Type);
                    vars = vars.AddOrUpdate(stateItemIndex, itemVar);
                    il.Emit(OpCodes.Stloc, itemVar);
                    il.Emit(OpCodes.Ldloc, itemVar);
                }
                return ok;
            }

            private static bool VisitBinary(BinaryExpression b, ILGenerator il, ref ImTreeMap<int, LocalBuilder> vars)
            {
                var ok = TryVisit(b.Left, il, ref vars);
                if (ok)
                    ok = TryVisit(b.Right, il, ref vars);
                // skips TryVisit(b.Conversion) for NodeType.Coalesce (?? operation)
                return ok;
            }

            private static bool VisitExpressionList(IList<Expression> eList, ILGenerator state, ref ImTreeMap<int, LocalBuilder> vars)
            {
                var ok = true;
                for (int i = 0, n = eList.Count; i < n && ok; i++)
                    ok = TryVisit(eList[i], state, ref vars);
                return ok;
            }

            private static bool VisitConvert(UnaryExpression node, ILGenerator il, ref ImTreeMap<int, LocalBuilder> vars)
            {
                var ok = TryVisit(node.Operand, il, ref vars);
                if (ok)
                {
                    var convertTargetType = node.Type;
                    if (convertTargetType == typeof(object)) // not supported, probably required for converting ValueType
                        return false;
                    il.Emit(OpCodes.Castclass, convertTargetType);
                }
                return ok;
            }

            private static bool VisitConstant(ConstantExpression node, ILGenerator il)
            {
                var value = node.Value;
                if (value == null)
                    il.Emit(OpCodes.Ldnull);
                else if (value is int || value.GetType().IsEnum())
                    il.Emit(OpCodes.Ldc_I4, (int)value);
                else if (value is string)
                    il.Emit(OpCodes.Ldstr, (string)value);
                else
                    return false;
                return true;
            }

            private static bool VisitNew(NewExpression node, ILGenerator il, ref ImTreeMap<int, LocalBuilder> vars)
            {
                var ok = VisitExpressionList(node.Arguments, il, ref vars);
                if (ok)
                    il.Emit(OpCodes.Newobj, node.Constructor);
                return ok;
            }

            private static bool VisitNewArray(NewArrayExpression node, ILGenerator il, ref ImTreeMap<int, LocalBuilder> vars)
            {
                var elems = node.Expressions;
                var arrType = node.Type;
                var elemType = arrType.GetArrayElementTypeOrNull();
                var isElemOfValueType = elemType.IsValueType();

                var arrVar = il.DeclareLocal(arrType);

                il.Emit(OpCodes.Ldc_I4, elems.Count);
                il.Emit(OpCodes.Newarr, elemType);
                il.Emit(OpCodes.Stloc, arrVar);

                var ok = true;
                for (int i = 0, n = elems.Count; i < n && ok; i++)
                {
                    il.Emit(OpCodes.Ldloc, arrVar);
                    il.Emit(OpCodes.Ldc_I4, i);

                    // loading element address for later copying of value into it.
                    if (isElemOfValueType)
                        il.Emit(OpCodes.Ldelema, elemType);

                    ok = TryVisit(elems[i], il, ref vars);
                    if (ok)
                    {
                        if (isElemOfValueType)
                            il.Emit(OpCodes.Stobj, elemType); // store element of value type by array element address
                        else
                            il.Emit(OpCodes.Stelem_Ref);
                    }
                }

                il.Emit(OpCodes.Ldloc, arrVar);
                return ok;
            }

            private static bool VisitArrayIndex(BinaryExpression node, ILGenerator il, ref ImTreeMap<int, LocalBuilder> vars)
            {
                var ok = VisitBinary(node, il, ref vars);
                if (ok)
                    il.Emit(OpCodes.Ldelem_Ref);
                return ok;
            }

            private static bool VisitMemberInit(MemberInitExpression mi, ILGenerator il, ref ImTreeMap<int, LocalBuilder> vars)
            {
                var ok = VisitNew(mi.NewExpression, il, ref vars);
                if (!ok) return false;

                var obj = il.DeclareLocal(mi.Type);
                il.Emit(OpCodes.Stloc, obj);

                var bindings = mi.Bindings;
                for (int i = 0, n = bindings.Count; i < n; i++)
                {
                    var binding = bindings[i];
                    if (binding.BindingType != MemberBindingType.Assignment)
                        return false;
                    il.Emit(OpCodes.Ldloc, obj);

                    ok = TryVisit(((MemberAssignment)binding).Expression, il, ref vars);
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
                    }
                }

                il.Emit(OpCodes.Ldloc, obj);
                return true;
            }

            private static bool VisitMethodCall(MethodCallExpression expr, ILGenerator il, ref ImTreeMap<int, LocalBuilder> vars)
            {
                var ok = true;
                if (expr.Object != null)
                    ok = TryVisit(expr.Object, il, ref vars);

                if (ok && expr.Arguments.Count != 0)
                    ok = VisitExpressionList(expr.Arguments, il, ref vars);

                if (ok)
                    EmitMethodCall(expr.Method, il);

                return ok;
            }

            private static bool VisitMemberAccess(MemberExpression expr, ILGenerator il, ref ImTreeMap<int, LocalBuilder> vars)
            {
                if (expr.Expression != null)
                {
                    var ok = TryVisit(expr.Expression, il, ref vars);
                    if (!ok) return false;
                }

                var field = expr.Member as FieldInfo;
                if (field != null)
                {
                    il.Emit(field.IsStatic() ? OpCodes.Ldsfld : OpCodes.Ldfld, field);
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
                il.Emit(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method);
            }
        }
    }
}
