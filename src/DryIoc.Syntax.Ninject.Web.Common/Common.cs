  
// -------------------------------------------------------------------------------------------------
// <copyright file="IBootstrapper.cs" company="Ninject Project Contributors">
//   Copyright (c) 2010-2011 bbv Software Services AG.
//   Copyright (c) 2011-2017 Ninject Project Contributors. All rights reserved.
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

using System;

using Ninject.Infrastructure;
using Ninject.Infrastructure.Language;

namespace Ninject.Web.Common
{
    /// <summary>A basic bootstrapper that can be used to setup web applications.</summary>
    public interface IBootstrapper : IHaveKernel
    {
        /// <summary>Starts the application.</summary>
        void Initialize(Func<IKernel> createKernelCallback);

        /// <summary>Releases the kernel on application end.</summary>
        void ShutDown();
    }

    /// <summary>A basic bootstrapper that can be used to setup web applications.</summary>
    public class Bootstrapper : IBootstrapper
    {
        /// <summary>Gets the kernel.</summary>
        public IKernel Kernel { get; private set; }

        /// <summary>Starts the application.</summary>
        public void Initialize(Func<IKernel> createKernelCallback)
        {
            _kernelInstance = createKernelCallback();
            _kernelInstance.Components.GetAll<INinjectHttpApplicationPlugin>().Map(c => c.Start());
        }

        /// <summary>Releases the kernel on application end.</summary>
        public void ShutDown()
        {
            if (KernelInstance != null)
            {
                KernelInstance.Components.GetAll<INinjectHttpApplicationPlugin>().Map(c => c.Stop());
                KernelInstance.Dispose();
                KernelInstance = null;
            }
        }
    }
}