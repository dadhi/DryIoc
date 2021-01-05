// -------------------------------------------------------------------------------------------------
// <copyright file="IResolutionRoot.cs" company="Ninject Project Contributors">
//   Copyright (c) 2007-2010 Enkari, Ltd. All rights reserved.
//   Copyright (c) 2010-2021 Ninject Project Contributors. All rights reserved.
//
//   Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
//   You may not use this file except in compliance with one of the Licenses.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//   or
//       http://www.microsoft.com/opensource/licenses.mspx
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace Ninject.Syntax
{
    using System;
    using System.Collections.Generic;

    using Ninject.Activation;
    using Ninject.Parameters;
    using Ninject.Planning.Bindings;

    /// <summary>Provides a path to resolve instances.</summary>
    public interface IResolutionRoot : IFluentSyntax
    {
        /// <summary>Injects the specified existing instance, without managing its lifecycle.</summary>
        void Inject(object instance, params IParameter[] parameters);

        /// <summary>Determines whether the specified request can be resolved.</summary>
        bool CanResolve(IRequest request);

        /// <summary>Determines whether the specified request can be resolved.</summary>
        bool CanResolve(IRequest request, bool ignoreImplicitBindings);

        /// <summary>Resolves instances for the specified request. The instances are not actually resolved
        /// until a consumer iterates over the enumerator.</summary>
        IEnumerable<object> Resolve(IRequest request);

        /// <summary>Resolves an instance for the specified request.</summary>
        object ResolveSingle(IRequest request);

        /// <summary>Creates a request for the specified service.</summary>
        /// <param name="service">The service that is being requested.</param>
        /// <param name="constraint">The constraint to apply to the bindings to determine if they match the request.</param>
        /// <param name="parameters">The parameters to pass to the resolution.</param>
        /// <param name="isOptional"><see langword="true"/> if the request is optional; otherwise, <see langword="false"/>.</param>
        /// <param name="isUnique"><see langword="true"/> if the request should return a unique result; otherwise, <see langword="false"/>.</param>
        /// <returns>The request for the specified service.</returns>
        IRequest CreateRequest(Type service, Func<IBindingMetadata, bool> constraint, IReadOnlyList<IParameter> parameters, bool isOptional, bool isUnique);

        /// <summary>Deactivates and releases the specified instance if it is currently managed by Ninject.</summary>
        bool Release(object instance);
    }

    /// <summary>The marker</summary>
    public interface IFluentSyntax {}
}

namespace Ninject.Parameters
{
    using System;

    using Ninject.Activation;
    using Ninject.Planning.Targets;

    /// <summary>Modifies an activation process in some way.</summary>
    public interface IParameter : IEquatable<IParameter>
    {
        /// <summary>Gets the name of the parameter.</summary>
        string Name { get; }

        /// <summary>Gets a value indicating whether the parameter should be inherited into child requests.</summary>
        bool ShouldInherit { get; }

        /// <summary>Gets the value for the parameter within the specified context.</summary>
        object GetValue(IContext context, ITarget target);
    }
}