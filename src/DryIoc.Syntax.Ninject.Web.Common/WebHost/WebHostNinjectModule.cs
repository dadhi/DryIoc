// -------------------------------------------------------------------------------------------------
// <copyright file="WebHostNinjectModule.cs" company="Ninject Project Contributors">
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

using System.Collections.Generic;
using System.Linq;

using System.Web;
using System.Web.Routing;

using Ninject.Activation.Caching;
using Ninject.Infrastructure;
using Ninject.Infrastructure.Disposal;

namespace Ninject.Web.Common.WebHost
{
    /// <summary>Defines the bindings that are common for all ASP.NET web extensions.</summary>
    public class WebHostNinjectModule : GlobalKernelRegistrationModule<OnePerRequestHttpModule>
    {
        /// <summary>Loads the module into the kernel.</summary>
        public override void Load()
        {
            base.Load();
            Bind<RouteCollection>() .ToConstant(RouteTable.Routes);
            Bind<HttpContextBase>() .ToMethod(ctx => new HttpContextWrapper(HttpContext.Current)).InTransientScope();
            Bind<HttpContext>()     .ToMethod(ctx => HttpContext.Current).InTransientScope();
        }
    }

    /// <summary>Provides callbacks to more aggressively collect objects scoped to HTTP requests.</summary>
    public sealed class OnePerRequestHttpModule : GlobalKernelRegistration, IHttpModule
    {
        /// <summary>Initializes a new instance of the <see cref="OnePerRequestHttpModule"/> class.</summary>
        public OnePerRequestHttpModule() => 
            ReleaseScopeAtRequestEnd = true;

        /// <summary>Gets or sets a value indicating whether the request scope shall be released immediately after the request has ended.</summary>
        /// <value>
        ///     <c>true</c> if the request scope shall be released immediately after the request has ended.; otherwise, <c>false</c>.
        /// </value>
        public bool ReleaseScopeAtRequestEnd { get; set; }

        /// <summary>Initializes the module.</summary>
        public void Init(HttpApplication application) =>
            application.EndRequest += (o, e) => DeactivateInstancesForCurrentHttpRequest();

        /// <summary>Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.</summary>
        public void Dispose() {}

        /// <summary>Deactivates instances owned by the current <see cref="HttpContext"/>.</summary>
        public void DeactivateInstancesForCurrentHttpRequest()
        {
            if (ReleaseScopeAtRequestEnd)
            {
                var context = HttpContext.Current;
                MapKernels(kernel => kernel.Components.Get<ICache>().Clear(context));
            }
        }
    }

    /// <summary>Initializes a <see cref="HttpApplication"/> instance.</summary>
    public class HttpApplicationInitializationHttpModule : DisposableObject, IHttpModule
    {
        private readonly Func<IKernel> _lazyKernel;

        /// <summary>Initializes a new instance of the <see cref="HttpApplicationInitializationHttpModule"/> class.</summary>
        public HttpApplicationInitializationHttpModule(Func<IKernel> lazyKernel) => 
            _lazyKernel = lazyKernel;

        /// <summary>Initializes a module and prepares it to handle requests.</summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application.</param>
        public void Init(HttpApplication context) =>
            _lazyKernel().Inject(context);
    }

    /// <summary>Base implementation of <see cref="HttpApplication"/> that adds injection support.</summary>
    public abstract class NinjectHttpApplication : HttpApplication, IHaveKernel
    {
        /// <summary>The one per request module to release request scope at the end of the request.</summary>
        private readonly OnePerRequestHttpModule _onePerRequestHttpModule;

        /// <summary>The _bootstrapper that starts the application.</summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>Initializes a new instance of the <see cref="NinjectHttpApplication"/> class.</summary>
        protected NinjectHttpApplication()
        {
            _onePerRequestHttpModule = new OnePerRequestHttpModule();
            _onePerRequestHttpModule.Init(this);
            _bootstrapper = new Bootstrapper();
        }

        /// <summary>Executes custom initialization code after all event handler modules have been added.</summary>
        public override void Init()
        {
            base.Init();
            _bootstrapper.Kernel.Inject(this);
        }

        /// <summary>Starts the application.</summary>
        public void Application_Start()
        {
            lock (this)
            {
                _bootstrapper.Initialize(CreateKernel);
                _onePerRequestHttpModule.ReleaseScopeAtRequestEnd = _bootstrapper.Kernel.Settings.Get("ReleaseScopeAtRequestEnd", true);
                OnApplicationStarted();
            }
        }

        /// <summary>Releases the kernel on application end.</summary>
        public void Application_End()
        {
            OnApplicationStopped();
            _bootstrapper.ShutDown();
        }

        /// <summary>Creates the kernel that will manage your application.</summary>
        protected abstract IKernel CreateKernel();

        /// <summary>Called when the application is started.</summary>
        protected virtual void OnApplicationStarted() {}

        /// <summary>Called when the application is stopped.</summary>
        protected virtual void OnApplicationStopped() {}
    }

    /// <summary>HttpModule to add support for constructor injection to HttpModules.</summary>
    public sealed class NinjectHttpModule : IHttpModule
    {
        private IList<IHttpModule> _httpModules;

        /// <summary>Initializes a module and prepares it to handle requests.</summary>
        public void Init(HttpApplication context)
        {
            _httpModules = new Bootstrapper().Kernel.GetAll<IHttpModule>().ToList();
            foreach (var httpModule in _httpModules)
                httpModule.Init(context);
        }

        /// <summary>Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.</summary>
        public void Dispose()
        {
            if (_httpModules != null) 
            {
                foreach (var httpModule in _httpModules)
                    httpModule.Dispose();
                _httpModules.Clear();
                _httpModules = null;
            }
        }
    }
}