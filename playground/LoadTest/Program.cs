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
using ThreadState = System.Threading.ThreadState;

namespace LoadTest
{
    public class Program
    {
        public static IContainer RootContainer = null;

        public static IContainer CreateContainer()
        {
            var config = new HttpConfiguration();
            var container = new Container(rules => rules
                // With UseInterpretation it completes without error in 28 sec
                .WithoutFastExpressionCompiler()
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

        static void Main(string[] args)
        {
            Console.WriteLine("Starting up!");

            var container = CreateContainer();

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine("Validate started");

            // Validate IoC registrations
            var results = container.Validate();
            if (results.Length > 0)
            {
                throw new Exception(results.ToString());
            }
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;

            Console.WriteLine("");
            Console.WriteLine("Validation finished");
            Console.WriteLine($"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}");
            Console.WriteLine("");

            var httpControllerType = typeof(IHttpController);

            // Get Controllers which would normally be used for routing web requests
            var controllers = Assembly.GetExecutingAssembly().GetLoadedTypes()
                .Where((t) =>
                    !t.IsAbstract && !t.IsInterface && !t.Name.Contains("Base") &&
                    httpControllerType.IsAssignableFrom(t))
                .ToArray();

            // Make sure all controllers can be resolved
            ResolveAllControllersOnce(container, controllers);

            Console.WriteLine("");
            Console.WriteLine("----------------------------------");
            Console.WriteLine(" Starting compiled + cached tests ");
            Console.WriteLine("----------------------------------");
            Console.WriteLine("");

            container = CreateContainer();
            ForceGarbageCollector();
            ResolveAllControllersOnce(container, controllers); // Intepret
            ResolveAllControllersOnce(container, controllers); // Compile, cache
            IterateInOrder(controllers, container);
            container = CreateContainer();
            ForceGarbageCollector();
            ResolveAllControllersOnce(container, controllers); // Intepret
            ResolveAllControllersOnce(container, controllers); // Compile, cache
            StartRandomOrderTest(controllers, container);



            Console.WriteLine("");
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("      Starting cold run tests          ");
            Console.WriteLine("      This can take a long time...     ");
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("");

            container = CreateContainer();
            ForceGarbageCollector();
            IterateInOrder(controllers, container);
            container = CreateContainer();
            ForceGarbageCollector();
            StartRandomOrderTest(controllers, container);
        }

        public static void IterateInOrder(Type[] controllerTypes, IContainer container)
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
            public int iterations;
            public int threadNum;
            public Type[] controllerTypes;
            public IContainer container;
        }

        public static void ParaetrizedLoop(object param)
        {
            LoadTestParams p = (LoadTestParams)param;
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

        public static void StartRandomOrderTest(Type[] controllerTypes, IContainer container)
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
            using (var scope = container.OpenScope(Reuse.WebRequestScopeName))
            {
                foreach (var controller in controllers)
                {
                    scope.Resolve(controller);
                }
            }
        }

        static void ForceGarbageCollector()
        {
            GC.Collect(0, GCCollectionMode.Forced, true);
            GC.Collect(1, GCCollectionMode.Forced, true);
            GC.Collect(2, GCCollectionMode.Forced, true);
        }
    }
}