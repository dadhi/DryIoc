using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using DryIoc;

[assembly: InternalsVisibleTo(FactoryCompiler.DYNAMIC_ASSEMBLY_NAME)]

namespace DryIoc
{
    public static partial class FactoryCompiler
    {
        public const string DYNAMIC_ASSEMBLY_NAME = "DryIoc.CompiledFactoryProvider.DynamicAssembly";

        static partial void CompileToMethod(Expression<CompiledFactory> factoryExpression, ref CompiledFactory resultFactory)
        {
            resultFactory.ThrowIf(resultFactory != null);

            Interlocked.CompareExchange(ref _moduleBuilder, DefineDynamicModuleBuilder(), null);
            
            var typeName = "Factory" + Interlocked.Increment(ref TypeId);
            var typeBuilder = _moduleBuilder.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Abstract);
            
            var methodBuilder = typeBuilder.DefineMethod("GetService", MethodAttributes.Public | MethodAttributes.Static,
                typeof(object), new[] { typeof(object[]), typeof(Scope) });
            
            factoryExpression.CompileToMethod(methodBuilder);
            
            var dynamicType = typeBuilder.CreateType();
            resultFactory = (CompiledFactory)Delegate.CreateDelegate(typeof(CompiledFactory), dynamicType.GetMethod("GetService"));
        }

        #region Implementation

        private static int TypeId;

        private static ModuleBuilder _moduleBuilder;

        private static ModuleBuilder DefineDynamicModuleBuilder()
        {
            var assemblyName = new AssemblyName(DYNAMIC_ASSEMBLY_NAME);
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName,
                AssemblyBuilderAccess.RunAndCollect);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
            return moduleBuilder;
        }

        #endregion
    }
}
