/*
The MIT License (MIT)

Copyright (c) 2016 Maksim Volkau

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

namespace DryIocZero
{
    using System;
    using System.Runtime.Remoting.Messaging;
    using System.Threading;

    /// <summary>Stores scopes propagating through async-await boundaries.</summary>
    public sealed class AsyncExecutionFlowScopeContext : IScopeContext, IDisposable
    {
        /// <summary>Statically known name of root scope in this context.</summary>
        public static readonly string ScopeContextName = typeof(AsyncExecutionFlowScopeContext).FullName;

        /// <summary>Name associated with context root scope - so the reuse may find scope context.</summary>
        public string RootScopeName { get { return ScopeContextName; } }

        /// <summary>Creates new scope context.</summary>
        public AsyncExecutionFlowScopeContext()
        {
            _currentScopeEntryKey = RootScopeName + Interlocked.Increment(ref _seedKey);
        }

        /// <summary>Returns current scope or null if no ambient scope available at the moment.</summary>
        /// <returns>Current scope or null.</returns>
        public IScope GetCurrentOrDefault()
        {
            var scopeEntry = (ScopeEntry<IScope>)CallContext.LogicalGetData(_currentScopeEntryKey);
            return scopeEntry == null ? null : scopeEntry.Value;
        }

        /// <summary>Changes current scope using provided delegate. Delegate receives current scope as input and  should return new current scope.</summary>
        /// <param name="setCurrentScope">Delegate to change the scope.</param>
        /// <remarks>Important: <paramref name="setCurrentScope"/> may be called multiple times in concurrent environment.
        /// Make it predictable by removing any side effects.</remarks>
        /// <returns>New current scope. So it is convenient to use method in "using (var newScope = ctx.SetCurrent(...))".</returns>
        public IScope SetCurrent(SetCurrentScopeHandler setCurrentScope)
        {
            var oldScope = GetCurrentOrDefault();
            var newScope = setCurrentScope(oldScope);
            var scopeEntry = newScope == null ? null : new ScopeEntry<IScope>(newScope);
            CallContext.LogicalSetData(_currentScopeEntryKey, scopeEntry);
            return newScope;
        }

        /// <summary>Nothing to dispose.</summary>
        public void Dispose() { }

        private static int _seedKey;
        private readonly string _currentScopeEntryKey;

        [Serializable]
        internal sealed class ScopeEntry<T> : MarshalByRefObject
        {
            public readonly T Value;
            public ScopeEntry(T value) { Value = value; }
        }
    }
}
