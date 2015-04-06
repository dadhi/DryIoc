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

using System.Linq;
using System.Reflection;
using System.Threading;

namespace DryIoc.Zero
{
    using System;
    using System.Collections.Generic;
    
    public partial class ServiceFactory : IResolverContext, IResolverContextProvider, IDisposable
    {
        /// <summary>Maps Type to <see cref="FactoryDelegate"/>.</summary>
        public static IntKeyTree DefaultResolutions = IntKeyTree.Empty;

        /// <summary>Maps Type to sub-tree of object Service Key to <see cref="FactoryDelegate"/> mappings.</summary>
        public static IntKeyTree KeyedResolutions = IntKeyTree.Empty;

        public ServiceFactory()
        {
            SingletonScope = new Scope();
        }

        public IResolverContext Resolver
        {
            get { return this; }
        }

        public void Dispose()
        {
            SingletonScope.Dispose();
        }

        public IScope SingletonScope { get; private set; }

        public IScope GetCurrentNamedScope(object name, bool throwIfNotFound)
        {
            throw new NotImplementedException();
        }

        public IScope GetOrCreateResolutionScope(ref IScope scope, Type serviceType, object serviceKey)
        {
            return scope ?? (scope = new Scope(name: new TypeValuePair(serviceType, serviceKey)));
        }

        public IScope GetMatchingResolutionScope(IScope scope, Type assignableFromServiceType, object serviceKey, bool outermost,
            bool throwIfNotFound)
        {
            throw new NotImplementedException();
        }

        #region IResolver

        public object ResolveDefault(Type serviceType, IfUnresolved ifUnresolved, IScope scope)
        {
            var factoryDelegate = DefaultResolutions.GetValueOrDefault(serviceType);
            return factoryDelegate != null
                ? factoryDelegate(this, null)
                : GetDefaultOrThrowIfUnresolved(serviceType, ifUnresolved);
        }

        public object ResolveKeyed(Type serviceType, object serviceKey, IfUnresolved ifUnresolved,
            Type requiredServiceType, IScope scope)
        {
            if (serviceKey == null && requiredServiceType == null)
                return ResolveDefault(serviceType, ifUnresolved, scope);

            serviceType = requiredServiceType ?? serviceType;
            var resolutions = KeyedResolutions.GetValueOrDefault(serviceType);
            if (resolutions != null)
            {
                var factoryDelegate = resolutions.GetValueOrDefault(serviceKey);
                if (factoryDelegate != null)
                    return factoryDelegate(this, null);
            }

            return GetDefaultOrThrowIfUnresolved(serviceType, ifUnresolved);
        }

        public IEnumerable<object> ResolveMany(Type serviceType, object serviceKey, Type requiredServiceType,
            object compositeParentKey, IScope scope)
        {
            serviceType = requiredServiceType ?? serviceType;

            var resolutions = KeyedResolutions.GetValueOrDefault(serviceType);
            if (resolutions != null)
            {
                if (serviceKey != null)
                {
                    var factoryDelegate = resolutions.GetValueOrDefault(serviceKey);
                    if (factoryDelegate != null)
                        yield return factoryDelegate(this, scope);
                }
                else
                {
                    foreach (var resolution in resolutions.Enumerate())
                    {
                        var factoryDelegate = resolution.Value;
                        yield return factoryDelegate(this, scope);
                    }
                }
            }
            else
            {
                var factoryDelegate = DefaultResolutions.GetValueOrDefault(serviceType);
                if (factoryDelegate != null)
                    yield return factoryDelegate(this, scope);
            }
        }

        private static object GetDefaultOrThrowIfUnresolved(Type serviceType, IfUnresolved ifUnresolved)
        {
            return ifUnresolved == IfUnresolved.ReturnDefault
                ? null
                : Throw.For<object>(Error.UNABLE_TO_RESOLVE_SERVICE, serviceType);
        }

        #endregion
    }

    public delegate object FactoryDelegate(IResolverContextProvider r, IScope scope);

    /// <summary>Returns reference to actual resolver implementation. 
    /// Minimizes <see cref="FactoryDelegate"/> dependency on container.</summary>
    public interface IResolverContextProvider
    {
        /// <summary>Provides access to resolver implementation.</summary>
        IResolverContext Resolver { get; }
    }

    /// <summary>Provides access to both resolver and scopes to <see cref="FactoryDelegate"/>.</summary>
    public interface IResolverContext : IResolver
    {
        /// <summary>Scope associated with container.</summary>
        IScope SingletonScope { get; }

        /// <summary>Gets current scope matching the <paramref name="name"/>. 
        /// If name is null then current scope is returned, or if there is no current scope then exception thrown.</summary>
        /// <param name="name">May be null</param> <returns>Found scope or throws exception.</returns>
        /// <param name="throwIfNotFound">Says to throw if no scope found.</param>
        IScope GetCurrentNamedScope(object name, bool throwIfNotFound);

        /// <summary>Check if scope is not null, then just returns it, otherwise will create and return it.</summary>
        /// <param name="scope">May be null scope.</param>
        /// <param name="serviceType">Marking scope with resolved service type.</param> 
        /// <param name="serviceKey">Marking scope with resolved service key.</param>
        /// <returns>Input <paramref name="scope"/> ensuring it is not null.</returns>
        IScope GetOrCreateResolutionScope(ref IScope scope, Type serviceType, object serviceKey);

        /// <summary>If both <paramref name="assignableFromServiceType"/> and <paramref name="serviceKey"/> are null, 
        /// then returns input <paramref name="scope"/>.
        /// Otherwise searches scope hierarchy to find first scope with: Type assignable <paramref name="assignableFromServiceType"/> and 
        /// Key equal to <paramref name="serviceKey"/>.</summary>
        /// <param name="scope">Scope to start matching with Type and Key specified.</param>
        /// <param name="assignableFromServiceType">Type to match.</param> <param name="serviceKey">Key to match.</param>
        /// <param name="outermost">If true - commands to look for outermost match instead of nearest.</param>
        /// <param name="throwIfNotFound">Says to throw if no scope found.</param>
        IScope GetMatchingResolutionScope(IScope scope, Type assignableFromServiceType, object serviceKey, bool outermost, bool throwIfNotFound);
    }

    /// <summary>Declares minimal API for service resolution.
    /// The user friendly convenient methods are implemented as extension methods in <see cref="Resolver"/> class.</summary>
    /// <remarks>Resolve default and keyed is separated because of micro optimization for faster resolution.</remarks>
    public interface IResolver
    {
        /// <summary>Resolves service from container and returns created service object.</summary>
        /// <param name="serviceType">Service type to search and to return.</param>
        /// <param name="ifUnresolved">Says what to do if service is unresolved.</param>
        /// <param name="scope">Propagated resolution scope.</param>
        /// <returns>Created service object or default based on <paramref name="ifUnresolved"/> provided.</returns>
        object ResolveDefault(Type serviceType, IfUnresolved ifUnresolved, IScope scope);

        /// <summary>Resolves service from container and returns created service object.</summary>
        /// <param name="serviceType">Service type to search and to return.</param>
        /// <param name="serviceKey">Optional service key used for registering service.</param>
        /// <param name="ifUnresolved">Says what to do if service is unresolved.</param>
        /// <param name="requiredServiceType">Actual registered service type to use instead of <paramref name="serviceType"/>, 
        ///     or wrapped type for generic wrappers.  The type should be assignable to return <paramref name="serviceType"/>.</param>
        /// <param name="scope">Propagated resolution scope.</param>
        /// <returns>Created service object or default based on <paramref name="ifUnresolved"/> provided.</returns>
        /// <remarks>
        /// This method covers all possible resolution input parameters comparing to <see cref="ResolveDefault"/>, and
        /// by specifying the same parameters as for <see cref="ResolveDefault"/> should return the same result.
        /// </remarks>
        object ResolveKeyed(Type serviceType, object serviceKey, IfUnresolved ifUnresolved, Type requiredServiceType, IScope scope);

        /// <summary>Resolve all services registered for specified <paramref name="serviceType"/>, or if not found returns
        /// empty enumerable. If <paramref name="serviceType"/> specified then returns only (single) service registered with
        /// this type. Excludes for result composite parent identified by <paramref name="compositeParentKey"/>.</summary>
        /// <param name="serviceType">Return type of an service item.</param>
        /// <param name="serviceKey">(optional) Resolve only single service registered with the key.</param>
        /// <param name="requiredServiceType">(optional) Actual registered service to search for.</param>
        /// <param name="compositeParentKey">(optional) Parent service key to exclude to support Composite pattern.</param>
        /// <param name="scope">propagated resolution scope, may be null.</param>
        /// <returns>Enumerable of found services or empty. Does Not throw if no service found.</returns>
        IEnumerable<object> ResolveMany(Type serviceType, object serviceKey, Type requiredServiceType, object compositeParentKey, IScope scope);
    }

    public delegate object CreateValueHandler();

    /// <summary>Lazy object storage that will create object with provided factory on first access, 
    /// then will be returning the same object for subsequent access.</summary>
    public interface IScope : IDisposable
    {
        /// <summary>Parent scope in scope stack. Null for root scope.</summary>
        IScope Parent { get; }

        /// <summary>Optional name object associated with scope.</summary>
        object Name { get; }

        /// <summary>Accumulates exceptions thrown by disposed items.</summary>
        Exception[] DisposingExceptions { get; }

        /// <summary>Creates, stores, and returns stored object.</summary>
        /// <param name="id">Unique ID to find created object in subsequent calls.</param>
        /// <param name="createValue">Delegate to create object. It will be used immediately, and reference to delegate will Not be stored.</param>
        /// <returns>Created and stored object.</returns>
        /// <remarks>Scope does not store <paramref name="createValue"/> (no memory leak here), 
        /// it stores only result of <paramref name="createValue"/> call.</remarks>
        object GetOrAdd(int id, CreateValueHandler createValue);

        /// <summary>Sets (replaces) value at specified id, or adds value if no existing id found.</summary>
        /// <param name="id">To set value at.</param> <param name="value">Value to set.</param>
        void SetOrAdd(int id, object value);
    }

    /// <summary><see cref="IScope"/> implementation which will dispose stored <see cref="IDisposable"/> items on its own dispose.
    /// Locking is used internally to ensure that object factory called only once.</summary>
    public sealed class Scope : IScope
    {
        /// <summary>Parent scope in scope stack. Null for root scope.</summary>
        public IScope Parent { get; private set; }

        /// <summary>Optional name object associated with scope.</summary>
        public object Name { get; private set; }

        /// <summary>Returns true if scope disposed.</summary>
        public bool IsDisposed
        {
            get { return _disposed == 1; }
        }

        /// <summary>Accumulates exceptions thrown by disposed items.</summary>
        public Exception[] DisposingExceptions { get; private set; }

        /// <summary>Create scope with optional parent and name.</summary>
        /// <param name="parent">(optional) Parent in scope stack.</param>
        /// <param name="name">(optional) Associated name object.</param>
        public Scope(IScope parent = null, object name = null)
        {
            Parent = parent;
            Name = name;
        }

        /// <summary>Provides access to <see cref="GetOrAdd"/> method for reflection client.</summary>
        public static readonly MethodInfo GetOrAddMethod = typeof(IScope).GetMethod("GetOrAdd");

        /// <summary><see cref="IScope.GetOrAdd"/> for description.
        /// Will throw exception if scope is disposed.</summary>
        /// <param name="id">Unique ID to find created object in subsequent calls.</param>
        /// <param name="factory">Delegate to create object. It will be used immediately, and reference to delegate will Not be stored.</param>
        /// <returns>Created and stored object.</returns>
        public object GetOrAdd(int id, Func<object> factory)
        {
            if (_disposed == 1)
                Throw.It(Error.SCOPE_IS_DISPOSED);

            lock (_syncRoot)
            {
                var item = _items.GetValueOrDefault(id);
                if (item == null ||
                    item is IRecyclable && ((IRecyclable)item).IsRecycled)
                {
                    if (item != null)
                        DisposeItem(item);

                    Throw.If(_disposed == 1, Error.SCOPE_IS_DISPOSED);

                    item = factory();
                    Ref.Swap(ref _items, items => items.AddOrUpdate(id, item));
                }
                return item;
            }
        }

        /// <summary>Sets (replaces) value at specified id, or adds value if no existing id found.</summary>
        /// <param name="id">To set value at.</param> <param name="value">Value to set.</param>
        public void SetOrAdd(int id, object value)
        {
            Throw.If(_disposed == 1, Error.SCOPE_IS_DISPOSED);
            Ref.Swap(ref _items, items => items.AddOrUpdate(id, value));
        }

        /// <summary>Disposes all stored <see cref="IDisposable"/> objects and nullifies object storage.</summary>
        /// <remarks>If item disposal throws exception, then it won't be propagated outside, so the rest of the items could be disposed.
        /// Rather all thrown exceptions are aggregated in <see cref="DisposingExceptions"/> array. If no exceptions, array is null.</remarks>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                return;
            if (!_items.IsEmpty)
                foreach (var item in _items.Enumerate().Select(x => x.Value).Where(x => x is IDisposable || x is IReuseWrapper))
                    DisposeItem(item);
            _items = IntKeyTree.Empty;
        }

        /// <summary>Prints scope info (name and parent) to string for debug purposes.</summary> <returns>String representation.</returns>
        public override string ToString()
        {
            return "named: " +
                (Name == null ? "no" : Name.ToString()) +
                (Parent == null ? "" : " (parent " + Parent + ")");
        }

        #region Implementation

        private IntKeyTree _items = IntKeyTree.Empty;
        private int _disposed;

        // Sync root is required to create object only once. The same reason as for Lazy<T>.
        private readonly object _syncRoot = new object();

        private void DisposeItem(object item)
        {
            try
            {
                var disposable = item as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
                else
                {
                    var reused = item as IReuseWrapper;
                    while (reused != null && !(reused is IHideDisposableFromContainer)
                           && reused.Target != null && (disposable = reused.Target as IDisposable) == null)
                        reused = reused.Target as IReuseWrapper;
                    if (disposable != null)
                        disposable.Dispose();
                }
            }
            catch (Exception ex)
            {
                DisposingExceptions = DisposingExceptions.AppendOrUpdate(ex);
            }
        }

        #endregion
    }

    /// <summary>Defines reused object wrapper.</summary>
    public interface IReuseWrapper
    {
        /// <summary>Wrapped value.</summary>
        object Target { get; }
    }

    /// <summary>Marker interface used by Scope to skip dispose for reused disposable object.</summary>
    public interface IHideDisposableFromContainer { }

    /// <summary>Specifies what to return when <see cref="IResolver"/> unable to resolve service.</summary>
    public enum IfUnresolved
    {
        /// <summary>Specifies to throw exception if no service found.</summary>
        Throw,
        /// <summary>Specifies to return default value instead of throwing error.</summary>
        ReturnDefault
    }

    public sealed class TypeValuePair
    {
        public Type Type;
        public object Value;

        public TypeValuePair(Type type, object value)
        {
            Type = type;
            Value = value;
        }
    }

    // TODO Add Type hash conflicts resolution
    /// <summary>Simple immutable AVL tree with integer keys and object values.</summary>
    public sealed class IntKeyTree
    {
        /// <summary>Empty tree to start with. The <see cref="Height"/> of the empty tree is 0.</summary>
        public static readonly IntKeyTree Empty = new IntKeyTree();

        /// <summary>Key.</summary>
        public readonly int Key;

        /// <summary>Value.</summary>
        public readonly object Value;

        /// <summary>Left subtree/branch, or empty.</summary>
        public readonly IntKeyTree Left;

        /// <summary>Right subtree/branch, or empty.</summary>
        public readonly IntKeyTree Right;

        /// <summary>Height of longest subtree/branch. It is 0 for empty tree, and 1 for single node tree.</summary>
        public readonly int Height;

        /// <summary>Returns true is tree is empty.</summary>
        public bool IsEmpty { get { return Height == 0; } }

        /// <summary>Returns new tree with added or updated value for specified key.</summary>
        /// <param name="key"></param> <param name="value"></param>
        /// <returns>New tree.</returns>
        public IntKeyTree AddOrUpdate(int key, object value)
        {
            return AddOrUpdate(key, value, false);
        }

        /// <summary>Returns new tree with updated value for the key, Or the same tree if key was not found.</summary>
        /// <param name="key"></param> <param name="value"></param>
        /// <returns>New tree if key is found, or the same tree otherwise.</returns>
        public IntKeyTree Update(int key, object value)
        {
            return AddOrUpdate(key, value, true);
        }

        /// <summary>Get value for found key or null otherwise.</summary>
        /// <param name="key"></param> <returns>Found value or null.</returns>
        public object GetValueOrDefault(int key)
        {
            var t = this;
            while (t.Height != 0 && t.Key != key)
                t = key < t.Key ? t.Left : t.Right;
            return t.Height != 0 ? t.Value : null;
        }

        /// <summary>Returns all sub-trees enumerated from left to right.</summary> 
        /// <returns>Enumerated sub-trees or empty if tree is empty.</returns>
        public IEnumerable<IntKeyTree> Enumerate()
        {
            if (Height == 0)
                yield break;

            var parents = new IntKeyTree[Height];

            var tree = this;
            var parentCount = -1;
            while (tree.Height != 0 || parentCount != -1)
            {
                if (tree.Height != 0)
                {
                    parents[++parentCount] = tree;
                    tree = tree.Left;
                }
                else
                {
                    tree = parents[parentCount--];
                    yield return tree;
                    tree = tree.Right;
                }
            }
        }

        #region Implementation

        private IntKeyTree() { }

        private IntKeyTree(int key, object value, IntKeyTree left, IntKeyTree right)
        {
            Key = key;
            Value = value;
            Left = left;
            Right = right;
            Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
        }

        private IntKeyTree AddOrUpdate(int key, object value, bool updateOnly)
        {
            return Height == 0 ? (updateOnly ? this : new IntKeyTree(key, value, Empty, Empty)) // if not found and updateOnly returning current tree.
                : (key == Key ? new IntKeyTree(key, value, Left, Right) // actual update
                : (key < Key
                    ? With(Left.AddOrUpdate(key, value, updateOnly), Right)
                    : With(Left, Right.AddOrUpdate(key, value, updateOnly))).KeepBalanced());
        }

        private IntKeyTree KeepBalanced()
        {
            var delta = Left.Height - Right.Height;
            return delta >= 2 ? With(Left.Right.Height - Left.Left.Height == 1 ? Left.RotateLeft() : Left, Right).RotateRight()
                : (delta <= -2 ? With(Left, Right.Left.Height - Right.Right.Height == 1 ? Right.RotateRight() : Right).RotateLeft()
                : this);
        }

        private IntKeyTree RotateRight()
        {
            return Left.With(Left.Left, With(Left.Right, Right));
        }

        private IntKeyTree RotateLeft()
        {
            return Right.With(With(Left, Right.Left), Right.Right);
        }

        private IntKeyTree With(IntKeyTree left, IntKeyTree right)
        {
            return left == Left && right == Right ? this : new IntKeyTree(Key, Value, left, right);
        }

        #endregion
    }
}
