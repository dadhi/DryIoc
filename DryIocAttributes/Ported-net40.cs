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
    /// <summary>Provides a lazy indirect reference to an object and its associated metadata for use by the Managed Extensibility Framework.</summary>
    /// <typeparam name="T">The type of the service</typeparam>
    /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
    public class Lazy<T, TMetadata> // : Lazy<T> is defined in DryIoc
    {
        /// <summary>Initializes a new instance of the <see cref="Lazy{T, TMetadata}"/> class.</summary>
        /// <param name="valueFactory">The value factory.</param>
        /// <param name="metadata">The metadata.</param>
        /// <exception cref="System.ArgumentNullException">valueFactory</exception>
        public Lazy(Func<T> valueFactory, TMetadata metadata)
        {
            if (valueFactory == null) throw new ArgumentNullException("valueFactory");
            _valueFactory = valueFactory;
            Metadata = metadata;
        }

        /// <summary>Gets the metadata associated with the referenced object.</summary>
        public TMetadata Metadata { get; private set; }

        /// <summary>Indicates if value is computed already, or not.</summary>
        public bool IsValueCreated { get; private set; }

        /// <summary>Computes value if it was not before, and returns it.
        /// Value is guaranteed to be computed only once despite possible thread contention.</summary>
        /// <exception cref="InvalidOperationException">Throws if value computation is recursive.</exception>
        public T Value
        {
            get { return IsValueCreated ? _createdValue : CreateValue(); }
        }

        private Func<T> _valueFactory;

        private T _createdValue;

        private object _valueCreationLock = new object();

        private T CreateValue()
        {
            lock (_valueCreationLock)
            {
                if (!IsValueCreated)
                {
                    if (_valueFactory == null)
                        throw new InvalidOperationException("The initialization function tries to access Value on this instance.");

                    var factory = _valueFactory;
                    _valueFactory = null;
                    _createdValue = factory();
                    IsValueCreated = true;
                }
            }

            return _createdValue;
        }
    }
}