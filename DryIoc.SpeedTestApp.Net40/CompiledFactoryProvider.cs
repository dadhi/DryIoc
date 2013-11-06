using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;

[assembly: InternalsVisibleTo("DryIoc.CompiledFactoryProvider.DynamicAssembly")]

namespace DryIoc
{
    public static partial class CompiledFactoryProvider
    {
        static partial void CompileToDynamicMethod(Expression<CompiledFactory> factoryExpression, ref CompiledFactory resultFactory)
        {
            Interlocked.CompareExchange(ref _moduleBuilder, DefineDynamicModuleBuilder(), null);
            var typeName = "type" + Interlocked.Increment(ref TypeId);
            var typeBuilder = _moduleBuilder.DefineType(typeName, TypeAttributes.Public);
            var methodBuilder = typeBuilder.DefineMethod("GetService", MethodAttributes.Public | MethodAttributes.Static, 
                typeof(object), new[] { typeof(object[]), typeof(Scope) });
            factoryExpression.CompileToMethod(methodBuilder);
            var dynamicType = typeBuilder.CreateType();
            resultFactory = (CompiledFactory)Delegate.CreateDelegate(typeof(CompiledFactory), 
                dynamicType.GetMethod("GetService"));
        }

        private static int TypeId; 

        private static ModuleBuilder _moduleBuilder;

        private static ModuleBuilder DefineDynamicModuleBuilder()
        {
            var assemblyName = new AssemblyName("DryIoc.CompiledFactoryProvider.DynamicAssembly");
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
            return moduleBuilder;
        }
    }
}
