using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;

namespace DryIoc.Playground
{
    [SecurityCritical]
    internal static class CommonReflectionUtil {
 
        public static void Assert(bool b) {
            if (!b) { 
                throw new PlatformNotSupportedException(); 
            }
        } 

        public static ConstructorInfo FindConstructor(Type type, bool isStatic, Type[] argumentTypes) {
            ConstructorInfo ctor = type.GetConstructor(GetBindingFlags(isStatic), null, argumentTypes, null);
            Assert(ctor != null); 
            return ctor;
        } 
 
        public static FieldInfo FindField(Type containingType, string fieldName, bool isStatic, Type fieldType) {
            FieldInfo field = containingType.GetField(fieldName, GetBindingFlags(isStatic)); 
            Assert(field.FieldType == fieldType);
            return field;
        }
 
        public static MethodInfo FindMethod(Type containingType, string methodName, bool isStatic, Type[] argumentTypes, Type returnType) {
            MethodInfo method = containingType.GetMethod(methodName, GetBindingFlags(isStatic), null, argumentTypes, null); 
            Assert(method.ReturnType == returnType); 
            return method;
        } 

        private static BindingFlags GetBindingFlags(bool isStatic) {
            return ((isStatic) ? BindingFlags.Static : BindingFlags.Instance)
                | BindingFlags.NonPublic | BindingFlags.Public; 
        }
 
        public static T MakeDelegate<T>(MethodInfo method) where T : class { 
            return MakeDelegate<T>(null /* target */, method);
        } 

        public static T MakeDelegate<T>(object target, MethodInfo method) where T : class {
            return MakeDelegate(typeof(T), target, method) as T;
        } 

        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "This is contained within a fully critical type, and we carefully control the callers.")] 
        [ReflectionPermission(SecurityAction.Assert, MemberAccess = true)] // allow delegate creation to private members 
        public static Delegate MakeDelegate(Type delegateType, object target, MethodInfo method) {
            return Delegate.CreateDelegate(delegateType, target, method); 
        }

        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "This is contained within a fully critical type, and we carefully control the callers.")]
        [ReflectionPermission(SecurityAction.Assert, MemberAccess = true)] // field access doesn't have side effects like executing user code 
        public static object ReadField(FieldInfo fieldInfo, object target) {
            return fieldInfo.GetValue(target); 
        } 

        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "This is contained within a fully critical type, and we carefully control the callers.")] 
        [ReflectionPermission(SecurityAction.Assert, MemberAccess = true)] // field access doesn't have side effects like executing user code
        public static void WriteField(FieldInfo fieldInfo, object target, object value) {
            fieldInfo.SetValue(target, value);
        } 

        // Returns a function that's basically obj => Delegate.CreateDelegate(typeof(TDelegate), obj, methodInfo). 
        // Useful if you need to perform the equivalent of CreateDelegate in a tight loop, as CreateDelegate 
        // is somewhat slow. The provided MethodInfo must be an instance method, otherwise calling the returned
        // delegate will fail at JIT time. We need to assert permission to create the delegate, but by using 
        // a delegate instead of MethodInfo.Invoke directly the delegate will execute with the permission set
        // of its immediate caller.
        //
        // Sample usage: MakeFastCreateDelegate<string, Func<string, bool>>(String::Contains) 
        [ReflectionPermission(SecurityAction.Assert, MemberAccess = true)]
        public static Func<TInstance, TDelegate> MakeFastCreateDelegate<TInstance, TDelegate>(MethodInfo methodInfo) 
            where TInstance : class 
            where TDelegate : class {
            DynamicMethod dynamicMethod = new DynamicMethod( 
                name: "FastCreateDelegate_" + methodInfo.Name,
                returnType: typeof(TDelegate),
                parameterTypes: new Type[] { typeof(TInstance) },
                restrictedSkipVisibility: true); 

            ConstructorInfo delegateCtor = typeof(TDelegate).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }); 
 
            // To quickly create a delegate, call its .ctor(object, IntPtr).
            // The IntPtr passed in is the result of calling LDVIRTFTN on the target method. 
            // C# compiler always uses LDVIRTFTN for instance methods, even for non-virtual methods.

            ILGenerator ilGen = dynamicMethod.GetILGenerator();
 
            ilGen.Emit(OpCodes.Ldarg_0); // Stack contains 'this' parameter
            ilGen.Emit(OpCodes.Dup); // Stack contains ('this', 'this') 
            ilGen.Emit(OpCodes.Ldvirtftn, methodInfo); // Virtual method lookup on 'this', stack now contains ('this', native function pointer) 
            ilGen.Emit(OpCodes.Newobj, delegateCtor); // Delegate creation, stack now contains just the delegate
            ilGen.Emit(OpCodes.Ret); 

            return (Func<TInstance, TDelegate>)dynamicMethod.CreateDelegate(typeof(Func<TInstance, TDelegate>));
        }
 
        // Returns a function that's basically (obj, p1, p2, ...) => obj.Method(p1, p2, ...).
        // Similar to MakeFastCreateDelegate, but useful for scenarios where using MakeFastCreateDelegate would incur heavy 
        // CAS performance penalties. See that method for security notes. 
        //
        // Sample usage: BindMethodToDelegate<Action<NameObjectCollectionBase, string, object>>(NameObjectCollectionBase::BaseAdd) 
        [ReflectionPermission(SecurityAction.Assert, MemberAccess = true)]
        public static TDelegate BindMethodToDelegate<TDelegate>(MethodInfo methodInfo)
            where TDelegate : class {
 
            // Need to extract the delegate's argument types
            Type[] delArgumentTypes; 
            Type delReturnType; 
            ExtractDelegateSignature(typeof(TDelegate), out delArgumentTypes, out delReturnType);
 
            DynamicMethod dynamicMethod = new DynamicMethod(
                name: "BindMethodToDelegate_" + methodInfo.Name,
                returnType: delReturnType,
                parameterTypes: delArgumentTypes, 
                restrictedSkipVisibility: true);
 
            // C# compiler always uses LDVIRTFTN for instance methods, even for non-virtual methods. 

            ILGenerator ilGen = dynamicMethod.GetILGenerator(); 

            for (int i = 0; i < delArgumentTypes.Length; i++) {
                ilGen.Emit(OpCodes.Ldarg, (short)i); // push this argument onto the stack
            } 
            ilGen.Emit(OpCodes.Callvirt, methodInfo); // Method call, stack now contains the return value of the method call
            ilGen.Emit(OpCodes.Ret); 
 
            return dynamicMethod.CreateDelegate(typeof(TDelegate)) as TDelegate;
        } 

        // Returns a function that's basically (p1, p2, ...) => new T(p1, p2, ...).
        // Useful if you need to create objects in a tight loop, as ConstructorInfo.Invoke is somewhat slow.
        // The provided Type must have an instance constructor matching the delegate signature, otherwise 
        // compilation will fail. We need to assert permission to create the delegate, but by using
        // a delegate instead of MethodInfo.Invoke directly the delegate will execute with the permission set 
        // of its immediate caller. 
        //
        // Sample usage: MakeFastNewObject<Func<char[], string>>(typeof(String)) 
        [ReflectionPermission(SecurityAction.Assert, MemberAccess = true)]
        public static TDelegate MakeFastNewObject<TDelegate>(Type type)
            where TDelegate : class {
 
            // Need to extract the delegate's argument types
            Type[] delArgumentTypes; 
            Type delReturnType; 
            ExtractDelegateSignature(typeof(TDelegate), out delArgumentTypes, out delReturnType);
 
            // Locate the target constructor
            ConstructorInfo ctorInfo = CommonReflectionUtil.FindConstructor(
                type: type,
                isStatic: false, 
                argumentTypes: delArgumentTypes);
 
            DynamicMethod dynamicMethod = new DynamicMethod( 
                name: "MakeFastNewObject_" + type.Name,
                returnType: delReturnType, 
                parameterTypes: delArgumentTypes,
                restrictedSkipVisibility: true);

            ILGenerator ilGen = dynamicMethod.GetILGenerator(); 

            for (int i = 0; i < delArgumentTypes.Length; i++) { 
                ilGen.Emit(OpCodes.Ldarg, (short)i); // push all delegate arguments onto the stack 
            }
            ilGen.Emit(OpCodes.Newobj, ctorInfo); // instantiate the target object, stack now contains just the object 
            ilGen.Emit(OpCodes.Ret);

            return dynamicMethod.CreateDelegate(typeof(TDelegate)) as TDelegate;
        } 

        // Breaks a delegate down into its argument types and return type 
        private static void ExtractDelegateSignature(Type delegateType, out Type[] argumentTypes, out Type returnType) { 
            MethodInfo delInvokeMethod = delegateType.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            argumentTypes = Array.ConvertAll(delInvokeMethod.GetParameters(), pInfo => pInfo.ParameterType); 
            returnType = delInvokeMethod.ReturnType;
        }

    }
}
