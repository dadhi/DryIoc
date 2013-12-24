/*
The MIT License (MIT)

Copyright (c) 2013 Maksim Volkau

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

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
