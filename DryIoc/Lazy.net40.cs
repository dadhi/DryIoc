using System;
using System.Diagnostics;

namespace DryIoc
{
    [DebuggerStepThrough]
    [DebuggerDisplay("{Value}")]
    public sealed class Lazy<T>
    {
        public Lazy(Func<T> valueFactory)
        {
            _valueFactory = valueFactory.ThrowIfNull();
        }

        public bool IsValueCreated { get; private set; }

        public T Value
        {
            get { return IsValueCreated ? _value : Create(); }
        }

        #region Implementation

        private Func<T> _valueFactory;
        private T _value;
        private readonly object _valueCreationLock = new object();

        private T Create()
        {
            lock (_valueCreationLock)
            {
                if (!IsValueCreated)
                {
                    var factory = _valueFactory.ThrowIfNull("Recursive creation of Lazy value is detected.");
                    _valueFactory = null;
                    _value = factory();
                    IsValueCreated = true;
                }
            }

            return _value;
        }

        #endregion
    }
}