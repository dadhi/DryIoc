using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using DryIoc;
using DryIoc.WebApi;
using Mega;
using ThreadState = System.Threading.ThreadState;

namespace LoadTest
{

    public class LoadTestBenchmark
    {
        /*
## v4.1.5 - Singleton decorators 

Validation finished
00:01:24.33

ResolveAllControllersOnce of 156 controllers is done in 0.1533677 seconds

----------------------------------
 Starting compiled + cached tests
----------------------------------

New container created

container with ambient ScopeContext DryIoc.AsyncExecutionFlowScopeContext without scope
 with Rules  with Made={FactoryMethod=ConstructorWithResolvableArguments}

ResolveAllControllersOnce of 156 controllers is done in 0.0073486 seconds
ResolveAllControllersOnce of 156 controllers is done in 0.1591292 seconds
-- Starting Load test --
32 Threads.

-- Load Test Finished --
00:00:00.16

New container created

container with ambient ScopeContext DryIoc.AsyncExecutionFlowScopeContext without scope
 with Rules  with Made={FactoryMethod=ConstructorWithResolvableArguments}

ResolveAllControllersOnce of 156 controllers is done in 0.0080617 seconds
ResolveAllControllersOnce of 156 controllers is done in 0.1478801 seconds
-- Starting Randomized Load test --
155 Threads.

-- Randomized Load Finished --
00:00:00.36
*/

        public static IContainer RootContainer = null;

        public static IContainer CreateContainer()
        {
            var config = new HttpConfiguration();
            var container = new Container(rules => rules
                    //.WithoutFastExpressionCompiler()
                    //.WithUseInterpretation()
                    .With(FactoryMethod.ConstructorWithResolvableArguments))
                .WithWebApi(config);

            Registrations.RegisterTypes(container, true);
            RootContainer = container;

            Console.WriteLine("New container created");
            Console.WriteLine("");
            Console.WriteLine(container.ToString());
            Console.WriteLine("");

            return container;
        }

        public static void Start()
        {
            var container = CreateContainer();

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine("Validate started");

            // Validate IoC registrations

            bool IsController(ServiceRegistrationInfo x) => x.ServiceType.Name.EndsWith("Controller");

            // Paralleling the Validation
            //var controllers = container.GetServiceRegistrations()
            //    .Where(IsController).Select(x => x.ToServiceInfo()).ToArray();
            //var controllersPerCpu = new List<ServiceInfo>[Environment.ProcessorCount];
            //for (var i = 0; i < controllers.Length;)
            //    for (var j = 0; j < Environment.ProcessorCount && i < controllers.Length; j++, i++)
            //        (controllersPerCpu[j] ?? (controllersPerCpu[j] = new List<ServiceInfo>())).Add(controllers[i]);
            //var validationTasks = controllersPerCpu
            //    .Select(x => Task.Run(() => container.Validate(x.ToArray()))).ToArray();
            //var results = (await Task.WhenAll(validationTasks))
            //    .SelectMany(x => x).ToArray();

            var results = container.Validate(IsController);
            //var results = container.Validate();
            if (results.Length > 0)
            {
                foreach (var kvp in results)
                {
                    Console.WriteLine("Validation error ServiceType = {0}", kvp.Key.ServiceType.Name);
                    Console.WriteLine(kvp.Value.Message);
                }

                throw new Exception(results.ToString());
            }
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;

            Console.WriteLine("");
            Console.WriteLine("Validation finished");
            Console.WriteLine($"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}");
            Console.WriteLine("");

            // Get Controllers which would normally be used for routing web requests
            var controllers = TestHelper.GetAllControllers();

            // Make sure all controllers can be resolved
            ResolveAllControllersOnce(container, controllers);

            Console.WriteLine("");
            Console.WriteLine("----------------------------------");
            Console.WriteLine(" Starting compiled + cached tests ");
            Console.WriteLine("----------------------------------");
            Console.WriteLine("");

            container = CreateContainer();
            ForceGarbageCollector();
            ResolveAllControllersOnce(container, controllers); // Interpret
            ResolveAllControllersOnce(container, controllers); // Compile, cache
            IterateInOrder(container, controllers);
            container = CreateContainer();
            ForceGarbageCollector();
            ResolveAllControllersOnce(container, controllers); // Interpret
            ResolveAllControllersOnce(container, controllers); // Compile, cache
            StartRandomOrderTest(container, controllers);



            Console.WriteLine("");
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("      Starting cold run tests          ");
            Console.WriteLine("      This can take a long time...     ");
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("");

            container = CreateContainer();
            ForceGarbageCollector();
            IterateInOrder(container, controllers);
            container = CreateContainer();
            ForceGarbageCollector();
            StartRandomOrderTest(container, controllers);
        }

        public static void IterateInOrder(IContainer container, Type[] controllerTypes)
        {
            var threadCount = 32;
            var iterations = 10;
            var i = 0;
            var threads = new Thread[threadCount];

            Console.WriteLine("-- Starting Load test --");
            Console.WriteLine(threadCount + " Threads.");
            // Create threads
            for (i = 0; i < threadCount; i++)
            {
                threads[i] = new Thread(delegate ()
                {
                    var controllers = controllerTypes;
                    var controllersCount = controllers.Length;

                    for (var j = 0; j < iterations; j++)
                    {
                        for (var k = 0; k < controllersCount; k++)
                        {
                            // Simulate WebAPI loop, open scope resolve and repeat
                            using (var scope = container.OpenScope(Reuse.WebRequestScopeName))
                            {
                                scope.Resolve(controllers[k]);
                            }
                        }
                    }
                });
            }


            var stopWatch = new Stopwatch();
            stopWatch.Start();

            // Start all
            for (i = 0; i < threadCount; i++)
            {
                threads[i].Start();
            }

            // Join all
            for (i = 0; i < threadCount; i++)
            {
                threads[i].Join();
            }

            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            Console.WriteLine("");
            Console.WriteLine("-- Load Test Finished --");
            Console.WriteLine($"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}");
            Console.WriteLine("");
        }

        private class LoadTestParams
        {
            /*
         * The results after fixes on my 8750h machine:
         *
         * - Validation finished       - 00:00:27.39
         *
         * - ResolveAllControllersOnce of 156 controllers
         * -- Default               - 1st time: 0.2001231 seconds, 2nd time (cache entry compilation):    4.8156956  seconds (!!! - FEC is 4 times faster)
         * -- WithoutFEC            - 1st time: 0.2052857 seconds, 2nd time (cache entry compilation):    16.4215528 seconds
         * -- WithUseInterpretation - 1st time: 0.20758   seconds, 2nd time (cache entry interpretation): 0.1651708  seconds
         *
         * - Starting compiled + cached tests:
         * -- Load Test Finished       - 00:00:37.35; WithoutFEC: 00:00:33.65; WithUseInterpretation: 00:00:58.54; 
         * -- Randomized Load Finished - 00:07:42.93; WithoutFEC: 00:07:49.95; WithUseInterpretation: 00:08:07.61; 
         *
         * - Starting cold run tests
         * -- Load Test Finished       - 00:00:41.93; WithoutFEC: 00:00:50.20; WithUseInterpretation: 00:00:59.72; 
         * -- Randomized Load Finished - 00:07:53.44; WithoutFEC: 00:08:18.08; WithUseInterpretation: 00:08:20.03; 
         */
            public int iterations;
            public int threadNum;
            public Type[] controllerTypes;
            public IContainer container;
        }

        public static void ParaetrizedLoop(object param)
        {
            var p = (LoadTestParams)param;
            int controllerCount = p.controllerTypes.Length;

            for (var j = 0; j < p.iterations; j++)
            {
                for (var k = 0; k < controllerCount; k++)
                {
                    // Simulate WebAPI loop, open scope resolve and repeat
                    using (var scope = p.container.OpenScope(Reuse.WebRequestScopeName))
                    {
                        int index = (p.threadNum + k) % controllerCount; // Make sure threads start at different types
                        scope.Resolve(p.controllerTypes[index]);
                    }
                }
            }
        }

        private static Thread[] _threads;

        public static void StartRandomOrderTest(IContainer container, Type[] controllerTypes)
        {
            var threadCount = controllerTypes.Length - 1;
            var iterations = 10;
            int i;
            _threads = new Thread[threadCount];

            Console.WriteLine("-- Starting Randomized Load test -- ");
            Console.WriteLine(threadCount + " Threads.");

            // Create threads
            for (i = 0; i < threadCount; i++)
            {
                _threads[i] = new Thread(new ParameterizedThreadStart(ParaetrizedLoop));
            }


            var stopWatch = new Stopwatch();
            stopWatch.Start();
            Random rnd = new Random();

            // Start all
            for (i = 0; i < threadCount; i++)
            {
                _threads[i].Start
                (
                    new LoadTestParams()
                    {
                        container = container,
                        controllerTypes = controllerTypes,
                        iterations = iterations,
                        threadNum = rnd.Next(0, threadCount)
                    }
                );
            }

            // Poll thread status
            var aTimer = new System.Timers.Timer();
            aTimer.Interval = 15000;

            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += CheckThreadStatus;
            aTimer.Enabled = true;

            // Join all
            for (i = 0; i < threadCount; i++)
            {
                _threads[i].Join();
            }
            aTimer.Stop();
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            var ts = stopWatch.Elapsed;
            Console.WriteLine("");
            Console.WriteLine("-- Randomized Load Finished --");
            Console.WriteLine($"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}");
            Console.WriteLine("");
        }

        // Map all statuses => counter
        private static Dictionary<System.Threading.ThreadState, int> ThreadStatuses = new Dictionary<System.Threading.ThreadState, int>()
        {
            {ThreadState.Running, 0},
            {ThreadState.StopRequested, 0},
            {ThreadState.SuspendRequested, 0},
            {ThreadState.Background, 0},
            {ThreadState.Unstarted, 0},
            {ThreadState.Stopped, 0},
            {ThreadState.WaitSleepJoin, 0},
            {ThreadState.Suspended, 0},
            {ThreadState.AbortRequested, 0},
            {ThreadState.Aborted, 0}
        };

        private static void CheckThreadStatus(Object source, System.Timers.ElapsedEventArgs e)
        {
            // Clear counts
            ThreadStatuses[ThreadState.Running] = 0;
            ThreadStatuses[ThreadState.StopRequested] = 0;
            ThreadStatuses[ThreadState.SuspendRequested] = 0;
            ThreadStatuses[ThreadState.Background] = 0;
            ThreadStatuses[ThreadState.Unstarted] = 0;
            ThreadStatuses[ThreadState.Stopped] = 0;
            ThreadStatuses[ThreadState.WaitSleepJoin] = 0;
            ThreadStatuses[ThreadState.Suspended] = 0;
            ThreadStatuses[ThreadState.AbortRequested] = 0;
            ThreadStatuses[ThreadState.Aborted] = 0;

            for (var i = 0; i < _threads.Length; i++)
            {
                var thread = _threads[i];
                int j;
                ThreadStatuses.TryGetValue(thread.ThreadState, out j);

                ThreadStatuses[thread.ThreadState] = ++j;
            }

            Console.WriteLine("");
            Console.WriteLine("Thread status check:");

            foreach (var keyValuePair in ThreadStatuses)
            {
                if (keyValuePair.Value > 0)
                {
                    Console.WriteLine(keyValuePair.Value + " threads are " + keyValuePair.Key);
                }
            }

            Console.WriteLine("");
        }

        static void ResolveAllControllersOnce(IContainer container, Type[] controllers)
        {
            var sw = Stopwatch.StartNew();
            using (var scope = container.OpenScope(Reuse.WebRequestScopeName))
            {
                foreach (var controller in controllers)
                {
                    scope.Resolve(controller);
                }
            }
            sw.Stop();
            Console.WriteLine($"ResolveAllControllersOnce of {controllers.Length} controllers is done in {sw.Elapsed.TotalSeconds} seconds");
        }

        static void ForceGarbageCollector()
        {
            GC.Collect(0, GCCollectionMode.Forced, true);
            GC.Collect(1, GCCollectionMode.Forced, true);
            GC.Collect(2, GCCollectionMode.Forced, true);
        }
    }
}
