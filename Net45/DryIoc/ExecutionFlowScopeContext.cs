namespace DryIoc
{
    using System;
    using System.Runtime.Remoting.Messaging;

    public sealed class ExecutionFlowScopeContext : IScopeContext
    {
        public static readonly object ROOT_SCOPE_NAME = typeof(ExecutionFlowScopeContext);

        public object RootScopeName { get { return ROOT_SCOPE_NAME; } }

        public IScope GetCurrentOrDefault()
        {
            var scope = (Copied<IScope>)CallContext.LogicalGetData(_key);
            return scope == null ? null : scope.Value;
        }

        public void SetCurrent(Func<IScope, IScope> update)
        {
            var oldScope = GetCurrentOrDefault();
            var newScope = update.ThrowIfNull()(oldScope);
            CallContext.LogicalSetData(_key, new Copied<IScope>(newScope));
        }

        #region Implementation

        private static readonly string _key = typeof(ExecutionFlowScopeContext).Name;

        private sealed class Copied<T> : MarshalByRefObject
        {
            public readonly T Value;

            public Copied(T value)
            {
                Value = value;
            }
        }

        #endregion
    }
}
