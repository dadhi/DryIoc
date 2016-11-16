/*
The MIT License (MIT)

Copyright (c) 2013-2016 Maksim Volkau

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

namespace DryIoc.MefAttributedModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>Fake polyfill for the reflection API missing in the PCL profiles.</summary>
    /// <remarks>Actually, PCL-Net45 has CustomAttributeData class, but it's almost empty.</remarks>
    public class CustomAttributeData
    {
        /// <summary>Gets or sets the constructor information.</summary>
        public ConstructorInfo Constructor { get; set; }

        /// <summary>Gets or sets the constructor arguments.</summary>
        public IList<CustomAttributeTypedArgument> ConstructorArguments { get; set; }

        /// <summary>Gets or sets the named arguments.</summary>
        public IList<CustomAttributeNamedArgument> NamedArguments { get; set; }

        /// <summary>Fake polyfill for the reflection API missing in the PCL profiles.</summary>
        public struct CustomAttributeTypedArgument
        {
            /// <summary>Gets or sets the type of the value.</summary>
            public Type ArgumentType { get; set; }

            /// <summary>Gets or sets the value.</summary>
            public object Value { get; set; }
        }

        /// <summary>Fake polyfill for the reflection API missing in the PCL profiles.</summary>
        public struct CustomAttributeNamedArgument
        {
            /// <summary>Gets or sets the argument member information.</summary>
            public MemberInfo MemberInfo { get; set; }

            /// <summary>Gets or sets the value of the argument.</summary>
            public CustomAttributeTypedArgument TypedValue { get; set; }
        }

        /// <summary>Returns an empty data list as the functionaility is not available.</summary>
        /// <param name="type"></param> <returns></returns>
        public static IEnumerable<CustomAttributeData> GetCustomAttributes(Type type)
        {
            return Enumerable.Empty<CustomAttributeData>();
        }
    }
}