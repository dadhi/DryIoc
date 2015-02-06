/*
The MIT License (MIT)

Copyright (c) 2014 Maksim Volkau

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

namespace DryIoc
{
    using System;
    using System.Runtime.Remoting.Messaging;
    using System.Diagnostics.CodeAnalysis;

    partial class Container
    {
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        static partial void GetDefaultScopeContext(ref IScopeContext resultContext)
        {
            resultContext = new AsyncExecutionFlowScopeContext();
        }
    }

    public sealed class AsyncExecutionFlowScopeContext : IScopeContext
    {
        public static readonly object ROOT_SCOPE_NAME = typeof(AsyncExecutionFlowScopeContext);

        public object RootScopeName { get { return ROOT_SCOPE_NAME; } }

        public IScope GetCurrentOrDefault()
        {
            var scope = (Copyable<IScope>)CallContext.LogicalGetData(_key);
            return scope == null ? null : scope.Value;
        }

        public IScope SetCurrent(Func<IScope, IScope> getNewCurrentScope)
        {
            var oldScope = GetCurrentOrDefault();
            var newScope = getNewCurrentScope.ThrowIfNull()(oldScope);
            CallContext.LogicalSetData(_key, new Copyable<IScope>(newScope));
            return newScope;
        }

        #region Implementation

        private static readonly string _key = typeof(AsyncExecutionFlowScopeContext).Name;

        [Serializable]
        private sealed class Copyable<T> : MarshalByRefObject
        {
            public readonly T Value;

            public Copyable(T value)
            {
                Value = value;
            }
        }

        #endregion
    }
}
