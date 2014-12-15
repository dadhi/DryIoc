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

namespace System
{
    public sealed class Lazy<T>
    {
        public Lazy(Func<T> valueFactory)
        {
            if (valueFactory == null) throw new ArgumentNullException("valueFactory");
            _valueFactory = valueFactory;
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
                    if (_valueFactory == null) throw new InvalidOperationException("The initialization function tries to access Value on this instance.");
                    var factory = _valueFactory;
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